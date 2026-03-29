import { useState, useEffect } from 'react'
import { inventoryApi } from '../services/api'

function Inventory() {
  const [inventory, setInventory] = useState([])
  const [reservations, setReservations] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [editingProduct, setEditingProduct] = useState(null)
  const [editStock, setEditStock] = useState(0)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      const [invRes, resRes] = await Promise.all([
        inventoryApi.getAll(),
        inventoryApi.getReservations()
      ])
      setInventory(invRes.data)
      setReservations(resRes.data)
    } catch (err) {
      setError('Failed to load inventory data')
    } finally {
      setLoading(false)
    }
  }

  const handleEditClick = (item) => {
    setEditingProduct(item)
    setEditStock(item.stockQuantity)
  }

  const handleCancelEdit = () => {
    setEditingProduct(null)
    setEditStock(0)
  }

  const handleSaveStock = async () => {
    if (!editingProduct) return
    setSaving(true)
    try {
      await inventoryApi.updateStock(editingProduct.productId, editStock)
      // Update local state
      setInventory(inventory.map(item => 
        item.productId === editingProduct.productId 
          ? { ...item, stockQuantity: editStock, availableQuantity: editStock - item.reservedQuantity }
          : item
      ))
      setEditingProduct(null)
    } catch (err) {
      setError('Failed to update stock')
    } finally {
      setSaving(false)
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
        <h1>Inventory</h1>
        <p className="text-muted">Manage product inventory and stock levels</p>
      </div>

      <div className="row">
        <div className="col-md-8">
          <div className="card">
            <div className="card-header">Product Inventory</div>
            <div className="card-body">
              <div className="table-responsive">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Product ID</th>
                      <th>Name</th>
                      <th>Category</th>
                      <th>Price</th>
                      <th>Stock</th>
                      <th>Reserved</th>
                      <th>Available</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {inventory.map(item => (
                      <tr key={item.productId}>
                        <td>{item.productId}</td>
                        <td>{item.productName}</td>
                        <td>{item.category}</td>
                        <td>${item.price?.toLocaleString()}</td>
                        <td>{item.stockQuantity}</td>
                        <td>{item.reservedQuantity}</td>
                        <td>
                          <span className={item.availableQuantity < 5 ? 'text-danger fw-bold' : ''}>
                            {item.availableQuantity}
                          </span>
                        </td>
                        <td>
                          <button 
                            className="btn btn-sm btn-outline-primary"
                            onClick={() => handleEditClick(item)}
                          >
                            Edit Stock
                          </button>
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
            <div className="card-header">Recent Reservations</div>
            <div className="card-body">
              {reservations.length === 0 ? (
                <p className="text-muted">No active reservations</p>
              ) : (
                <div style={{ maxHeight: '400px', overflowY: 'auto' }}>
                  {reservations.slice(0, 10).map(res => (
                    <div key={res.reservationId} className="border-bottom pb-2 mb-2">
                      <div className="d-flex justify-content-between">
                        <span className="fw-bold">Order #{res.orderId}</span>
                        <span className={`badge ${res.status === 'Confirmed' ? 'bg-success' : 'bg-warning'}`}>
                          {res.status}
                        </span>
                      </div>
                      <small className="text-muted">
                        {res.productName} x {res.quantity}
                      </small>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Edit Stock Modal */}
      {editingProduct && (
        <div className="modal fade show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Stock - {editingProduct.productName}</h5>
                <button type="button" className="btn-close" onClick={handleCancelEdit}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Current Stock Quantity</label>
                  <input 
                    type="number" 
                    className="form-control" 
                    value={editStock}
                    onChange={(e) => setEditStock(parseInt(e.target.value) || 0)}
                    min="0"
                  />
                </div>
                <div className="alert alert-info">
                  <strong>Reserved:</strong> {editingProduct.reservedQuantity} | 
                  <strong> Available after update:</strong> {editStock - editingProduct.reservedQuantity}
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={handleCancelEdit}>Cancel</button>
                <button 
                  type="button" 
                  className="btn btn-primary" 
                  onClick={handleSaveStock}
                  disabled={saving}
                >
                  {saving ? 'Saving...' : 'Save Changes'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default Inventory
