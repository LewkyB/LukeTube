FROM node:latest as build

ARG RESOURCE_REAPER_SESSION_ID="00000000-0000-0000-0000-000000000001"
WORKDIR /usr/src/app
COPY package.json ./
RUN npm install -g @angular/cli
RUN npm install
COPY . ./
RUN ng build --configuration production --output-path=dist

FROM nginx:1.17.1-alpine
#COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=build /usr/src/app/dist/* /usr/share/nginx/html/