import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getApiConnectionHint, getOrderDetails } from '../api'
import { AdminOrderDetailsDto } from '../types'

export function OrderDetailsPage() {
  const { orderId } = useParams()
  const [order, setOrder] = useState<AdminOrderDetailsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    const parsedOrderId = Number(orderId)

    if (!orderId || Number.isNaN(parsedOrderId) || parsedOrderId <= 0) {
      setOrder(null)
      setError('Missing or invalid order id.')
      setLoading(false)
      return
    }

    let isCurrent = true

    async function loadOrderDetails() {
      setLoading(true)
      setError(null)

      try {
        const nextOrder = await getOrderDetails(parsedOrderId)

        if (!isCurrent) {
          return
        }

        setOrder(nextOrder)
      } catch (err) {
        if (!isCurrent) {
          return
        }

        setOrder(null)
        setError(err instanceof Error ? err.message : 'Unable to load order details.')
      } finally {
        if (isCurrent) {
          setLoading(false)
        }
      }
    }

    void loadOrderDetails()

    return () => {
      isCurrent = false
    }
  }, [orderId, reloadToken])

  if (loading) {
    return <div className="state-card">Loading order details...</div>
  }

  if (error) {
    return (
      <section>
        <p>
          <Link className="page-link" to="/">
            Back to orders
          </Link>
        </p>
        <div className="state-card error">
          <p>Order details could not be loaded.</p>
          <p className="muted">{error}</p>
          <p className="muted">Connection: {getApiConnectionHint()}</p>
          <button className="action-button" type="button" onClick={() => setReloadToken((value) => value + 1)}>
            Retry
          </button>
        </div>
      </section>
    )
  }

  if (!order) {
    return (
      <section>
        <p>
          <Link className="page-link" to="/">
            Back to orders
          </Link>
        </p>
        <div className="state-card empty">Order not found.</div>
      </section>
    )
  }

  return (
    <section>
      <p>
        <Link className="page-link" to="/">
          Back to orders
        </Link>
      </p>
      <h2>Order #{order.orderID}</h2>
      <p>Operational detail view for payment, inventory, shipping, and failures.</p>
      <div className="detail-grid">
        <div className="detail-card">
          <strong>Status:</strong> {order.status}
        </div>
        <div className="detail-card">
          <strong>Customer:</strong> {order.customerName ?? 'Unknown'}
        </div>
        <div className="detail-card">
          <strong>Payment status:</strong> {order.paymentStatus ?? 'n/a'}
        </div>
        <div className="detail-card">
          <strong>Payment ref:</strong> {order.paymentReference ?? 'n/a'}
        </div>
        <div className="detail-card">
          <strong>Inventory result:</strong> {order.inventoryResult ?? 'n/a'}
        </div>
        <div className="detail-card">
          <strong>Inventory ref:</strong> {order.inventoryReference ?? 'n/a'}
        </div>
        <div className="detail-card">
          <strong>Shipment carrier:</strong> {order.shipmentCarrier ?? 'n/a'}
        </div>
        <div className="detail-card">
          <strong>Shipment ref:</strong> {order.shipmentReference ?? 'n/a'}
        </div>
        <div className="detail-card">
          <strong>Tracking:</strong> {order.trackingNumber ?? 'n/a'}
        </div>
        <div className="detail-card">
          <strong>Updated:</strong> {new Date(order.updatedAtUtc).toLocaleString()}
        </div>
        <div className="detail-card detail-card-wide">
          <strong>Failure reason:</strong> {order.failureReason ?? 'n/a'}
        </div>
      </div>

      <h3>Line items</h3>
      <ul className="line-items">
        {order.lines.map((line) => (
          <li key={`${line.productID}-${line.name}`}>
            {line.name} x {line.quantity} - {line.lineTotal.toFixed(2)}
          </li>
        ))}
      </ul>
    </section>
  )
}
