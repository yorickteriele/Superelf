import React from 'react';
import { Link } from 'react-router-dom';

/** 404 error page */
const NotFound: React.FC = () => {
  return (
    <div className="container text-center py-5">
      <h1 className="display-1">404</h1>
      <h2 className="mb-4">Page Not Found</h2>
      <p className="lead">The page you are looking for doesn't exist or has been moved.</p>
      <Link to="/" className="btn btn-primary mt-3">
        Return to Home
      </Link>
    </div>
  );
};

export default NotFound;
