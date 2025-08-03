import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
// Using Bootstrap with our custom dark theme
import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css';

// Context
import { AuthProvider } from './context/AuthContext';

// Components
import Navbar from './components/Navbar';
import Footer from './components/Footer';
import ProtectedRoute from './components/ProtectedRoute';

// Pages
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import NotFound from './pages/NotFound';
import Pools from './pages/Pools';
import PoolDetail from './pages/PoolDetail';
import Selection from './pages/Selection';

function App() {
  // Add data-bs-theme attribute to body
  React.useEffect(() => {
    document.body.setAttribute('data-bs-theme', 'dark');
    return () => {
      document.body.removeAttribute('data-bs-theme');
    };
  }, []);

  return (
    <AuthProvider>
      <Router>
        <div className="d-flex flex-column min-vh-100">
          <Navbar />
          <main className="flex-grow-1">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              
              {/* Protected Routes */}
              <Route element={<ProtectedRoute />}>
                <Route path="/pools" element={<Pools />} />
                <Route path="/pools/:poolId" element={<PoolDetail />} />
                <Route path="/pools/:poolId/selection" element={<Selection />} />
                <Route path="/pools/:poolId/selection/:userId" element={<Selection />} />
              </Route>
              
              {/* 404 Page */}
              <Route path="*" element={<NotFound />} />
            </Routes>
          </main>
          <Footer />
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
