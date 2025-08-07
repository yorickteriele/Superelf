import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { isAdmin } from '../utils/auth';

interface ProtectedAdminRouteProps {
  redirectPath?: string;
}

const ProtectedAdminRoute: React.FC<ProtectedAdminRouteProps> = ({ 
  redirectPath = '/' 
}) => {
  const { user, loading } = useAuth();

  if (loading) {
    return <div className="d-flex justify-content-center p-5">Loading...</div>;
  }

  if (!user || !isAdmin(user)) {
    return <Navigate to={redirectPath} replace />;
  }

  return <Outlet />;
};

export default ProtectedAdminRoute;
