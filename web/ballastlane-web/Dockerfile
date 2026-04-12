# Frontend Dockerfile — build with Node, serve with nginx
FROM node:22-alpine AS build
WORKDIR /app

COPY web/ballastlane-web/package*.json ./
# Use `npm install` in the build image to avoid failing when no package-lock.json exists
RUN npm install --no-audit --progress=false

COPY web/ballastlane-web/ .
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
# Custom config: SPA fallback (try_files) + /api proxy to backend container
COPY web/ballastlane-web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
