FROM node:22-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG EXPO_PUBLIC_APP_ENV=production
ARG EXPO_PUBLIC_API_URL=/api
ARG EXPO_PUBLIC_TRACKING_WS_URL=/logistic-service
ARG EXPO_PUBLIC_APP_URL=

ENV EXPO_PUBLIC_APP_ENV=$EXPO_PUBLIC_APP_ENV \
    EXPO_PUBLIC_API_URL=$EXPO_PUBLIC_API_URL \
    EXPO_PUBLIC_TRACKING_WS_URL=$EXPO_PUBLIC_TRACKING_WS_URL \
    EXPO_PUBLIC_APP_URL=$EXPO_PUBLIC_APP_URL

RUN npx expo export -p web

FROM nginx:alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
