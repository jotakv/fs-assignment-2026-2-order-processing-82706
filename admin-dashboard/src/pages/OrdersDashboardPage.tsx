import { useEffect, useState } from 'react'
import { getOrders } from '../api'
import { OrdersTable } from '../components/OrdersTable'
import { OrderDto } from '../types'

const STATUSES = ['', 'Submitted', 'InventoryConfirmed', 'PaymentApproved', 'Completed', 'Failed']

export function OrdersDashboardPage() {
  const [orders, setOrders] = useState<OrderDto[]>([])
  const [status, setStatus] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    setLoading(true)
    setError(null)
    getOrders(status || undefined)
      .then(setOrders)
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))
  }, [status])

  return (
    <section>
      <h2>Orders Dashboard</h2>
      <p>Operational monitoring for distributed order processing.</p>

      <label>
        Filter by status:{' '}
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          {STATUSES.map((option) => (
            <option key={option} value={option}>
              {option || 'All'}
            </option>
          ))}
        </select>
      </label>

      {loading && <p>Loading orders...</p>}
      {error && <p>{error}</p>}
      {!loading && !error && <OrdersTable orders={orders} />}
    </section>
  )
}
