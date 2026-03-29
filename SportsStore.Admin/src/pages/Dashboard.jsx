import { useState, useEffect } from 'react'
import { orderApi } from '../services/api'

function Dashboard() {
  const [dashboard, setDashboard] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    loadDashboard()
  }, [])

  const loadDashboard = async () => {
    try {
      setLoading(true)
      const response = await orderApi.getDashboard()
      setDashboard(response.data)
    } catch (err) {
      setError('Failed to load dashboard data')
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="loading-spinner">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    )
  }

  if (error) {
    return <div className="alert alert-danger">{error}</div>
  }

  const stats = dashboard || {}

  return (
    <div>
      <div className="page-header">
        <h1>Dashboard</h1>
        <p className="text-muted">Overview of your store performance</p>
      </div>

      <div className="row">
        <div className="col-md-3">
          <div className="card">
            <div className="stat-card">
              <div className="stat-value">{stats.totalOrders || 0}</div>
              <div className="stat-label">Total Orders</div>
            </div>
          </div>
        </div>
        <div className="col-md-3">
          <div className="card">
            <div className="stat-card">
              <div className="stat-value text-success">
                ${(stats.totalRevenue || 0).toLocaleString()}
              </div>
              <div className="stat-label">Total Revenue</div>
            </div>
          </div>
        </div>
        <div className="col-md-3">
          <div className="card">
            <div className="stat-card">
              <div className="stat-value text-warning">{stats.pendingOrders || 0}</div>
              <div className="stat-label">Pending Orders</div>
            </div>
          </div>
        </div>
        <div className="col-md-3">
          <div className="card">
            <div className="stat-card">
              <div className="stat-value text-success">{stats.completedOrders || 0}</div>
              <div className="stat-label">Completed Orders</div>
            </div>
          </div>
        </div>
      </div>

      <div className="row mt-4">
        <div className="col-md-6">
          <div className="card">
            <div className="card-header">Orders by Status</div>
            <div className="card-body">
              <table className="table">
                <tbody>
                  <tr>
                    <td>Pending</td>
                    <td><span className="badge badge-pending">{stats.pendingOrders || 0}</span></td>
                  </tr>
                  <tr>
                    <td>Inventory Pending</td>
                    <td><span className="badge badge-inventory">{stats.inventoryPendingOrders || 0}</span></td>
                  </tr>
                  <tr>
                    <td>Payment Pending</td>
                    <td><span className="badge badge-payment">{stats.paymentPendingOrders || 0}</span></td>
                  </tr>
                  <tr>
                    <td>Shipped</td>
                    <td><span className="badge badge-shipped">{stats.shippedOrders || 0}</span></td>
                  </tr>
                  <tr>
                    <td>Completed</td>
                    <td><span className="badge badge-completed">{stats.completedOrders || 0}</span></td>
                  </tr>
                  <tr>
                    <td>Cancelled</td>
                    <td><span className="badge badge-cancelled">{stats.cancelledOrders || 0}</span></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
        <div className="col-md-6">
          <div className="card">
            <div className="card-header">Quick Actions</div>
            <div className="card-body">
              <div className="d-grid gap-2">
                <a href="/orders" className="btn btn-outline-primary">View All Orders</a>
                <a href="/inventory" className="btn btn-outline-primary">Check Inventory</a>
                <a href="/payments" className="btn btn-outline-primary">View Payments</a>
                <a href="/shipments" className="btn btn-outline-primary">Track Shipments</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Dashboard
