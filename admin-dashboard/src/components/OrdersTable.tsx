import { Link } from 'react-router-dom'
import { OrderDto } from '../types'

function formatLocation(order: OrderDto): string {
  return [order.city, order.country].filter(Boolean).join(', ') || 'Location unavailable'
}

function formatShipment(order: OrderDto): string {
  return [order.shipmentCarrier, order.shipmentReference, order.trackingNumber].filter(Boolean).join(' / ') || 'n/a'
}

function formatUpdatedAt(value: string): string {
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? 'n/a' : parsed.toLocaleString()
}

export function OrdersTable({ orders }: { orders: OrderDto[] }) {
  return (
    <div className="table-wrapper">
      <table className="data-table">
        <thead>
          <tr>
            <th>Order</th>
            <th>Customer</th>
            <th>Status</th>
            <th>Payment</th>
            <th>Inventory</th>
            <th>Shipment</th>
            <th>Failure</th>
            <th>Items</th>
            <th>Total</th>
            <th>Updated</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => (
            <tr key={order.orderID}>
              <td>#{order.orderID}</td>
              <td>
                <div>{order.name ?? 'Unknown'}</div>
                <div className="cell-meta">{formatLocation(order)}</div>
              </td>
              <td>{order.status}</td>
              <td>{order.paymentStatus ?? order.stripePaymentStatus ?? 'n/a'}</td>
              <td>{order.inventoryResult ?? 'n/a'}</td>
              <td>{formatShipment(order)}</td>
              <td title={order.failureReason}>{order.failureReason ?? 'n/a'}</td>
              <td>{order.itemCount}</td>
              <td>{order.totalAmount.toFixed(2)}</td>
              <td>{formatUpdatedAt(order.updatedAtUtc)}</td>
              <td>
                <Link to={`/orders/${order.orderID}`}>Details</Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
