import axios from 'axios';

/**
 * Настроенный экземпляр Axios для взаимодействия с REST API.
 *
 * withCredentials: true — браузер автоматически отправляет HttpOnly cookies
 * с каждым запросом. Это ключевой механизм передачи JWT без хранения токена
 * в localStorage/sessionStorage (защита от XSS).
 *
 * baseURL: в dev-режиме — прямой URL backend (http://localhost:5007/api).
 * Прямое обращение + CORS AllowCredentials гарантирует корректную работу HttpOnly cookie
 * (при Vite-прокси cookie может привязаться к неверному origin).
 */
const api = axios.create({
  baseURL: import.meta.env.DEV ? 'http://localhost:5007/api' : '/api',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Перехватчик ответов: при 401 перенаправляем на страницу входа
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !window.location.pathname.includes('/login')) {
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  login: (username, password) =>
    api.post('/auth/login', { username, password }),
  logout: () => api.post('/auth/logout'),
  me: () => api.get('/auth/me'),
};

export const devicesApi = {
  getAll: () => api.get('/devices'),
  getById: (id) => api.get(`/devices/${id}`),
  create: (data) => api.post('/devices', data),
  update: (id, data) => api.put(`/devices/${id}`, data),
  delete: (id) => api.delete(`/devices/${id}`),
};

export default api;
