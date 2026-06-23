import { useEffect, useState } from 'react';
import { Link, useNavigate, useOutletContext, useParams } from 'react-router-dom';
import { devicesApi } from '../api/axios';
import Navbar from '../components/Navbar';

/**
 * Страница детальной карточки устройства.
 * Отображает: основные данные, NetworkRole, таблицу интерфейсов, блок аудита.
 * Admin может редактировать и удалять устройство.
 */
export default function DeviceDetails() {
  const { id } = useParams();
  const { user } = useOutletContext();
  const navigate = useNavigate();
  const [device, setDevice] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState(null);
  const [saving, setSaving] = useState(false);

  const loadDevice = () => {
    setLoading(true);
    devicesApi
      .getById(id)
      .then((res) => {
        setDevice(res.data);
        setForm({
          name: res.data.name,
          type: res.data.type,
          hardwareModel: res.data.hardwareModel,
          networkRole: res.data.networkRole,
          networkInterfaces: res.data.networkInterfaces.map((i) => ({
            portName: i.portName,
            ipAddress: i.ipAddress,
            macAddress: i.macAddress,
            isActive: i.isActive,
          })),
        });
      })
      .catch(() => setError('Устройство не найдено'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadDevice();
  }, [id]);

  const formatDate = (dateStr) =>
    new Date(dateStr).toLocaleString('ru-RU', {
      day: '2-digit',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });

  const handleSave = async () => {
    setSaving(true);
    try {
      const res = await devicesApi.update(id, form);
      setDevice(res.data);
      setEditing(false);
    } catch (err) {
      setError(err.response?.data?.message || 'Ошибка сохранения');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm(`Удалить устройство "${device.name}"?`)) return;
    try {
      await devicesApi.delete(id);
      navigate('/');
    } catch {
      setError('Ошибка удаления');
    }
  };

  const updateInterface = (index, field, value) => {
    const updated = [...form.networkInterfaces];
    updated[index] = { ...updated[index], [field]: value };
    setForm({ ...form, networkInterfaces: updated });
  };

  if (loading) {
    return (
      <div className="page">
        <Navbar />
        <main className="container"><div className="loading-inline">Загрузка...</div></main>
      </div>
    );
  }

  if (error && !device) {
    return (
      <div className="page">
        <Navbar />
        <main className="container">
          <div className="alert alert-error">{error}</div>
          <Link to="/" className="btn btn-ghost">← Назад к списку</Link>
        </main>
      </div>
    );
  }

  return (
    <div className="page">
      <Navbar />
      <main className="container">
        <Link to="/" className="back-link">← Все устройства</Link>

        <div className="device-detail-header">
          <div>
            <span className={`device-type type-${device.type.toLowerCase()}`}>{device.type}</span>
            <h1>{device.name}</h1>
            <p className="hardware-model">{device.hardwareModel}</p>
          </div>
          {user?.role === 'Admin' && !editing && (
            <div className="action-buttons">
              <button type="button" className="btn btn-secondary" onClick={() => setEditing(true)}>
                Редактировать
              </button>
              <button type="button" className="btn btn-danger" onClick={handleDelete}>
                Удалить
              </button>
            </div>
          )}
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        {editing ? (
          <section className="card edit-form">
            <h2>Редактирование</h2>
            <div className="form-grid">
              <label>Название<input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} /></label>
              <label>Тип<input value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })} /></label>
              <label>Модель<input value={form.hardwareModel} onChange={(e) => setForm({ ...form, hardwareModel: e.target.value })} /></label>
              <label className="full-width">Роль в сети<textarea value={form.networkRole} onChange={(e) => setForm({ ...form, networkRole: e.target.value })} rows={2} /></label>
            </div>

            <h3>Интерфейсы</h3>
            {form.networkInterfaces.map((iface, idx) => (
              <div key={idx} className="interface-edit-row">
                <input placeholder="Порт" value={iface.portName} onChange={(e) => updateInterface(idx, 'portName', e.target.value)} />
                <input placeholder="IP" value={iface.ipAddress} onChange={(e) => updateInterface(idx, 'ipAddress', e.target.value)} />
                <input placeholder="MAC" value={iface.macAddress} onChange={(e) => updateInterface(idx, 'macAddress', e.target.value)} />
                <label className="checkbox-label">
                  <input type="checkbox" checked={iface.isActive} onChange={(e) => updateInterface(idx, 'isActive', e.target.checked)} />
                  Активен
                </label>
              </div>
            ))}

            <div className="form-actions">
              <button type="button" className="btn btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'Сохранение...' : 'Сохранить'}
              </button>
              <button type="button" className="btn btn-ghost" onClick={() => setEditing(false)}>Отмена</button>
            </div>
          </section>
        ) : (
          <>
            <section className="card">
              <h2>Роль в топологии сети</h2>
              <p className="network-role-detail">{device.networkRole}</p>
            </section>

            <section className="card">
              <h2>Сетевые интерфейсы</h2>
              <div className="table-wrapper">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Порт</th>
                      <th>IP-адрес</th>
                      <th>MAC-адрес</th>
                      <th>Статус</th>
                    </tr>
                  </thead>
                  <tbody>
                    {device.networkInterfaces.map((iface) => (
                      <tr key={iface.id}>
                        <td><code>{iface.portName}</code></td>
                        <td>{iface.ipAddress}</td>
                        <td><code>{iface.macAddress}</code></td>
                        <td>
                          <span className={`status-badge ${iface.isActive ? 'active' : 'inactive'}`}>
                            {iface.isActive ? 'Активен' : 'Неактивен'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>

            <section className="card audit-block">
              <h2>Аудит изменений</h2>
              <p className="audit-description">
                Принцип Accountability (подотчётность): каждое изменение фиксируется автоматически
                в <code>AppDbContext.SaveChangesAsync</code>.
              </p>
              <div className="audit-info">
                <div className="audit-item">
                  <span className="audit-label">Последнее изменение</span>
                  <span className="audit-value">{formatDate(device.lastModifiedDate)}</span>
                </div>
                <div className="audit-item">
                  <span className="audit-label">Изменил сотрудник</span>
                  <span className="audit-value">{device.lastModifiedByUsername}</span>
                </div>
              </div>
            </section>
          </>
        )}
      </main>
    </div>
  );
}
