import { Navigate, Outlet } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { authApi } from '../api/axios';

/**
 * HOC (Higher-Order Component) для защиты маршрутов.
 * Проверяет авторизацию через запрос /api/auth/me (cookie отправляется автоматически).
 * Неавторизованные пользователи перенаправляются на /login.
 */
export default function ProtectedRoute() {
  const [status, setStatus] = useState('loading');
  const [user, setUser] = useState(null);

  useEffect(() => {
    authApi
      .me()
      .then((res) => {
        setUser(res.data);
        setStatus('authenticated');
      })
      .catch(() => setStatus('unauthenticated'));
  }, []);

  if (status === 'loading') {
    return (
      <div className="loading-screen">
        <div className="spinner" />
        <p>Проверка авторизации...</p>
      </div>
    );
  }

  if (status === 'unauthenticated') {
    return <Navigate to="/login" replace />;
  }

  return <Outlet context={{ user }} />;
}
