FROM ubuntu:14.04

RUN apt-get update && apt-get install -y nodejs && apt-get clean

RUN useradd -ms /bin/bash appuser

USER appuser

WORKDIR /sslv3-proxy

COPY SSLv3Proxy/. .

ENV LOCAL_FQDN=false
ENV CLIENT_HOST=172.17.0.1

EXPOSE 42127

CMD [ "nodejs", "proxy.js" ]
