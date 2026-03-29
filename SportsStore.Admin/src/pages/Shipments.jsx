import { useState, useEffect } from 'react'
import { shippingApi } from '../services/api'

function Shipments() {
  const [shipments, setShipments] = useState([])
  const [carriers, setCarriers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [statusFilter, setStatusFilter] = useState('')
  const [actionLoading, setActionLoading] = useState(null)

  useEffect(() => {
    loadData()
  }, [statusFilter])

  const loadData = async () => {
    try {
      setLoading(true)
      const [shipRes, carrierRes] = await Promise.all([
        shippingApi.getShipments(null, statusFilter || null),
        shippingApi.getCarriers()
      ])
      setShipments(shipRes.data)
      setCarriers(carrierRes.data)
    } catch (err) {
      setError('Failed to load shipment data')
    } finally {
      setLoading(false)
    }
  }

  const formatDate = (date) => {
    if (!date) return '-'
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    })
  }

  const handleDispatch = async (shipmentId) => {
    try {
      setActionLoading(shipmentId)
      await shippingApi.dispatch(shipmentId)
      setShipments(shipments.map(s => 
        s.shipmentId === shipmentId 
          ? { ...s, status: 'Dispatched', actualDispatchDate: new Date().toISOString() }
          : s
      ))
    } catch (err) {
      setError('Failed to dispatch shipment')
    } finally {
      setActionLoading(null)
    }
  }

  const handleDeliver = async (shipmentId) => {
    try {
      setActionLoading(shipmentId)
      await shippingApi.deliver(shipmentId)
      setShipments(shipments.map(s => 
        s.shipmentId === shipmentId 
          ? { ...s, status: 'Delivered', actualDeliveryDate: new Date().toISOString() }
          : s
      ))
    } catch (err) {
      setError('Failed to mark as delivered')
    } finally {
      setActionLoading(null)
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

  return (
    <div>
      <div className="page-header">
        <h1>Shipments</h1>
        <p className="text-muted">Track and manage order shipments</p>
      </div>

      <div className="row">
        <div className="col-md-8">
          <div className="card">
            <div className="card-header d-flex justify-content-between align-items-center">
              <span>Shipments</span>
              <select 
                className="form-select w-auto" 
                value={statusFilter} 
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="">All Statuses</option>
                <option value="Created">Created</option>
                <option value="Dispatched">Dispatched</option>
                <option value="InTransit">In Transit</option>
                <option value="Delivered">Delivered</option>
              </select>
            </div>
            <div className="card-body">
              <div className="table-responsive">
                <table className="table">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Order #</th>
                      <th>Tracking #</th>
                      <th>Carrier</th>
                      <th>Status</th>
                      <th>Est. Delivery</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {shipments.map(ship => (
                      <tr key={ship.shipmentId}>
                        <td>{ship.shipmentId}</td>
                        <td>#{ship.orderId}</td>
                        <td>
                          <code>{ship.trackingNumber}</code>
                        </td>
                        <td>{ship.carrier}</td>
                        <td>
                          <span className={`badge ${
                            ship.status === 'Delivered' ? 'bg-success' :
                            ship.status === 'InTransit' ? 'bg-primary' :
                            ship.status === 'Dispatched' ? 'bg-info' : 'bg-secondary'
                          }`}>
                            {ship.status}
                          </span>
                        </td>
                        <td>{formatDate(ship.estimatedDeliveryDate)}</td>
                        <td>
                          {ship.status === 'Created' && (
                            <button 
                              className="btn btn-sm btn-primary"
                              onClick={() => handleDispatch(ship.shipmentId)}
                              disabled={actionLoading === ship.shipmentId}
                            >
                              {actionLoading === ship.shipmentId ? 'Processing...' : 'Dispatch'}
                            </button>
                          )}
                          {ship.status === 'Dispatched' && (
                            <button 
                              className="btn btn-sm btn-success"
                              onClick={() => handleDeliver(ship.shipmentId)}
                              disabled={actionLoading === ship.shipmentId}
                            >
                              {actionLoading === ship.shipmentId ? 'Processing...' : 'Deliver'}
                            </button>
                          )}
                          {ship.status === 'Delivered' && (
                            <span className="text-muted">Completed</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card">
            <div className="card-header">Available Carriers</div>
            <div className="card-body">
              {carriers.map(carrier => (
                <div key={carrier.carrierId} className="mb-3 pb-3 border-bottom">
                  <h6 className="mb-1">{carrier.name}</h6>
                  <small className="text-muted">{carrier.description}</small>
                  <div className="mt-1">
                    <span className="me-3">${carrier.baseCost?.toLocaleString()}</span>
                    <span className="text-muted">{carrier.estimatedDays} days</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Shipments
