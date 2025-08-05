
# Multi-stage build for React app, but use pre-built build/ if available
FROM nginx:alpine AS production

WORKDIR /app

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy pre-built build/ directory if it exists (from deployment archive)
COPY build /usr/share/nginx/html

# If build/ is not present, fallback to building in Docker (for local/dev)
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ARG REACT_APP_API_URL
ARG REACT_APP_ENVIRONMENT
ENV REACT_APP_API_URL=$REACT_APP_API_URL
ENV REACT_APP_ENVIRONMENT=$REACT_APP_ENVIRONMENT
RUN CI=false npm run build

# Final stage: use pre-built if present, else use built
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Prefer pre-built build/ (from deployment archive), fallback to build stage
COPY --from=build /app/build /usr/share/nginx/html
COPY build /usr/share/nginx/html

# Create a non-root user
RUN addgroup -g 1001 -S superelf && \
    adduser -S superelf -u 1001

# Change ownership of nginx directories
RUN chown -R superelf:superelf /usr/share/nginx/html && \
    chown -R superelf:superelf /var/cache/nginx && \
    chown -R superelf:superelf /var/log/nginx && \
    chown -R superelf:superelf /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R superelf:superelf /var/run/nginx.pid

USER superelf

# Expose port
EXPOSE 3000

CMD ["nginx", "-g", "daemon off;"]
