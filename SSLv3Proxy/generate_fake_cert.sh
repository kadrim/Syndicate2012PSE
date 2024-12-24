#!/bin/bash

# Thanks to https://github.com/Aim4kill/Bug_OldProtoSSL

# Certificate Authority name
CA_NAME=OTG3

# Certificate name
C_NAME=gosredirector

# Modified der file name, that will be used later
MOD_NAME=gosredirector_mod

# Create private key for the Certificate Authority
openssl genrsa -aes128 -out $CA_NAME.key.pem -passout pass:123456 1024
openssl rsa -in $CA_NAME.key.pem -out $CA_NAME.key.pem -passin pass:123456

# Create the certificate of the Certificate Authority
openssl req -new -md5 -x509 -days 28124 -key $CA_NAME.key.pem -out $CA_NAME.crt -subj "/OU=Online Technology Group/O=Electronic Arts, Inc./L=Redwood City/ST=California/C=US/CN=OTG3 Certificate Authority"

# ------------Certificate Authority created, now we can create Certificate------------

# Create private key for the Certificate
openssl genrsa -aes128 -out $C_NAME.key.pem -passout pass:123456 1024
openssl rsa -in $C_NAME.key.pem -out $C_NAME.key.pem -passin pass:123456

# Create certificate signing request of the certificate
openssl req -new -key $C_NAME.key.pem -out $C_NAME.csr -subj "/CN=gosredirector.ea.com/OU=Global Online Studio/O=Electronic Arts, Inc./ST=California/C=US"

# Create the certificate
openssl x509 -req -in $C_NAME.csr -CA $CA_NAME.crt -CAkey $CA_NAME.key.pem -CAcreateserial -out $C_NAME.crt -days 10000 -md5

# ------------Certificate created, now export it to .der format so we can modify it------------
openssl x509 -outform der -in $C_NAME.crt -out $C_NAME.der

echo Der file exported, now patch it manually. Filename: $C_NAME.der
echo Please use a Hex-Editor and search for the Hex-Value '2a864886f70d010104'.
echo This Hex-Value should appear two times within in the file.
echo The second occurance must be changed to '2a864886f70d010101'
echo that means, just replace the '04' with '01'.
echo Then save the file as $MOD_NAME.der in this directory.
echo Afterwards please press Enter to continue ...
echo
read

# ------------Certificate modified, now export it to .pfx format so we can use it------------

# Convert .der back to .crt
openssl x509 -inform der -in $MOD_NAME.der -out $MOD_NAME.crt

echo In the next step you will be asked for a password. Fo usage within this project,
echo set an empty password. For other tools you might need a password like '123456'
echo
read

# Export it as .pfx file
openssl pkcs12 -export -out $MOD_NAME.pfx -inkey $C_NAME.key.pem -in $MOD_NAME.crt

echo Done!
