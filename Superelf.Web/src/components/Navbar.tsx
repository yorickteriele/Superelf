import React, { useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  // Initialize Bootstrap JavaScript for dropdowns - simplified approach
  useEffect(() => {
    // Skip Bootstrap JS import for now to avoid TypeScript issues
    // Bootstrap CSS classes will still work for styling
    console.log('Navbar initialized - using Bootstrap CSS only');
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
