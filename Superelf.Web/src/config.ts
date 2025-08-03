interface Config {
  apiUrl: string;
  hubUrl: string;
  isProduction: boolean;
}

// Determine if we're in a production environment
const isProduction = window.location.hostname !== 'localhost' && 
                    !window.location.hostname.includes('127.0.0.1');

// Determine the API base URL based on the environment
let apiBaseUrl = '';

if (isProduction) {
  // For production: use relative path to avoid CORS issues (assuming API is on same domain)
  apiBaseUrl = '/api';
  
  // If we know we're on the actual production domain, we can be explicit about it
  if (window.location.hostname === 'superelf.yorickteriele.nl') {
    apiBaseUrl = '/api'; // Use relative path - avoids CORS completely
  }
} else {
  // For development: use the local dev server via proxy
  apiBaseUrl = '/api'; // This will be proxied via setupProxy.js
}

// Environment and API URL configuration

// Simple configuration with environment-specific paths
const config: Config = {
  apiUrl: apiBaseUrl,
  hubUrl: apiBaseUrl,
  isProduction: isProduction
};

export default config;
