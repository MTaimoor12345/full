import { useState, useEffect } from 'react'
import { paymentApi } from '../services/api'

function Payments() {
  const [transactions, setTransactions] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [statusFilter, setStatusFilter] = useState('')

  useEffect(() => {
    loadTransactions()
  }, [statusFilter])

  const loadTransactions = async () => {
    try {
      setLoading(true)
      const response = await paymentApi.getTransactions(null, statusFilter || null)
      // API returns { value: [...], Count: n } format
      setTransactions(response.data.value || response.data || [])
    } catch (err) {
      setError('Failed to load payment transactions')
    } finally {
      setLoading(false)
    }
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
        <h1>Payments</h1>
        <p className="text-muted">View and manage payment transactions</p>
      </div>

      <div className="card">
        <div className="card-header d-flex justify-content-between align-items-center">
          <span>Payment Transactions</span>
          <select 
            className="form-select w-auto" 
            value={statusFilter} 
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
          </select>
        </div>
        <div className="card-body">
          <div className="table-responsive">
            <table className="table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Order #</th>
                  <th>Customer</th>
                  <th>Amount</th>
                  <th>Status</th>
                  <th>Reference</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map(tx => (
                  <tr key={tx.transactionId}>
                    <td>{tx.transactionId}</td>
                    <td>#{tx.orderId}</td>
                    <td>{tx.customerId}</td>
                    <td>${tx.amount?.toLocaleString() || '0.00'}</td>
                    <td>
                      <span className={`badge ${
                        tx.status === 'Completed' ? 'bg-success' :
                        tx.status === 'Failed' ? 'bg-danger' :
                        tx.status === 'Pending' ? 'bg-warning' : 'bg-secondary'
                      }`}>
                        {tx.status}
                      </span>
                    </td>
                    <td>
                      {tx.transactionReference || 
                        <span className="text-muted fst-italic">{tx.rejectionReason}</span>
                      }
                    </td>
                    <td>{formatDate(tx.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Payments
