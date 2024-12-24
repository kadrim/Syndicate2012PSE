var tls = require('tls');
var fs = require('fs');
var net = require('net');

console.log('LOCAL_FQDN', process.env.LOCAL_FQDN);
console.log('CLIENT_HOST', process.env.CLIENT_HOST);

const SERVER_PORT = 42127;
const SERVER_HOST = '0.0.0.0'
var serverOptions = {
    pfx: (process.env.LOCAL_FQDN === 'true') ? fs.readFileSync('gosredirector_mod_local.pfx') : fs.readFileSync('gosredirector_mod.pfx'),
    secureProtocol: 'SSLv3_method',
    ciphers: 'AES128-GCM-SHA256:RC4:HIGH:MD5:aNULL:EDH:ecdh-ecdsa-aes128-sha:ecdh-ecdsa-aes256-sha'
};

const CLIENT_PORT = 42130;
const CLIENT_HOST = (process.env.CLIENT_HOST !== undefined) ? process.env.CLIENT_HOST : '172.17.0.1';

var server = tls.createServer(serverOptions, function (socket) {
    socket.on('data', function (data) {
        client.write(data);
        console.log('Server received: %s [it is %d bytes long]',
            data.toString().replace(/(\n)/gm, ""),
            data.length);
    });

    socket.on('end', function () {
        client.end();
        client.destroy();
        console.log('EOT (End Of Transmission)');
    });

    var client = net.connect(CLIENT_PORT, CLIENT_HOST, function () {
        console.log("Client connected!");
    });
    client.on("data", function (data) {
        socket.write(data);
        console.log('Client received: %s [it is %d bytes long]',
            data.toString().replace(/(\n)/gm, ""),
            data.length);
    });
    client.on('close', function () {
        console.log("Client connection closed");
    });

    client.on('error', function (error) {
        console.error(error);
        client.destroy();
    });
});

server.listen(SERVER_PORT, SERVER_HOST, function () {
    console.log("I'm listening at %s, on port %s", SERVER_HOST, SERVER_PORT);
});

server.on('error', function (error) {
    console.error(error);
    server.destroy();
});
