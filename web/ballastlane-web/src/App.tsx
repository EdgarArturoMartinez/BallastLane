import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import LoginPage    from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import TasksPage    from './pages/TasksPage';
import './index.css';

/** Redirects unauthenticated users to /login. */
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  if (isLoading) return <div className="loading-screen">Loading…</div>;
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

function AppRoutes() {
  const { isAuthenticated } = useAuth();
  return (
    <Routes>
      <Route path="/login"    element={isAuthenticated ? <Navigate to="/tasks" replace /> : <LoginPage />} />
      <Route path="/register" element={isAuthenticated ? <Navigate to="/tasks" replace /> : <RegisterPage />} />
      <Route path="/tasks"    element={<ProtectedRoute><TasksPage /></ProtectedRoute>} />
      <Route path="*"         element={<Navigate to="/tasks" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}

