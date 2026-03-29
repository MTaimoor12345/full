import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api/orders': {
        target: 'http://localhost:5138',
        changeOrigin: true
      },
      '/api/products': {
        target: 'http://localhost:5138',
        changeOrigin: true
      },
      '/api/inventory': {
        target: 'http://localhost:5139',
        changeOrigin: true
      },
      '/api/payment': {
        target: 'http://localhost:5140',
        changeOrigin: true
      },
      '/api/shipping': {
        target: 'http://localhost:5141',
        changeOrigin: true
      }
    }
  }
})
