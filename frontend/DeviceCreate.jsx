import { useState } from 'react';
import { Link, useNavigate, useOutletContext } from 'react-router-dom';
import { devicesApi } from '../api/axios';
import Navbar from '../components/Navbar';

/**
 * Страница создания нового сетевого устройства.
 * Доступна только для Admin (проверка user.role).
 * Реализует отправку данных на POST /api/devices.
 */
export default function DeviceCreate() {
  const { user } = useOutletContext();
  const navigate = useNavigate();
  
  const [form, setForm] = useState({
    name: '',
    type: 'Router',
    hardwareModel: '',
    networkRole: '',
    networkInterfaces: [],
  });
  
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  // Добавление пустого интерфейса в форму
  const addInterface = () => {
    setForm({
      ...form,
      networkInterfaces: [
        ...form.networkInterfaces,
        { portName: '', ipAddress: '', macAddress: '', isActive: true },
      ],
    });
  };

  const removeInterface = (index) => {
    const updated = form.networkInterfaces.filter((_, i) => i !== index);
    setForm({ ...form, networkInterfaces: updated });
  };

  const updateInterface = (index, field, value) => {
    const updated = [...form.networkInterfaces];
    updated[index] = { ...updated[index], [field]: value };
    setForm({ ...form, networkInterfaces: updated });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      // Отправка DTO на сервер
      const res = await devicesApi.create(form);
      // После создания переходим на карточку нового устройства
      navigate(`/devices/${res.data.id}`);
    } catch (err) {
      setError(err.response?.data?.message || 'Ошибка при создании устройства');
    } finally {
      setSaving(false);
    }
  };

  // Проверка RBAC на уровне UI
  if (user?.role !== 'Admin') {
    return (
      <div className="page">
        <Navbar />
        <main className="container">
          <div className="alert alert-error">Доступ запрещен. Только администраторы могут добавлять устройства.</div>
          <Link to="/" className="btn btn-ghost">← Назад к списку</Link>
        </main>
      </div>
    );
  }

  return (
    <div className="page">
      <Navbar />
      <main className="container">
        <Link to="/" className="back-link">← Отмена</Link>
        <h1>Новое устройство</h1>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit} className="card edit-form">
          <div className="form-grid">
            <label>Название<input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="Напр: GW-CORE-01" /></label>
            <label>Тип
              <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })}>
                <option value="Router">Router</option>
                <option value="Switch">Switch</option>
                <option value="Firewall">Firewall</option>
                <option value="Access Point">Access Point</option>
              </select>
            </label>
            <label>Модель<input required value={form.hardwareModel} onChange={(e) => setForm({ ...form, hardwareModel: e.target.value })} placeholder="Напр: Cisco ISR 4451" /></label>
            <label className="full-width">Роль в сети<textarea required value={form.networkRole} onChange={(e) => setForm({ ...form, networkRole: e.target.value })} rows={2} placeholder="Описание назначения в топологии..." /></label>
          </div>

          <div className="section-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '1.5rem', marginBottom: '1rem' }}>
            <h3>Сетевые интерфейсы</h3>
            <button type="button" className="btn btn-secondary btn-sm" onClick={addInterface}>
              + Добавить порт
            </button>
          </div>

          {form.networkInterfaces.length === 0 ? (
            <p className="hint-text">Интерфейсы еще не добавлены. Нажмите кнопку выше.</p>
          ) : (
            form.networkInterfaces.map((iface, idx) => (
              <div key={idx} className="interface-edit-row">
                <input required placeholder="Порт" value={iface.portName} onChange={(e) => updateInterface(idx, 'portName', e.target.value)} />
                <input required placeholder="IP" value={iface.ipAddress} onChange={(e) => updateInterface(idx, 'ipAddress', e.target.value)} />
                <input required placeholder="MAC" value={iface.macAddress} onChange={(e) => updateInterface(idx, 'macAddress', e.target.value)} />
                <label className="checkbox-label">
                  <input type="checkbox" checked={iface.isActive} onChange={(e) => updateInterface(idx, 'isActive', e.target.checked)} />
                  Активен
                </label>
                <button type="button" className="btn btn-danger btn-sm" onClick={() => removeInterface(idx)}>×</button>
              </div>
            ))
          )}

          <div className="form-actions" style={{ marginTop: '2rem', borderTop: '1px solid #eee', paddingTop: '1.5rem' }}>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Создание...' : 'Создать устройство'}
            </button>
            <Link to="/" className="btn btn-ghost">Отмена</Link>
          </div>
        </form>
      </main>
    </div>
  );
}