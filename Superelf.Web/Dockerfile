
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
ARG CACHEBUST=1
RUN npm ci
COPY . .
ARG REACT_APP_API_URL
ARG REACT_APP_ENVIRONMENT
ENV REACT_APP_API_URL=$REACT_APP_API_URL
ENV REACT_APP_ENVIRONMENT=$REACT_APP_ENVIRONMENT
RUN CI=false npm run build

FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/build /usr/share/nginx/html

RUN addgroup -g 1001 -S superelf && \
    adduser -S superelf -u 1001

RUN chown -R superelf:superelf /usr/share/nginx/html && \
    chown -R superelf:superelf /var/cache/nginx && \
    chown -R superelf:superelf /var/log/nginx && \
    chown -R superelf:superelf /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R superelf:superelf /var/run/nginx.pid

USER superelf

EXPOSE 3000

CMD ["nginx", "-g", "daemon off;"]
