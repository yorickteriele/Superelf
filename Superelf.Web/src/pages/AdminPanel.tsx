import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { isAdmin } from '../utils/auth';
import { 
  adminService, 
  AdminUser, 
  AdminStats 
} from '../services/adminService';
import { Navigate } from 'react-router-dom';
import { PlayerManagement } from '../components/admin/PlayerManagement';
import { ClubManagement } from '../components/admin/ClubManagement';
import { MatchManagement } from '../components/admin/MatchManagement';
import { LeagueManagement } from '../components/admin/LeagueManagement';
import { showConfirmDialog } from '../components/common/ConfirmDialog';

const AdminPanel: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<'dashboard' | 'users' | 'players' | 'clubs' | 'matches' | 'leagues' | 'maintenance'>('dashboard');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // State for different sections
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [stats, setStats] = useState<AdminStats | null>(null);

  // Confirmation dialog state
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [confirmMessage, setConfirmMessage] = useState('');
  const [onConfirm, setOnConfirm] = useState<(() => void) | null>(null);
  

  // Always call hooks at the top level
  useEffect(() => {
    // Only load data if user is admin
    if (user && isAdmin(user)) {
      loadInitialData();
    }
  }, [activeTab, user]);

  useEffect(() => {
    if (error || success) {
      const timer = setTimeout(() => {
        setError(null);
        setSuccess(null);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [error, success]);

  // Check if user is admin
  if (!user || !isAdmin(user)) {
    return <Navigate to="/login" replace />;
  }

  const loadInitialData = async () => {
    if (activeTab === 'dashboard') {
      await loadStats();
    } else if (activeTab === 'users') {
      await loadUsers();
    }
  };

  const loadStats = async () => {
    setLoading(true);
    try {
      const statsData = await adminService.getStats();
      setStats(statsData);
    } catch (err: any) {
      setError(err.message || 'Failed to load statistics');
    } finally {
      setLoading(false);
    }
  };

  const loadUsers = async () => {
    setLoading(true);
    try {
      const usersData = await adminService.getAllUsers();
      setUsers(usersData);
    } catch (err: any) {
      setError(err.message || 'Failed to load users');
    } finally {
      setLoading(false);
    }
  };

  // Confirmation dialog handlers

  const handleMakeAdmin = async (userId: string) => {
    const confirmed = await showConfirmDialog('Are you sure you want to make this user an admin?');
    if (!confirmed) return;
    try {
      await adminService.makeUserAdmin(userId);
      setSuccess('User promoted to admin successfully');
      await loadUsers();
    } catch (err: any) {
      setError(err.message || 'Failed to make user admin');
    }
  };

  const handleRemoveAdmin = async (userId: string) => {
    const confirmed = await showConfirmDialog('Are you sure you want to remove admin privileges from this user?');
    if (!confirmed) return;
    try {
      await adminService.removeUserAdmin(userId);
      setSuccess('Admin privileges removed successfully');
      await loadUsers();
    } catch (err: any) {
      setError(err.message || 'Failed to remove admin privileges');
    }
  };

  const handleDeleteUser = async (userId: string) => {
    const confirmed = await showConfirmDialog('Are you sure you want to delete this user? This action cannot be undone.');
    if (!confirmed) return;
    try {
      await adminService.deleteUser(userId);
      setSuccess('User deleted successfully');
      await loadUsers();
    } catch (err: any) {
      setError(err.message || 'Failed to delete user');
    }
  };

  const handleCleanupDatabase = async () => {
    const confirmed = await showConfirmDialog('Are you sure you want to cleanup the database? This will remove old incomplete data?');
    if (!confirmed) return;
    try {
      const result = await adminService.cleanupDatabase();
      setSuccess(result);
    } catch (err: any) {
      setError(err.message || 'Failed to cleanup database');
    }
  };

  const renderDashboard = () => (
    <div className="container-fluid">
      <div className="row mb-4">
        <div className="col-md-12">
          <h4>Admin Dashboard</h4>
          <p className="text-muted">Welcome to the Superelf administration panel</p>
        </div>
      </div>

      {loading ? (
        <div className="text-center">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : stats ? (
        <>
          {/* Statistics Cards */}
          <div className="row mb-4">
            <div className="col-md-3 col-sm-6 mb-3">
              <div className="card bg-primary text-white">
                <div className="card-body">
                  <div className="d-flex justify-content-between">
                    <div>
                      <h4>{stats.totalUsers}</h4>
                      <p className="mb-0">Total Users</p>
                    </div>
                    <div className="align-self-center">
                      <i className="fas fa-users fa-2x"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div className="col-md-3 col-sm-6 mb-3">
              <div className="card bg-success text-white">
                <div className="card-body">
                  <div className="d-flex justify-content-between">
                    <div>
                      <h4>{stats.totalPlayers}</h4>
                      <p className="mb-0">Total Players</p>
                    </div>
                    <div className="align-self-center">
                      <i className="fas fa-running fa-2x"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div className="col-md-3 col-sm-6 mb-3">
              <div className="card bg-info text-white">
                <div className="card-body">
                  <div className="d-flex justify-content-between">
                    <div>
                      <h4>{stats.totalClubs}</h4>
                      <p className="mb-0">Total Clubs</p>
                    </div>
                    <div className="align-self-center">
                      <i className="fas fa-shield-alt fa-2x"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div className="col-md-3 col-sm-6 mb-3">
              <div className="card bg-warning text-white">
                <div className="card-body">
                  <div className="d-flex justify-content-between">
                    <div>
                      <h4>{stats.totalMatches}</h4>
                      <p className="mb-0">Total Matches</p>
                      <small>({stats.completedMatches} completed)</small>
                    </div>
                    <div className="align-self-center">
                      <i className="fas fa-futbol fa-2x"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            {/* Players by Position */}
            <div className="col-md-4">
              <div className="card">
                <div className="card-header">
                  <h5>Players by Position</h5>
                </div>
                <div className="card-body">
                  {stats.playersByPosition.map(item => (
                    <div key={item.position} className="d-flex justify-content-between mb-2">
                      <span>{item.position}</span>
                      <span className="badge bg-primary">{item.count}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Top Nationalities */}
            <div className="col-md-4">
              <div className="card">
                <div className="card-header">
                  <h5>Top Nationalities</h5>
                </div>
                <div className="card-body">
                  {stats.playersByNationality.slice(0, 10).map(item => (
                    <div key={item.nationality} className="d-flex justify-content-between mb-2">
                      <span>{item.nationality}</span>
                      <span className="badge bg-success">{item.count}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Top Clubs */}
            <div className="col-md-4">
              <div className="card">
                <div className="card-header">
                  <h5>Top Clubs by Players</h5>
                </div>
                <div className="card-body">
                  {stats.playersByClub.slice(0, 10).map(item => (
                    <div key={item.club} className="d-flex justify-content-between mb-2">
                      <span>{item.club}</span>
                      <span className="badge bg-info">{item.count}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </>
      ) : null}
    </div>
  );

  const renderUsers = () => (
    <div className="container-fluid">
      <div className="row mb-4">
        <div className="col-md-12">
          <h4>User Management</h4>
        </div>
      </div>

      {loading ? (
        <div className="text-center">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="table table-hover">
                <thead>
                  <tr>
                    <th>Username</th>
                    <th>Email</th>
                    <th>Email Confirmed</th>
                    <th>Roles</th>
                    <th>Failed Attempts</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map(userData => (
                    <tr key={userData.id}>
                      <td>{userData.username}</td>
                      <td>{userData.email}</td>
                      <td>
                        <span className={`badge ${userData.emailConfirmed ? 'bg-success' : 'bg-warning'}`}>
                          {userData.emailConfirmed ? 'Confirmed' : 'Pending'}
                        </span>
                      </td>
                      <td>
                        {userData.roles.map(role => (
                          <span key={role} className="badge bg-primary me-1">{role}</span>
                        ))}
                      </td>
                      <td>
                        <span className={`badge ${userData.accessFailedCount > 0 ? 'bg-danger' : 'bg-success'}`}>
                          {userData.accessFailedCount}
                        </span>
                      </td>
                      <td>
                        <div className="btn-group" role="group">
                          {!userData.roles.includes('Admin') ? (
                            <button
                              className="btn btn-sm btn-outline-success"
                              onClick={() => handleMakeAdmin(userData.id)}
                            >
                              Make Admin
                            </button>
                          ) : (
                            <button
                              className="btn btn-sm btn-outline-warning"
                              onClick={() => handleRemoveAdmin(userData.id)}
                              disabled={userData.id === user?.id}
                            >
                              Remove Admin
                            </button>
                          )}
                          <button
                            className="btn btn-sm btn-outline-danger"
                            onClick={() => handleDeleteUser(userData.id)}
                            disabled={userData.id === user?.id}
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );

  const renderMaintenance = () => (
    <div className="container-fluid">
      <div className="row mb-4">
        <div className="col-md-12">
          <h4>System Maintenance</h4>
        </div>
      </div>

      <div className="row">
        <div className="col-md-6">
          <div className="card">
            <div className="card-header">
              <h5>Database Cleanup</h5>
            </div>
            <div className="card-body">
              <p>Remove old incomplete lineups and orphaned data to improve performance.</p>
              <button
                className="btn btn-warning"
                onClick={handleCleanupDatabase}
              >
                <i className="fas fa-broom me-2"></i>
                Cleanup Database
              </button>
            </div>
          </div>
        </div>
        
        <div className="col-md-6">
          <div className="card">
            <div className="card-header">
              <h5>System Information</h5>
            </div>
            <div className="card-body">
              <p><strong>Current User:</strong> {user?.username}</p>
              <p><strong>Role:</strong> Admin</p>
              <p><strong>Last Updated:</strong> {new Date().toLocaleString()}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );

  return (
    <div className="min-vh-100 bg-light">
      {/* Alerts */}
      {error && (
        <div className="alert alert-danger alert-dismissible fade show mx-3 mt-3" role="alert">
          <i className="fas fa-exclamation-triangle me-2"></i>
          {error}
          <button type="button" className="btn-close" onClick={() => setError(null)}></button>
        </div>
      )}

      {success && (
        <div className="alert alert-success alert-dismissible fade show mx-3 mt-3" role="alert">
          <i className="fas fa-check-circle me-2"></i>
          {success}
          <button type="button" className="btn-close" onClick={() => setSuccess(null)}></button>
        </div>
      )}

      {/* Confirmation Dialog removed: now using showConfirmDialog utility */}

      <div className="d-flex">
        {/* Sidebar */}
        <div style={{ width: '250px', minHeight: 'calc(100vh - 56px)' }}>
          <div className="list-group list-group-flush">
            <button
              className={`list-group-item list-group-item-action ${activeTab === 'dashboard' ? 'active' : ''}`}
              onClick={() => setActiveTab('dashboard')}
            >
              <i className="fas fa-tachometer-alt me-2"></i>
              Dashboard
            </button>
            <button
              className={`list-group-item list-group-item-action ${activeTab === 'users' ? 'active' : ''}`}
              onClick={() => setActiveTab('users')}
            >
              <i className="fas fa-users me-2"></i>
              Users
            </button>
            <button
              className={`list-group-item list-group-item-action ${activeTab === 'players' ? 'active' : ''}`}
              onClick={() => setActiveTab('players')}
            >
              <i className="fas fa-running me-2"></i>
              Players
            </button>
            <button
              className={`list-group-item list-group-item-action ${activeTab === 'clubs' ? 'active' : ''}`}
              onClick={() => setActiveTab('clubs')}
            >
              <i className="fas fa-shield-alt me-2"></i>
              Clubs
            </button>
            <button
              className={`list-group-item list-group-item-action ${activeTab === 'matches' ? 'active' : ''}`}
              onClick={() => setActiveTab('matches')}
            >
              <i className="fas fa-futbol me-2"></i>
              Matches
            </button>
            <button
              className={`list-group-item list-group-item-action ${activeTab === 'leagues' ? 'active' : ''}`}
              onClick={() => setActiveTab('leagues')}
            >
              <i className="fas fa-trophy me-2"></i>
              Leagues
            </button>
            <button
              className={`list-group-item list-group-item-action ${activeTab === 'maintenance' ? 'active' : ''}`}
              onClick={() => setActiveTab('maintenance')}
            >
              <i className="fas fa-tools me-2"></i>
              Maintenance
            </button>
          </div>
        </div>

        {/* Main Content */}
        <div className="flex-grow-1 p-4">
          {activeTab === 'dashboard' && renderDashboard()}
          {activeTab === 'users' && renderUsers()}
          {activeTab === 'players' && (
            <PlayerManagement 
              onError={setError} 
              onSuccess={setSuccess}
            />
          )}
          {activeTab === 'clubs' && (
            <ClubManagement 
              onError={setError} 
              onSuccess={setSuccess}
            />
          )}
          {activeTab === 'matches' && (
            <MatchManagement 
              onError={setError} 
              onSuccess={setSuccess}
            />
          )}
          {activeTab === 'leagues' && (
            <LeagueManagement 
              onError={setError} 
              onSuccess={setSuccess}
            />
          )}
          {activeTab === 'maintenance' && renderMaintenance()}
        </div>
      </div>
    </div>
  );
};

export default AdminPanel;
