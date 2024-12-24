FROM node:20-alpine

WORKDIR /xenia-web-service

COPY submodules/Xenia-WebServices/package*.json ./

RUN npm install

COPY submodules/Xenia-WebServices/. .

RUN npm run build

ENV API_PORT=36000
ENV MONGO_URI=mongodb://127.0.0.1:27017/
ENV SWAGGER_API=false
ENV SSL=false
ENV NGINX=false
ENV HEROKU_NGINX=false

EXPOSE 36000

CMD [ "npm", "run", "start:prod" ]
