# Frontend Dockerfile — build with Node, serve with nginx
FROM node:18 AS build
WORKDIR /app

COPY web/ballastlane-web/package*.json ./
RUN npm ci --silent

COPY web/ballastlane-web/ .
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
