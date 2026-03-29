import axios from 'axios'

const API_BASE = '/api'

// Order API
export const orderApi = {
  getAll: (page = 1, pageSize = 10) => 
    axios.get(`${API_BASE}/orders?page=${page}&pageSize=${pageSize}`),
  
  getById: (id) => 
    axios.get(`${API_BASE}/orders/${id}`),
  
  getByStatus: (status) => 
    axios.get(`${API_BASE}/orders/status/${status}`),
  
  getCustomerOrders: (email) => 
    axios.get(`${API_BASE}/orders/customer?email=${encodeURIComponent(email)}`),
  
  getDashboard: () => 
    axios.get(`${API_BASE}/orders/dashboard/summary`),
  
  cancel: (id) => 
    axios.post(`${API_BASE}/orders/${id}/cancel`)
}

// Inventory API
export const inventoryApi = {
  getAll: () =>
    axios.get(`${API_BASE}/inventory`),
  
  getById: (productId) =>
    axios.get(`${API_BASE}/inventory/${productId}`),
  
  getReservations: (orderId = null) => {
    const url = orderId
      ? `${API_BASE}/inventory/reservations?orderId=${orderId}`
      : `${API_BASE}/inventory/reservations`
    return axios.get(url)
  },

  updateStock: (productId, stockQuantity) =>
    axios.put(`${API_BASE}/inventory/${productId}/stock`, { stockQuantity })
}

// Payment API
export const paymentApi = {
  getTransactions: (orderId = null, status = null) => {
    let url = `${API_BASE}/payment/transactions`
    const params = []
    if (orderId) params.push(`orderId=${orderId}`)
    if (status) params.push(`status=${status}`)
    if (params.length) url += `?${params.join('&')}`
    return axios.get(url)
  },
  
  getTransaction: (id) => 
    axios.get(`${API_BASE}/payment/transactions/${id}`),
  
  getTestCards: () => 
    axios.get(`${API_BASE}/payment/test-cards`)
}

// Shipping API
export const shippingApi = {
  getShipments: (orderId = null, status = null) => {
    let url = `${API_BASE}/shipping/shipments`
    const params = []
    if (orderId) params.push(`orderId=${orderId}`)
    if (status) params.push(`status=${status}`)
    if (params.length) url += `?${params.join('&')}`
    return axios.get(url)
  },
  
  getShipment: (id) =>
    axios.get(`${API_BASE}/shipping/shipments/${id}`),
  
  getCarriers: () =>
    axios.get(`${API_BASE}/shipping/carriers`),
  
  track: (trackingNumber) =>
    axios.get(`${API_BASE}/shipping/track/${trackingNumber}`),

  dispatch: (shipmentId) =>
    axios.post(`${API_BASE}/shipping/shipments/${shipmentId}/dispatch`),

  deliver: (shipmentId) =>
    axios.post(`${API_BASE}/shipping/shipments/${shipmentId}/deliver`)
}
