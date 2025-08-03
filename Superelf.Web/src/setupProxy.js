const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function(app) {
  // Add proxy for API requests
  app.use(
    '/api',
    createProxyMiddleware({
      target: 'http://localhost:5000', // Target API server (default ASP.NET Core Kestrel port)
      changeOrigin: true,
      secure: false,
      logLevel: 'silent', // Changed from 'debug' to 'silent' to hide console logs
      onProxyReq: (proxyReq, req, res) => {
        // Logging removed
      },
      onProxyRes: (proxyRes, req, res) => {
        // Logging removed
        
        // CORS headers check (logging removed)
      },
      onError: (err, req, res) => {
        // Only log critical errors without details
        console.error('Proxy Error: Could not connect to backend API');
        res.writeHead(500, {
          'Content-Type': 'text/plain',
        });
        res.end('Proxy error: Could not connect to backend API');
      }
    })
  );
};
