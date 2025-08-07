export interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  role?: string | string[];
  exp: number;
  iat: number;
  [key: string]: any; // Allow indexing with any string key
}

export const decodeJwt = (token: string): JwtPayload | null => {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error('Error decoding JWT:', error);
    return null;
  }
};

export const isAdmin = (user: { roles?: string[] } | null): boolean => {
  return user?.roles?.includes('Admin') || false;
};

export const getUserRolesFromToken = (token: string): string[] => {
  const payload = decodeJwt(token);
  if (!payload) return [];
  
  // JWT roles can be under different claim names
  const roleClaimName = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
  const roles = payload.role || payload[roleClaimName];
  
  if (!roles) return [];
  
  // Handle both string and array roles
  if (typeof roles === 'string') {
    return [roles];
  }
  
  return Array.isArray(roles) ? roles : [];
};
