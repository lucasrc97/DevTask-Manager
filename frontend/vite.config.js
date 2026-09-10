import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Configuração do Vite com proxy para a API .NET 8
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        // Porta do backend corrigida para 5246 (mesma do launchSettings.json)
        target: "http://localhost:5246",
        changeOrigin: true,
      },
    },
  },
});
