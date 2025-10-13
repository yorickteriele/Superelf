# Production stage with nginx - use pre-built files
FROM nginx:alpine

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy pre-built app (built by GitHub Actions)
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