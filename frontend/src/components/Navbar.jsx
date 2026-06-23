import { Link, useNavigate, useOutletContext } from 'react-router-dom';
import { authApi } from '../api/axios';

/**
 * Верхняя панель навигации с информацией о пользователе и кнопкой выхода.
 */
export default function Navbar() {
  const { user } = useOutletContext();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await authApi.logout();
    } finally {
      navigate('/login');
    }
  };

  return (
    <header className="navbar">
      <div className="navbar-inner">
        <Link to="/" className="navbar-brand">
          <span className="brand-icon">⬡</span>
          Network Inventory
        </Link>
        <div className="navbar-actions">
          <span className="user-badge">
            <span className="user-name">{user?.username}</span>
            <span className={`role-tag role-${user?.role?.toLowerCase()}`}>
              {user?.role}
            </span>
          </span>
          <button type="button" className="btn btn-ghost" onClick={handleLogout}>
            Выйти
          </button>
        </div>
      </div>
    </header>
  );
}
