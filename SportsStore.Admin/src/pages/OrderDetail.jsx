import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { orderApi } from '../services/api'

function OrderDetail() {
  const { id } = useParams()
  const [order, setOrder] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    loadOrder()
  }, [id])

  const loadOrder = async () => {
    try {
      setLoading(true)
      const response = await orderApi.getById(id)
      setOrder(response.data)
    } catch (err) {
      setError('Failed to load order')
    } finally {
      setLoading(false)
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
      month: 'long',
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

  if (error || !order) {
    return (
      <div className="alert alert-danger">
        {error || 'Order not found'}
        <Link to="/orders" className="btn btn-outline-primary ms-3">Back to Orders</Link>
      </div>
    )
  }

  return (
    <div>
      <div className="page-header">
        <nav aria-label="breadcrumb">
          <ol className="breadcrumb">
            <li className="breadcrumb-item"><Link to="/orders">Orders</Link></li>
            <li className="breadcrumb-item active">Order #{order.orderId}</li>
          </ol>
        </nav>
        <h1>Order #{order.orderId}</h1>
        <p className="text-muted">Placed on {formatDate(order.createdAt)}</p>
      </div>

      <div className="row">
        <div className="col-md-8">
          <div className="card">
            <div className="card-header d-flex justify-content-between align-items-center">
              <span>Order Items</span>
              {getStatusBadge(order.status)}
            </div>
            <div className="card-body">
              <table className="table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Quantity</th>
                    <th>Price</th>
                    <th>Total</th>
                  </tr>
                </thead>
                <tbody>
                  {order.items?.map((item, idx) => (
                    <tr key={idx}>
                      <td>{item.productName}</td>
                      <td>{item.quantity}</td>
                      <td>${item.productPrice?.toLocaleString() || '0.00'}</td>
                      <td>${item.lineTotal?.toLocaleString() || '0.00'}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr>
                    <td colSpan="3" className="text-end"><strong>Order Total:</strong></td>
                    <td><strong>${order.totalAmount?.toLocaleString() || '0.00'}</strong></td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card">
            <div className="card-header">Customer Information</div>
            <div className="card-body">
              <p><strong>{order.customerName}</strong></p>
              <p className="text-muted mb-0">{order.email}</p>
            </div>
          </div>

          {order.trackingNumber && (
            <div className="card mt-3">
              <div className="card-header">Shipping Information</div>
              <div className="card-body">
                <p><strong>Carrier:</strong> {order.carrier}</p>
                <p><strong>Tracking:</strong> {order.trackingNumber}</p>
              </div>
            </div>
          )}

          <div className="card mt-3">
            <div className="card-header">Order Status</div>
            <div className="card-body">
              <div className="mb-3">
                {getStatusBadge(order.status)}
              </div>
              <small className="text-muted">
                Status updates are sent to the customer's email.
              </small>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default OrderDetail
