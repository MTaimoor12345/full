import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import Dashboard from './pages/Dashboard'
import Orders from './pages/Orders'
import OrderDetail from './pages/OrderDetail'
import Inventory from './pages/Inventory'
import Payments from './pages/Payments'
import Shipments from './pages/Shipments'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="orders" element={<Orders />} />
          <Route path="orders/:id" element={<OrderDetail />} />
          <Route path="inventory" element={<Inventory />} />
          <Route path="payments" element={<Payments />} />
          <Route path="shipments" element={<Shipments />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
