import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { orderApi } from '../services/api'

function Orders() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [page, setPage] = useState(1)

  useEffect(() => {
    loadOrders()
  }, [page])

  const loadOrders = async () => {
    try {
      setLoading(true)
      const response = await orderApi.getAll(page, 10)
      setOrders(response.data.items || response.data.orders || response.data)
    } catch (err) {
      console.error('Error loading orders:', err)
      setError('Failed to load orders')
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = async (id) => {
    if (!window.confirm('Are you sure you want to cancel this order?')) return
    
    try {
      await orderApi.cancel(id)
      loadOrders()
    } catch (err) {
      alert('Failed to cancel order')
    }
  }

  const getStatusBadge = (status) => {
    const statusLower = status?.toLowerCase() || ''
    const badgeClass = {
      'pending': 'badge-pending',
      'inventorypending': 'badge-inventory',
      'paymentpending': 'badge-payment',
      'shipped': 'badge-shipped',
      'completed': 'badge-completed',
      'cancelled': 'badge-cancelled',
      'failed': 'badge-failed'
    }[statusLower] || 'bg-secondary'
    
    return <span className={`badge ${badgeClass}`}>{status}</span>
  }

  const formatDate = (date) => {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
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

  return (
    <div>
      <div className="page-header">
        <h1>Orders</h1>
        <p className="text-muted">Manage customer orders</p>
      </div>

      <div className="card">
        <div className="card-body">
          <div className="table-responsive">
            <table className="table table-hover">
              <thead>
                <tr>
                  <th>Order #</th>
                  <th>Customer</th>
                  <th>Email</th>
                  <th>Status</th>
                  <th>Total</th>
                  <th>Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map(order => (
                  <tr key={order.orderId}>
                    <td>
                      <Link to={`/orders/${order.orderId}`}>
                        #{order.orderId}
                      </Link>
                    </td>
                    <td>{order.customerName}</td>
                    <td>{order.email || 'N/A'}</td>
                    <td>{getStatusBadge(order.status)}</td>
                    <td>${order.totalAmount?.toLocaleString() || order.total?.toLocaleString() || '0.00'}</td>
                    <td>{formatDate(order.createdAt || order.orderDate)}</td>
                    <td>
                      <Link 
                        to={`/orders/${order.orderId}`} 
                        className="btn btn-sm btn-outline-primary btn-action me-1"
                      >
                        View
                      </Link>
                      {order.status?.toLowerCase() === 'pending' && (
                        <button 
                          className="btn btn-sm btn-outline-danger btn-action"
                          onClick={() => handleCancel(order.orderId)}
                        >
                          Cancel
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          
          <div className="d-flex justify-content-between mt-3">
            <button 
              className="btn btn-outline-secondary"
              disabled={page === 1}
              onClick={() => setPage(p => p - 1)}
            >
              Previous
            </button>
            <span className="align-self-center">Page {page}</span>
            <button 
              className="btn btn-outline-secondary"
              onClick={() => setPage(p => p + 1)}
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Orders
