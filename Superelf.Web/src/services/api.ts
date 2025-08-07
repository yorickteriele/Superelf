import axios from 'axios';
import config from '../config';

const API_URL = config.apiUrl;

export interface LoginRequest {
  emailOrUsername: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterRequest {
  username: string;
  email: string;
  phoneNumber?: string;
  password: string;
}

export interface User {
  id: string;
  username: string;
  email: string;
  roles?: string[];
}

export interface AuthResponse {
  success: boolean;
  message?: string;
  token?: string;
  user?: User;
}

// Create axios instance with credentials
const apiClient = axios.create({
  baseURL: API_URL,
  withCredentials: true, // Important for cookies
  headers: {
    'Content-Type': 'application/json'
  }
});

// Request interceptor to add JWT token to all requests
apiClient.interceptors.request.use(
  (config) => {
    // Get the JWT token from localStorage
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Authentication services
export const authService = {
  register: async (registerData: RegisterRequest): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post('/auth/register', registerData);
      return response.data;
    } catch (error: any) {
      if (error.response) {
        return error.response.data as AuthResponse;
      }
      return {
        success: false,
        message: 'Network error occurred. Please try again later.'
      };
    }
  },
  
  login: async (loginData: LoginRequest): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post('/auth/login', loginData);
      return response.data;
    } catch (error: any) {
      if (error.response) {
        return error.response.data as AuthResponse;
      }
      
      return {
        success: false,
        message: 'Network error occurred. Please try again later.'
      };
    }
  },
  
  logout: async (): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post('/auth/logout');
      return response.data;
    } catch (error: any) {
      if (error.response) {
        return error.response.data as AuthResponse;
      }
      return {
        success: false,
        message: 'Network error occurred. Please try again later.'
      };
    }
  }
};

export default apiClient;
