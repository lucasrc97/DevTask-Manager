import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Configuração do Vite com proxy para a API .NET 8
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:5088",
        changeOrigin: true,
      },
    },
  },
});
