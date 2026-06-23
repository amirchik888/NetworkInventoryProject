import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../api/axios';

/**
 * Страница входа в систему.
 * После успешного login JWT сохраняется в HttpOnly cookie сервером —
 * фронтенд НЕ получает и НЕ хранит токен в JavaScript.
 */
export default function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await authApi.login(username, password);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message || 'Ошибка входа. Проверьте учётные данные.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <span className="login-icon">⬡</span>
          <h1>Система учёта сетевого оборудования</h1>
          <p>Индивидуальное задание №11 — Артыков Амирбек, БИС 3-24</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          {error && <div className="alert alert-error">{error}</div>}

          <label>
            Имя пользователя
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="admin"
              required
              autoComplete="username"
            />
          </label>

          <label>
            Пароль
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
              autoComplete="current-password"
            />
          </label>

          <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
            {loading ? 'Вход...' : 'Войти'}
          </button>
        </form>

        <div className="login-hint">
          <p><strong>Тестовые учётные записи:</strong></p>
          <p>Admin: <code>admin</code> / <code>Admin123!</code></p>
          <p>User: <code>operator</code> / <code>Operator123!</code></p>
        </div>
      </div>
    </div>
  );
}
