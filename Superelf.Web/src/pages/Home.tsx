import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/** Landing page with feature highlights and auth-based content */
const Home: React.FC = () => {
  const { user } = useAuth();

  return (
    <div className="container py-5 fade-in">
      {/* Hero Section */}
      <div className="row align-items-center py-5">
        <div className="col-lg-6 mb-5 mb-lg-0">
          <h1 className="display-4 fw-bold mb-4">
            Dominate Fantasy Football with <span className="text-primary">SuperElf</span>
          </h1>
          <p className="lead mb-4">
            Create your ultimate fantasy team, compete with friends, and rise to the top of the leaderboards.
          </p>
          
          {!user ? (
            <div className="d-flex gap-3 mt-4">
              <Link to="/register" className="btn btn-primary btn-lg px-4">
                Get Started
                <i className="bi bi-arrow-right ms-2"></i>
              </Link>
              <Link to="/login" className="btn btn-outline-primary btn-lg px-4">
                Sign In
              </Link>
            </div>
          ) : (
            <div className="d-flex flex-column align-items-start">
              <p className="mb-3">
                Welcome back, <strong>{user.username}</strong>!
              </p>
              <Link to="/pools" className="btn btn-primary btn-lg px-4">
                View Your Pools
                <i className="bi bi-arrow-right ms-2"></i>
              </Link>
            </div>
          )}
        </div>
        <div className="col-lg-6">
          <div className="bg-gradient-primary p-5 text-center text-white rounded-3 shadow">
            <i className="bi bi-trophy-fill display-1 mb-3"></i>
            <h2 className="h1 mb-3">2025 Season</h2>
            <p className="lead">The competition is heating up! Join now to compete with friends.</p>
          </div>
        </div>
      </div>
      
      {/* Features Section */}
      <div className="row mt-5 pt-5 g-4">
        <div className="col-12 text-center mb-4">
          <h2 className="fw-bold">How SuperElf Works</h2>
          <p className="text-muted">Simple steps to fantasy football glory</p>
        </div>
        
        <div className="col-md-4">
          <div className="card h-100 shadow-sm">
            <div className="card-body text-center p-4">
              <div className="rounded-circle bg-primary bg-opacity-10 p-3 d-inline-flex mb-3">
                <i className="bi bi-people-fill text-primary fs-1"></i>
              </div>
              <h3 className="card-title h4">Create or Join Pools</h3>
              <p className="card-text text-muted">
                Create your own pool to compete with friends or join existing ones with a simple invitation code.
              </p>
            </div>
          </div>
        </div>
        
        <div className="col-md-4">
          <div className="card h-100 shadow-sm">
            <div className="card-body text-center p-4">
              <div className="rounded-circle bg-primary bg-opacity-10 p-3 d-inline-flex mb-3">
                <i className="bi bi-person-plus-fill text-primary fs-1"></i>
              </div>
              <h3 className="card-title h4">Select Your Players</h3>
              <p className="card-text text-muted">
                Build your dream team by selecting the best players for each position from different football clubs.
              </p>
            </div>
          </div>
        </div>
        
        <div className="col-md-4">
          <div className="card h-100 shadow-sm">
            <div className="card-body text-center p-4">
              <div className="rounded-circle bg-primary bg-opacity-10 p-3 d-inline-flex mb-3">
                <i className="bi bi-graph-up-arrow text-primary fs-1"></i>
              </div>
              <h3 className="card-title h4">Compete & Win</h3>
              <p className="card-text text-muted">
                Earn points based on your players' real-world performance and compete for the top spot on the leaderboard.
              </p>
            </div>
          </div>
        </div>
      </div>
      
      {/* CTA Section */}
      <div className="row mt-5 pt-5">
        <div className="col-12">
          <div className="bg-light p-5 rounded-3 text-center shadow-sm">
            <h2 className="fw-bold mb-3">Ready to Become a Champion?</h2>
            <p className="mb-4">Join thousands of players competing for glory in SuperElf fantasy football.</p>
            {!user ? (
              <div className="d-flex justify-content-center gap-3">
                <Link to="/register" className="btn btn-primary btn-lg px-4">
                  Sign Up Now
                </Link>
                <Link to="/login" className="btn btn-outline-secondary btn-lg px-4">
                  I Already Have an Account
                </Link>
              </div>
            ) : (
              <Link to="/pools" className="btn btn-primary btn-lg px-4">
                Get Started
                <i className="bi bi-arrow-right ms-2"></i>
              </Link>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Home;
