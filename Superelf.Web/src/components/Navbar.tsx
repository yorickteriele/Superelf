import React, { useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/**
 * Navigation bar component providing site-wide navigation and user authentication status.
 * Features include:
 * - Responsive navigation menu
 * - Dynamic links based on authentication state
 * - User dropdown menu with profile and logout options
 * - Admin panel access for users with admin role
 * 
 * @returns Navigation bar with dynamic content based on user authentication
 */
const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  /**
   * Handles user logout action by clearing authentication
   * and redirecting to home page.
   */
  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  useEffect(() => {
    // Component relies on Bootstrap CSS classes for styling
    // Bootstrap JS functionality is handled through data attributes
  }, []);

  return (
    <nav className="navbar navbar-expand-lg navbar-dark shadow sticky-top" style={{ background: 'linear-gradient(135deg, #1a1a1a 0%, #282828 100%)' }}>
      <div className="container">
        <Link className="navbar-brand fw-bold" to="/">
          <i className="bi bi-trophy-fill me-2"></i>
          Superelf
        </Link>
        <button 
          className="navbar-toggler" 
          type="button" 
          data-bs-toggle="collapse" 
          data-bs-target="#navbarNav" 
          aria-controls="navbarNav" 
          aria-expanded="false" 
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>
        <div className="collapse navbar-collapse" id="navbarNav">
          <ul className="navbar-nav me-auto">
            <li className="nav-item">
              <Link 
                className={`nav-link ${location.pathname === '/' ? 'active' : ''}`} 
                to="/"
              >
                Home
              </Link>
            </li>
            {user && (
              <>
                <li className="nav-item">
                  <Link 
                    className={`nav-link ${location.pathname.startsWith('/pools') ? 'active' : ''}`} 
                    to="/pools"
                  >
                    <i className="bi bi-people-fill me-1"></i>
                    Pools
                  </Link>
                </li>
                {user.roles?.includes('Admin') && (
                  <li className="nav-item">
                    <Link 
                      className={`nav-link ${location.pathname.startsWith('/admin') ? 'active' : ''}`} 
                      to="/admin"
                    >
                      <i className="bi bi-gear-fill me-1"></i>
                      Admin
                    </Link>
                  </li>
                )}
              </>
            )}
          </ul>
          <ul className="navbar-nav">
            {!user ? (
              <>
                <li className="nav-item">
                  <Link 
                    className="nav-link btn btn-outline-light me-2 px-3" 
                    to="/login"
                  >
                    <i className="bi bi-box-arrow-in-right me-1"></i>
                    Login
                  </Link>
                </li>
                <li className="nav-item">
                  <Link 
                    className="nav-link btn btn-light text-primary px-3" 
                    to="/register"
                  >
                    <i className="bi bi-person-plus me-1"></i>
                    Register
                  </Link>
                </li>
              </>
            ) : (
              <li className="nav-item dropdown">
                <button 
                  className="nav-link dropdown-toggle d-flex align-items-center border-0 bg-transparent" 
                  id="navbarDropdown" 
                  data-bs-toggle="dropdown" 
                  aria-expanded="false"
                >
                  <i className="bi bi-person-circle me-2"></i>
                  {user.username}
                </button>
                <ul className="dropdown-menu dropdown-menu-end shadow" aria-labelledby="navbarDropdown">
                  <li>
                    <Link className="dropdown-item" to="/profile">
                      <i className="bi bi-person me-2"></i>
                      Profile
                    </Link>
                  </li>
                  <li><hr className="dropdown-divider" /></li>
                  <li>
                    <button className="dropdown-item text-danger" onClick={handleLogout}>
                      <i className="bi bi-box-arrow-right me-2"></i>
                      Logout
                    </button>
                  </li>
                </ul>
              </li>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
