# Build stage
FROM node:20-alpine AS builder

WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --silent
COPY . .

ARG REACT_APP_API_URL=/api
ARG REACT_APP_ENVIRONMENT=production
ENV REACT_APP_API_URL=$REACT_APP_API_URL
ENV REACT_APP_ENVIRONMENT=$REACT_APP_ENVIRONMENT

RUN npm run build

# Production stage
FROM nginx:alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/build /usr/share/nginx/html

RUN addgroup -g 1001 -S superelf && \
    adduser -S superelf -u 1001 && \
    chown -R superelf:superelf /usr/share/nginx/html && \
    chown -R superelf:superelf /var/cache/nginx && \
    chown -R superelf:superelf /var/log/nginx && \
    chown -R superelf:superelf /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R superelf:superelf /var/run/nginx.pid

USER superelf
EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
