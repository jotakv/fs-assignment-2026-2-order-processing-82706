import { Link } from 'react-router-dom'
import { OrderDto } from '../types'

export function OrdersTable({ orders }: { orders: OrderDto[] }) {
  return (
    <table className="data-table">
      <thead>
        <tr>
          <th>Order</th>
          <th>Customer</th>
          <th>Status</th>
          <th>Payment</th>
          <th>Items</th>
          <th>Total</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order) => (
          <tr key={order.orderID}>
            <td>#{order.orderID}</td>
            <td>{order.name ?? 'Unknown'}</td>
            <td>{order.status}</td>
            <td>{order.stripePaymentStatus ?? 'n/a'}</td>
            <td>{order.itemCount}</td>
            <td>{order.totalAmount.toFixed(2)}</td>
            <td>
              <Link to={`/orders/${order.orderID}`}>Details</Link>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
