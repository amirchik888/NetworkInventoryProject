import { useEffect, useState } from 'react';
import { Link, useOutletContext } from 'react-router-dom';
import { devicesApi } from '../api/axios';
import Navbar from '../components/Navbar';

/**
 * Dashboard — главная страница со списком сетевого оборудования.
 * Отображает краткую информацию и данные аудита (кто/когда изменял).
 */
export default function Dashboard() {
  const { user } = useOutletContext();
  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    devicesApi
      .getAll()
      .then((res) => setDevices(res.data))
      .catch(() => setError('Не удалось загрузить список устройств'))
      .finally(() => setLoading(false));
  }, []);

  const formatDate = (dateStr) =>
    new Date(dateStr).toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });

  return (
    <div className="page">
      <Navbar />
      <main className="container">
        <div className="page-header">
          <div>
            <h1>Сетевое оборудование</h1>
            <p className="subtitle">Инвентаризация и учёт сетевых устройств организации</p>
          </div>
          {user?.role === 'Admin' && (
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: '10px' }}>
              <span className="admin-notice">Режим администратора: доступно редактирование</span>
              <Link to="/devices/new" className="btn btn-primary">+ Добавить устройство</Link>
            </div>
          )}
        </div>

        {loading && <div className="loading-inline">Загрузка...</div>}
        {error && <div className="alert alert-error">{error}</div>}

        {!loading && !error && (
          <div className="device-grid">
            {devices.map((device) => (
              <Link to={`/devices/${device.id}`} key={device.id} className="device-card">
                <div className="device-card-header">
                  <span className={`device-type type-${device.type.toLowerCase().replace(/\s/g, '-')}`}>
                    {device.type}
                  </span>
                  <span className="interface-count">{device.interfaceCount} портов</span>
                </div>
                <h3>{device.name}</h3>
                <p className="network-role">{device.networkRole}</p>
                <div className="audit-preview">
                  <span>Изменено: {formatDate(device.lastModifiedDate)}</span>
                  <span>Кем: {device.lastModifiedByUsername}</span>
                </div>
              </Link>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}
