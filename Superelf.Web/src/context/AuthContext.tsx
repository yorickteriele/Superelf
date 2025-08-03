import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, authService } from '../services/api';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (emailOrUsername: string, password: string, rememberMe: boolean) => Promise<boolean>;
  register: (username: string, email: string, password: string, phoneNumber?: string) => Promise<boolean>;
  logout: () => Promise<void>;
  error: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Check if the user is already logged in
  useEffect(() => {
    // Try to get user and token from local storage
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('authToken');
    
    if (storedUser && storedToken) {
      try {
        setUser(JSON.parse(storedUser));
      } catch (e) {
        // If user data is corrupted, clear both user and token
        localStorage.removeItem('user');
        localStorage.removeItem('authToken');
      }
    } else {
      // If either user or token is missing, clear both
      localStorage.removeItem('user');
      localStorage.removeItem('authToken');
    }
    setLoading(false);
  }, []);

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
        setUser(response.user);
        localStorage.setItem('user', JSON.stringify(response.user));
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
        setUser(response.user);
        localStorage.setItem('user', JSON.stringify(response.user));
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

  const logout = async (): Promise<void> => {
    setLoading(true);
    
    try {
      await authService.logout();
      setUser(null);
      localStorage.removeItem('user');
      localStorage.removeItem('authToken');
    } catch (error) {
      // Removed console.error - silent error handling
      // We don't want to show errors during logout
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

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
