import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Pool, poolService } from '../services/poolService';
import { useAuth } from '../context/AuthContext';

/** Pool management: view, create, join, and navigate pools */
const Pools: React.FC = () => {
  const [pools, setPools] = useState<Pool[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [newPoolName, setNewPoolName] = useState<string>('');
  const [joinPoolCode, setJoinPoolCode] = useState<string>('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const { /* user: currentUser */ } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    fetchPools();
  }, []);

  /**
   * Fetches all pools that the current user is a member of.
   * Updates state with the fetched pools or empty array on error.
   */
  const fetchPools = async () => {
    setLoading(true);
    try {
      const userPools = await poolService.getUserPools();
      setPools(Array.isArray(userPools) ? userPools : []);
    } catch (error) {
      setError('Failed to fetch pools');
      setPools([]);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handles the creation of a new pool.
   * Validates the pool name and creates a new pool using the pool service.
   * 
   * @param e - Form submission event
   */
  const handleCreatePool = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    
    if (!newPoolName.trim()) {
      setError('Pool name cannot be empty');
      return;
    }
    
    try {
      const createdPool = await poolService.createPool({ name: newPoolName.trim() });
      if (createdPool) {
        setPools([...(Array.isArray(pools) ? pools : []), createdPool]);
        setNewPoolName('');
        setSuccess('Pool created successfully!');
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError('Failed to create pool');
      }
    } catch (error) {
      setError('An error occurred while creating the pool');
    }
  };

  /**
   * Handles joining an existing pool using a pool code.
   * Validates the pool code and attempts to join using the pool service.
   * 
   * @param e - Form submission event
   */
  const handleJoinPool = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    
    if (!joinPoolCode.trim()) {
      setError('Pool code cannot be empty');
      return;
    }
    
    try {
      const result = await poolService.joinPool({ poolCode: joinPoolCode.trim() });
      if (result.success) {
        setJoinPoolCode('');
        setSuccess('Successfully joined pool!');
        fetchPools(); // Refresh pools
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.message || 'Failed to join pool');
      }
    } catch (error) {
      setError('An error occurred while joining the pool');
    }
  };

  /**
   * Navigates to the detailed view of a specific pool.
   * 
   * @param poolId - The ID of the pool to view
   */
  const handleViewPool = (poolId: string) => {
    navigate(`/pools/${poolId}`);
  };

  return (
    <div className="container py-5">
      <h1 className="mb-4">My Pools</h1>
      
      {/* Alert Messages */}
      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {error}
          <button 
            type="button" 
            className="btn-close" 
            onClick={() => setError(null)}
            aria-label="Close"
          ></button>
        </div>
      )}
      
      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          {success}
          <button 
            type="button" 
            className="btn-close" 
            onClick={() => setSuccess(null)}
            aria-label="Close"
          ></button>
        </div>
      )}
      
      <div className="row g-4">
        <div className="col-md-6">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <h5 className="card-title">Create New Pool</h5>
              <form onSubmit={handleCreatePool}>
                <div className="mb-3">
                  <label htmlFor="poolName" className="form-label">Pool Name</label>
                  <input
                    type="text"
                    className="form-control"
                    id="poolName"
                    value={newPoolName}
                    onChange={(e) => setNewPoolName(e.target.value)}
                    placeholder="Enter pool name"
                  />
                </div>
                <button 
                  type="submit" 
                  className="btn btn-primary"
                  disabled={!newPoolName.trim()}
                >
                  Create Pool
                </button>
              </form>
            </div>
          </div>
        </div>
        
        <div className="col-md-6">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <h5 className="card-title">Join Existing Pool</h5>
              <form onSubmit={handleJoinPool}>
                <div className="mb-3">
                  <label htmlFor="poolCode" className="form-label">Pool Code</label>
                  <input
                    type="text"
                    className="form-control"
                    id="poolCode"
                    value={joinPoolCode}
                    onChange={(e) => setJoinPoolCode(e.target.value)}
                    placeholder="Enter pool code"
                  />
                </div>
                <button 
                  type="submit" 
                  className="btn btn-success"
                  disabled={!joinPoolCode.trim()}
                >
                  Join Pool
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
      
      <div className="mt-5">
        <h2>My Pools</h2>
        {loading ? (
          <div className="d-flex justify-content-center mt-4">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
          </div>
        ) : !Array.isArray(pools) || pools.length === 0 ? (
          <div className="alert alert-info mt-3">
            You haven't joined any pools yet. Create a new one or join using a code!
          </div>
        ) : (
          <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4 mt-3">
            {Array.isArray(pools) && pools.map(pool => (
              <div className="col" key={pool.id}>
                <div className="card h-100 shadow-sm">
                  <div className="card-body">
                    <h5 className="card-title">{pool.name}</h5>
                    <h6 className="card-subtitle mb-2 text-muted">Code: {pool.code}</h6>
                    <p className="card-text">
                      <small className="text-muted">
                        Created by: {pool.ownerName}<br />
                        {new Date(pool.createTime).toLocaleDateString()}
                      </small>
                    </p>
                    <p className="card-text">
                      {pool.participants.length} participant{pool.participants.length !== 1 ? 's' : ''}
                    </p>
                  </div>
                  <div className="card-footer bg-transparent border-0">
                    <button
                      className="btn btn-primary"
                      onClick={() => handleViewPool(pool.id)}
                    >
                      View Pool
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default Pools;
