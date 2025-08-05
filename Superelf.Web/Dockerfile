# Multi-stage build for React app
FROM node:22-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --only=production

# Copy source code
COPY . .

# Build the application
ARG REACT_APP_API_URL
ARG REACT_APP_ENVIRONMENT
ENV REACT_APP_API_URL=$REACT_APP_API_URL
ENV REACT_APP_ENVIRONMENT=$REACT_APP_ENVIRONMENT

RUN npm run build

# Production stage with nginx
FROM nginx:alpine

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built app from build stage
COPY --from=build /app/build /usr/share/nginx/html

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
