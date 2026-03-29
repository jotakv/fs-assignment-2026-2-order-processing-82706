import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getOrderDetails } from '../api'
import { AdminOrderDetailsDto } from '../types'

export function OrderDetailsPage() {
  const { orderId } = useParams()
  const [order, setOrder] = useState<AdminOrderDetailsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!orderId) {
      setError('Missing order id')
      setLoading(false)
      return
    }

    getOrderDetails(Number(orderId))
      .then(setOrder)
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))
  }, [orderId])

  if (loading) return <p>Loading order details...</p>
  if (error) return <p>{error}</p>
  if (!order) return <p>Order not found.</p>

  return (
    <section>
      <h2>Order #{order.orderID}</h2>
      <div className="detail-grid">
        <div><strong>Status:</strong> {order.status}</div>
        <div><strong>Customer:</strong> {order.customerName ?? 'Unknown'}</div>
        <div><strong>Payment status:</strong> {order.paymentStatus ?? 'n/a'}</div>
        <div><strong>Payment ref:</strong> {order.paymentReference ?? 'n/a'}</div>
        <div><strong>Inventory result:</strong> {order.inventoryResult ?? 'n/a'}</div>
        <div><strong>Inventory ref:</strong> {order.inventoryReference ?? 'n/a'}</div>
        <div><strong>Shipment ref:</strong> {order.shipmentReference ?? 'n/a'}</div>
        <div><strong>Tracking:</strong> {order.trackingNumber ?? 'n/a'}</div>
        <div><strong>Updated:</strong> {new Date(order.updatedAtUtc).toLocaleString()}</div>
        <div><strong>Failure reason:</strong> {order.failureReason ?? 'n/a'}</div>
      </div>

      <h3>Line items</h3>
      <ul>
        {order.lines.map((line) => (
          <li key={`${line.productID}-${line.name}`}>
            {line.name} x {line.quantity} — {line.lineTotal.toFixed(2)}
          </li>
        ))}
      </ul>
    </section>
  )
}
