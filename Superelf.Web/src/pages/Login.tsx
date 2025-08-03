import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { diagnosticsService } from '../services/diagnosticsService';

const Login: React.FC = () => {
  const [formData, setFormData] = useState({
    emailOrUsername: '',
    password: '',
    rememberMe: false
  });
  
  const { login, error, loading } = useAuth();
  const navigate = useNavigate();
  const [formError, setFormError] = useState('');
  const [apiStatus, setApiStatus] = useState<{available: boolean, message: string}>({
    available: false,
    message: 'Checking API connectivity...'
  });

  // Run API connectivity test on component mount
  useEffect(() => {
    const checkApiConnection = async () => {
      try {
        // Test basic API connectivity
        const isAvailable = await diagnosticsService.pingApi();
        setApiStatus({
          available: isAvailable,
          message: isAvailable ? 'API is available' : 'API appears to be offline'
        });
        
        // If API is available, test CORS
        if (isAvailable) {
          await diagnosticsService.checkCors();
          await diagnosticsService.testPreflightRequest();
        }
      } catch (err) {
        // Removed console.error
        setApiStatus({
          available: false,
          message: 'Failed to connect to the API'
        });
      }
    };
    
    checkApiConnection();
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData(prevData => ({
      ...prevData,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setFormError('');
    
    // Simple validation
    if (!formData.emailOrUsername || !formData.password) {
      setFormError('Please fill in all fields');
      return;
    }

    const success = await login(
      formData.emailOrUsername,
      formData.password,
      formData.rememberMe
    );
    
    if (success) {
      navigate('/');
    }
  };

  return (
    <div className="container py-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-header bg-primary text-white text-center">
              <h4>Login</h4>
            </div>
            <div className="card-body">
              {/* API Status Alert */}
              {!apiStatus.available && (
                <div className="alert alert-warning mb-3" role="alert">
                  <strong>API Connection Status:</strong> {apiStatus.message}
                  <hr />
                  <small className="text-muted">
                    There might be connection issues with the API server. 
                    Please try again later or contact support if the problem persists.
                  </small>
                </div>
              )}

              {/* Error Messages */}
              {(error || formError) && (
                <div className="alert alert-danger" role="alert">
                  {formError || error}
                </div>
              )}
              
              <form onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label htmlFor="emailOrUsername" className="form-label">Email / Username</label>
                  <input
                    type="text"
                    className="form-control"
                    id="emailOrUsername"
                    name="emailOrUsername"
                    value={formData.emailOrUsername}
                    onChange={handleChange}
                    required
                  />
                </div>
                
                <div className="mb-3">
                  <label htmlFor="password" className="form-label">Password</label>
                  <input
                    type="password"
                    className="form-control"
                    id="password"
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    required
                  />
                </div>
                
                <div className="mb-3 form-check">
                  <input
                    type="checkbox"
                    className="form-check-input"
                    id="rememberMe"
                    name="rememberMe"
                    checked={formData.rememberMe}
                    onChange={handleChange}
                  />
                  <label className="form-check-label" htmlFor="rememberMe">Remember me</label>
                </div>
                
                <button
                  type="submit"
                  className="btn btn-primary w-100"
                  disabled={loading}
                >
                  {loading ? 'Logging in...' : 'Login'}
                </button>
              </form>
              
              <div className="mt-3 text-center">
                <p>
                  Don't have an account? <Link to="/register">Register here</Link>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
