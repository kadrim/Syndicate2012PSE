FROM ubuntu:trusty

WORKDIR /sslv3-proxy

RUN apt-get update && apt-get install -y nodejs && apt-get clean

COPY SSLv3Proxy/. .

ENV LOCAL_FQDN=false
ENV CLIENT_HOST=172.17.0.1

EXPOSE 42127

CMD [ "nodejs", "proxy.js" ]
