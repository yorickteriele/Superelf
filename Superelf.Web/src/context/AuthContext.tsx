import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, authService } from '../services/api';
import { getUserRolesFromToken } from '../utils/auth';

/**
 * Authentication context interface providing user state and auth operations.
 */
interface AuthContextType {
  /** Currently logged in user or null if not authenticated */
  user: User | null;
  /** Loading state for async operations */
  loading: boolean;
  /** Login function taking credentials and remember-me flag */
  login: (emailOrUsername: string, password: string, rememberMe: boolean) => Promise<boolean>;
  /** Registration function taking user details */
  register: (username: string, email: string, password: string, phoneNumber?: string) => Promise<boolean>;
  /** Logout function clearing user session */
  logout: () => Promise<void>;
  /** Any error message from auth operations */
  error: string | null;
}

/** React context for authentication state and operations */
const AuthContext = createContext<AuthContextType | undefined>(undefined);

/** Props for AuthProvider component */
interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Authentication provider component that manages user authentication state
 * and provides auth operations through context.
 * 
 * Features:
 * - Persistent auth state using localStorage
 * - JWT token management
 * - Role-based authorization
 * - Error handling for auth operations
 */
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  /** 
   * Initialize auth state from localStorage on mount.
   * Validates stored credentials and extracts roles from JWT.
   */
  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('authToken');
    
    if (storedUser && storedToken) {
      try {
        const user = JSON.parse(storedUser);
        const roles = getUserRolesFromToken(storedToken);
        user.roles = roles;
        setUser(user);
      } catch (e) {
        // Clear invalid auth data
        localStorage.removeItem('user');
        localStorage.removeItem('authToken');
      }
    } else {
      // Clear partial auth data
      localStorage.removeItem('user');
      localStorage.removeItem('authToken');
    }
    setLoading(false);
  }, []);

  /**
   * Authenticates a user with email/username and password.
   * Stores auth data in localStorage if successful.
   * 
   * @param emailOrUsername - Email or username to log in with
   * @param password - User's password
   * @param rememberMe - Whether to persist auth state
   * @returns Promise resolving to success state
   */
  const login = async (emailOrUsername: string, password: string, rememberMe: boolean): Promise<boolean> => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await authService.login({
        emailOrUsername,
        password,
        rememberMe
      });
      
      if (response.success && response.user && response.token) {
        // Get roles from JWT token
        const roles = getUserRolesFromToken(response.token);
        const userWithRoles = { ...response.user, roles };
        
        setUser(userWithRoles);
        localStorage.setItem('user', JSON.stringify(userWithRoles));
        localStorage.setItem('authToken', response.token);
        setLoading(false);
        return true;
      } else {
        setError(response.message || 'Login failed');
        setLoading(false);
        return false;
      }
    } catch (error) {
      setError('An unexpected error occurred');
      setLoading(false);
      return false;
    }
  };

  /**
   * Registers a new user with the provided details.
   * On success, automatically logs in the user.
   * 
   * @param username - Desired username
   * @param email - User's email address
   * @param password - User's password
   * @param phoneNumber - Optional phone number
   * @returns Promise resolving to success state
   */
  const register = async (
    username: string, 
    email: string, 
    password: string, 
    phoneNumber?: string
  ): Promise<boolean> => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await authService.register({
        username,
        email,
        password,
        phoneNumber
      });
      
      if (response.success && response.user && response.token) {
        const roles = getUserRolesFromToken(response.token);
        const userWithRoles = { ...response.user, roles };
        
        setUser(userWithRoles);
        localStorage.setItem('user', JSON.stringify(userWithRoles));
        localStorage.setItem('authToken', response.token);
        setLoading(false);
        return true;
      } else {
        setError(response.message || 'Registration failed');
        setLoading(false);
        return false;
      }
    } catch (error) {
      setError('An unexpected error occurred');
      setLoading(false);
      return false;
    }
  };

  /**
   * Logs out the current user and clears auth state.
   * Handles errors silently to ensure clean logout.
   */
  const logout = async (): Promise<void> => {
    setLoading(true);
    
    try {
      await authService.logout();
      setUser(null);
      localStorage.removeItem('user');
      localStorage.removeItem('authToken');
    } catch (error) {
      // Silent error handling ensures logout completes
      // even if server request fails
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout, error }}>
      {children}
    </AuthContext.Provider>
  );
};

/**
 * Hook to access authentication context.
 * Must be used within an AuthProvider component.
 * 
 * @returns Authentication context containing user state and auth operations
 * @throws Error if used outside AuthProvider
 */
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
