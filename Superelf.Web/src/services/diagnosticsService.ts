import axios from 'axios';
import config from '../config';

const API_URL = config.apiUrl;

// Create a separate axios instance that doesn't use credentials
// This is to test basic connectivity without CORS issues
const diagnosticsClient = axios.create({
  baseURL: API_URL,
  timeout: 10000, // 10 second timeout
  headers: {
    'Content-Type': 'application/json'
  }
});

// No monitoring for request/response
diagnosticsClient.interceptors.request.use(config => {
  return config;
});

diagnosticsClient.interceptors.response.use(
  response => {
    return response;
  },
  error => {
    return Promise.reject(error);
  }
);

export const diagnosticsService = {
  // Test API connectivity with different methods
  pingApi: async (): Promise<boolean> => {
    try {
      // First try with fetch API
      try {
        const fetchResponse = await fetch(`${API_URL}/diagnostics/ping`, {
          method: 'GET',
          mode: 'cors',
          headers: {
            'Content-Type': 'application/json'
          }
        });
        // Removed console.log
        if (fetchResponse.ok) {
          await fetchResponse.json();
          // Removed console.log
          return true;
        }
      } catch (fetchError) {
        // Silent error handling
      }

      // Then try with axios
      await diagnosticsClient.get('/diagnostics/ping');
      return true;
    } catch (error) {
      return false;
    }
  },

  // Check CORS configuration
  checkCors: async (): Promise<any> => {
    try {
      const response = await diagnosticsClient.get('/diagnostics/cors');
      return response.data;
    } catch (error) {
      return { error: 'CORS check failed' };
    }
  },

  // Check if we can make a simple OPTIONS request
  testPreflightRequest: async (): Promise<boolean> => {
    try {
      // Make a manual OPTIONS request
      const response = await fetch(`${API_URL}/diagnostics/ping`, {
        method: 'OPTIONS',
        headers: {
          'Access-Control-Request-Method': 'GET',
          'Access-Control-Request-Headers': 'content-type',
          'Origin': window.location.origin
        }
      });
      
      return response.ok;
    } catch (error) {
      // Silent error handling
      return false;
    }
  }
};

// Removed global export to avoid polluting the global namespace
