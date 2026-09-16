# Build context is the repo root (see deploy/compose.yaml).
FROM node:24-alpine AS build
WORKDIR /src

COPY src/frontend/package.json src/frontend/package-lock.json ./
RUN npm ci

COPY src/frontend/ .
RUN npm run build

FROM nginx:alpine AS runtime
COPY deploy/docker/frontend.nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist /usr/share/nginx/html

EXPOSE 80
