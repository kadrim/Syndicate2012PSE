FROM node:20-alpine

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /xenia-web-service

COPY submodules/Xenia-WebServices/. .

RUN chown -R appuser:appgroup .

USER appuser

RUN npm install

RUN npm run build

ENV API_PORT=36000
ENV MONGO_URI=mongodb://127.0.0.1:27017/
ENV SWAGGER_API=false
ENV SSL=false
ENV NGINX=false
ENV HEROKU_NGINX=false

EXPOSE 36000

CMD [ "npm", "run", "start:prod" ]
