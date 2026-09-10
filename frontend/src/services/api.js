import axios from "axios";

// Base URL da API .NET 8
// Deve bater com a porta do launchSettings.json do backend (http://localhost:5246)
const api = axios.create({
  baseURL: "http://localhost:5246",
});

// Interceptor: injeta o token JWT do localStorage em toda requisição
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor: redireciona para o login em caso de 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default api;
