import React from 'react';
import { Link } from 'react-router-dom';

const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();
  
  return (
    <footer className="py-4 mt-5" style={{ backgroundColor: '#1a1a1a', borderTop: '1px solid #343a40' }}>
      <div className="container">
        <div className="row gy-4">
          <div className="col-lg-4">
            <h5 className="fw-bold mb-3 text-white">SuperElf</h5>
            <p className="text-white-50">
              The ultimate fantasy football experience where you can build your dream team and compete with friends.
            </p>
          </div>
          <div className="col-lg-2">
            <h6 className="text-primary text-uppercase fw-bold mb-3">Navigation</h6>
            <ul className="list-unstyled">
              <li className="mb-2"><Link to="/" className="text-decoration-none text-white-50">Home</Link></li>
              <li className="mb-2"><Link to="/pools" className="text-decoration-none text-white-50">Pools</Link></li>
            </ul>
          </div>
          <div className="col-lg-2">
            <h6 className="text-primary text-uppercase fw-bold mb-3">Account</h6>
            <ul className="list-unstyled">
              <li className="mb-2"><Link to="/login" className="text-decoration-none text-white-50">Login</Link></li>
              <li className="mb-2"><Link to="/register" className="text-decoration-none text-white-50">Register</Link></li>
            </ul>
          </div>
          <div className="col-lg-4">
            <h6 className="text-primary text-uppercase fw-bold mb-3">Stay Connected</h6>
            <div className="d-flex gap-3 mb-3">
              <a href="https://facebook.com" target="_blank" rel="noopener noreferrer" className="text-decoration-none text-primary fs-5"><i className="bi bi-facebook"></i></a>
              <a href="https://twitter.com" target="_blank" rel="noopener noreferrer" className="text-decoration-none text-primary fs-5"><i className="bi bi-twitter-x"></i></a>
              <a href="https://instagram.com" target="_blank" rel="noopener noreferrer" className="text-decoration-none text-primary fs-5"><i className="bi bi-instagram"></i></a>
              <a href="https://youtube.com" target="_blank" rel="noopener noreferrer" className="text-decoration-none text-primary fs-5"><i className="bi bi-youtube"></i></a>
            </div>
          </div>
        </div>
        <hr className="mt-4 mb-3" style={{ borderColor: '#343a40' }} />
        <div className="row">
          <div className="col-md-6">
            <p className="mb-0 text-white-50">&copy; {currentYear} SuperElf. All rights reserved.</p>
          </div>
          <div className="col-md-6 text-md-end">
            <ul className="list-inline mb-0">
              <li className="list-inline-item">
                <Link to="/privacy-policy" className="text-decoration-none text-white-50 small">Privacy Policy</Link>
              </li>
              <li className="list-inline-item">
                <span className="text-white-50 mx-2">|</span>
              </li>
              <li className="list-inline-item">
                <Link to="/terms" className="text-decoration-none text-white-50 small">Terms of Service</Link>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
