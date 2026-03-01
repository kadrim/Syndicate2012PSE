using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace ME3Server_WV
{
    /// <summary>
    /// Handles SSLv3 connections using BouncyCastle's managed TLS implementation.
    /// Required because the game's ProtoSSL uses SSLv3 with RC4 ciphers, which
    /// modern TLS libraries no longer support.
    /// </summary>
    public static class SSLv3Handler
    {
        private static Pkcs12Store _store;
        private static string _alias;
        private static bool _loaded;

        /// <summary>
        /// Loads the server certificate from a PFX file for SSLv3 connections.
        /// </summary>
        public static void LoadCertificate(string pfxPath, string password)
        {
            _store = new Pkcs12StoreBuilder().Build();
            using (var fs = new FileStream(pfxPath, FileMode.Open, FileAccess.Read))
            {
                _store.Load(fs, password?.ToCharArray() ?? Array.Empty<char>());
            }

            foreach (string alias in _store.Aliases)
            {
                if (_store.IsKeyEntry(alias))
                {
                    _alias = alias;
                    break;
                }
            }

            if (_alias == null)
                throw new InvalidOperationException("No private key entry found in PFX: " + pfxPath);

            var certChain = _store.GetCertificateChain(_alias);
            if (certChain != null)
            {
                foreach (var entry in certChain)
                    Logger.Log("[SSLv3] Certificate: " + entry.Certificate.SubjectDN, LogColor.Gray, 3);
            }

            _loaded = true;
            Logger.Log("[SSLv3] Certificate loaded from " + pfxPath, LogColor.Black);
        }

        /// <summary>
        /// Wraps a raw network stream with SSLv3, performing the server-side handshake.
        /// Returns the decrypted stream for reading/writing application data.
        /// </summary>
        public static Stream AcceptSSLv3(Stream innerStream)
        {
            if (!_loaded)
                throw new InvalidOperationException("Certificate not loaded. Call LoadCertificate first.");

            var protocol = new TlsServerProtocol(innerStream);
            try
            {
                protocol.Accept(new SSLv3TlsServer(_store, _alias));
            }
            catch (Exception e)
            {
                Logger.Log("[SSLv3] Handshake failed: " + e.Message, LogColor.Red);
                Logger.Log("[SSLv3] Stack trace: " + e.StackTrace, LogColor.Red, 5);
                if (e.InnerException != null)
                    Logger.Log("[SSLv3] Inner: " + e.InnerException.Message, LogColor.Red, 5);
                throw;
            }
            return protocol.Stream;
        }
    }

    /// <summary>
    /// BouncyCastle TLS server configured for SSLv3 with RC4 cipher suites
    /// compatible with the game client's ProtoSSL implementation.
    /// </summary>
    internal class SSLv3TlsServer : DefaultTlsServer
    {
        private readonly Pkcs12Store _store;
        private readonly string _alias;

        public SSLv3TlsServer(Pkcs12Store store, string alias)
            : base(new BcTlsCryptoWithRC4(new SecureRandom()))
        {
            _store = store;
            _alias = alias;
        }

        protected override ProtocolVersion[] GetSupportedVersions()
            => new[] { ProtocolVersion.SSLv3 };

        // SSLv3 does not support RFC 5746 secure renegotiation. Accept it.
        public override void NotifySecureRenegotiation(bool secureRenegotiation) { }

        public override void ProcessClientExtensions(System.Collections.Generic.IDictionary<int, byte[]> clientExtensions)
        {
            // SSLv3 clients don't send extensions
            if (clientExtensions == null || clientExtensions.Count == 0)
                return;
            base.ProcessClientExtensions(clientExtensions);
        }

        protected override int[] GetSupportedCipherSuites() => new[]
        {
            CipherSuite.TLS_RSA_WITH_RC4_128_SHA,
            CipherSuite.TLS_RSA_WITH_RC4_128_MD5,
            CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
            CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA,
            CipherSuite.TLS_RSA_WITH_3DES_EDE_CBC_SHA,
        };

        public override void NotifyOfferedCipherSuites(int[] offeredCipherSuites)
        {
            if (Logger.LogLevel >= 5)
            {
                var suites = string.Join(" ", Array.ConvertAll(offeredCipherSuites, s => "0x" + s.ToString("X4")));
                Logger.Log("[SSLv3] Client offered cipher suites: " + suites, LogColor.Gray, 5);
            }
            base.NotifyOfferedCipherSuites(offeredCipherSuites);
        }

        public override TlsCredentials GetCredentials()
        {
            var crypto = (BcTlsCrypto)m_context.Crypto;

            var certChain = _store.GetCertificateChain(_alias);
            var tlsCerts = new TlsCertificate[certChain.Length];
            for (int i = 0; i < certChain.Length; i++)
                tlsCerts[i] = new BcTlsCertificate(crypto, certChain[i].Certificate.GetEncoded());

            var privateKey = _store.GetKey(_alias).Key;
            return new BcDefaultTlsCredentialedDecryptor(crypto, new Certificate(tlsCerts), privateKey);
        }
    }

    /// <summary>
    /// Extends BcTlsCrypto to re-add RC4 cipher support removed in BouncyCastle 2.x.
    /// </summary>
    internal class BcTlsCryptoWithRC4 : BcTlsCrypto
    {
        public BcTlsCryptoWithRC4(SecureRandom random) : base(random) { }

        public override bool HasEncryptionAlgorithm(int encryptionAlgorithm)
        {
            if (encryptionAlgorithm == EncryptionAlgorithm.RC4_128)
                return true;
            return base.HasEncryptionAlgorithm(encryptionAlgorithm);
        }

        public override TlsCipher CreateCipher(TlsCryptoParameters cryptoParams, int encryptionAlgorithm, int macAlgorithm)
        {
            if (encryptionAlgorithm == EncryptionAlgorithm.RC4_128)
            {
                var clientCipher = new RC4Engine();
                var serverCipher = new RC4Engine();
                var clientMac = CreateMac(cryptoParams, macAlgorithm);
                var serverMac = CreateMac(cryptoParams, macAlgorithm);
                return new TlsRC4StreamCipher(cryptoParams, clientCipher, serverCipher, clientMac, serverMac, 16);
            }
            return base.CreateCipher(cryptoParams, encryptionAlgorithm, macAlgorithm);
        }
    }

    /// <summary>
    /// RC4 stream cipher for TLS/SSLv3, replacing TlsStreamCipher removed in BouncyCastle 2.x.
    /// </summary>
    internal class TlsRC4StreamCipher : TlsCipher
    {
        private readonly IStreamCipher _encryptCipher;
        private readonly IStreamCipher _decryptCipher;
        private readonly TlsSuiteHmac _writeMac;
        private readonly TlsSuiteHmac _readMac;

        public bool UsesOpaqueRecordType => false;

        public TlsRC4StreamCipher(
            TlsCryptoParameters cryptoParams,
            IStreamCipher clientCipher,
            IStreamCipher serverCipher,
            TlsHmac clientMac,
            TlsHmac serverMac,
            int cipherKeySize)
        {
            int macLength = clientMac.MacLength;

            // Key block: client_MAC_key | server_MAC_key | client_cipher_key | server_cipher_key
            // No IVs for stream ciphers
            int keyBlockSize = (2 * cipherKeySize) + (2 * macLength);
            byte[] keyBlock = TlsImplUtilities.CalculateKeyBlock(cryptoParams, keyBlockSize);

            int offset = 0;

            // Distribute MAC keys
            clientMac.SetKey(keyBlock, offset, macLength);
            offset += macLength;
            serverMac.SetKey(keyBlock, offset, macLength);
            offset += macLength;

            // Extract cipher keys
            byte[] clientKey = new byte[cipherKeySize];
            Array.Copy(keyBlock, offset, clientKey, 0, cipherKeySize);
            offset += cipherKeySize;
            byte[] serverKey = new byte[cipherKeySize];
            Array.Copy(keyBlock, offset, serverKey, 0, cipherKeySize);

            // Initialize based on whether we're the server or client
            if (cryptoParams.IsServer)
            {
                _writeMac = new TlsSuiteHmac(cryptoParams, serverMac);
                _readMac = new TlsSuiteHmac(cryptoParams, clientMac);
                serverCipher.Init(true, new KeyParameter(serverKey));
                clientCipher.Init(false, new KeyParameter(clientKey));
                _encryptCipher = serverCipher;
                _decryptCipher = clientCipher;
            }
            else
            {
                _writeMac = new TlsSuiteHmac(cryptoParams, clientMac);
                _readMac = new TlsSuiteHmac(cryptoParams, serverMac);
                clientCipher.Init(true, new KeyParameter(clientKey));
                serverCipher.Init(false, new KeyParameter(serverKey));
                _encryptCipher = clientCipher;
                _decryptCipher = serverCipher;
            }
        }

        public int GetPlaintextLimit(int ciphertextLimit)
        {
            return ciphertextLimit - _writeMac.Size;
        }

        public int GetCiphertextDecodeLimit(int plaintextLimit)
        {
            return plaintextLimit + _readMac.Size;
        }

        public int GetCiphertextEncodeLimit(int plaintextLength, int plaintextLimit)
        {
            return plaintextLength + _writeMac.Size;
        }

        public TlsEncodeResult EncodePlaintext(long seqNo, short contentType, ProtocolVersion recordVersion,
            int headerAllocation, byte[] plaintext, int offset, int len)
        {
            byte[] mac = _writeMac.CalculateMac(seqNo, contentType, plaintext, offset, len);
            int totalLen = len + mac.Length;
            byte[] output = new byte[headerAllocation + totalLen];

            // Encrypt plaintext
            _encryptCipher.ProcessBytes(plaintext, offset, len, output, headerAllocation);
            // Encrypt MAC
            _encryptCipher.ProcessBytes(mac, 0, mac.Length, output, headerAllocation + len);

            return new TlsEncodeResult(output, 0, output.Length, contentType);
        }

        public TlsEncodeResult EncodePlaintext(long seqNo, short contentType, ProtocolVersion recordVersion,
            int headerAllocation, ReadOnlySpan<byte> plaintext)
        {
            return EncodePlaintext(seqNo, contentType, recordVersion, headerAllocation,
                plaintext.ToArray(), 0, plaintext.Length);
        }

        public TlsDecodeResult DecodeCiphertext(long seqNo, short recordType, ProtocolVersion recordVersion,
            byte[] ciphertext, int offset, int len)
        {
            int macSize = _readMac.Size;
            if (len < macSize)
                throw new TlsFatalAlert(AlertDescription.decode_error);

            int plaintextLen = len - macSize;

            // Decrypt in-place
            byte[] decrypted = new byte[len];
            _decryptCipher.ProcessBytes(ciphertext, offset, len, decrypted, 0);

            // Verify MAC
            byte[] expectedMac = _readMac.CalculateMac(seqNo, recordType, decrypted, 0, plaintextLen);

            // Constant-time comparison
            bool macValid = true;
            for (int i = 0; i < macSize; i++)
            {
                if (decrypted[plaintextLen + i] != expectedMac[i])
                    macValid = false;
            }

            if (!macValid)
                throw new TlsFatalAlert(AlertDescription.bad_record_mac);

            return new TlsDecodeResult(decrypted, 0, plaintextLen, recordType);
        }

        public void RekeyDecoder() { /* Not supported for SSLv3/stream ciphers */ }
        public void RekeyEncoder() { /* Not supported for SSLv3/stream ciphers */ }
    }
}
