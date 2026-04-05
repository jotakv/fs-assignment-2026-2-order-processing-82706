import { useEffect, useState } from 'react'
import { getApiConnectionHint, getOrders } from '../api'
import { OrdersTable } from '../components/OrdersTable'
import { ORDER_STATUSES } from '../orderStatus'
import { OrderDto } from '../types'

export function OrdersDashboardPage() {
  const [orders, setOrders] = useState<OrderDto[]>([])
  const [status, setStatus] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    let isCurrent = true

    async function loadOrders() {
      setLoading(true)
      setError(null)

      try {
        const nextOrders = await getOrders(status || undefined)

        if (!isCurrent) {
          return
        }

        setOrders(nextOrders)
      } catch (err) {
        if (!isCurrent) {
          return
        }

        setOrders([])
        setError(err instanceof Error ? err.message : 'Unable to load orders.')
      } finally {
        if (isCurrent) {
          setLoading(false)
        }
      }
    }

    void loadOrders()

    return () => {
      isCurrent = false
    }
  }, [reloadToken, status])

  return (
    <section>
      <h2>Orders Dashboard</h2>
      <p>Operational monitoring for distributed order processing.</p>

      <div className="toolbar">
        <label className="field-group">
          <span>Filter by status</span>
          <select value={status} onChange={(e) => setStatus(e.target.value)}>
            <option value="">All statuses</option>
            {ORDER_STATUSES.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </label>
        <button className="action-button secondary" type="button" onClick={() => setReloadToken((value) => value + 1)}>
          Refresh
        </button>
      </div>

      {loading && <div className="state-card">Loading orders from the operational API...</div>}

      {!loading && error && (
        <div className="state-card error">
          <p>Orders could not be loaded.</p>
          <p className="muted">{error}</p>
          <p className="muted">Connection: {getApiConnectionHint()}</p>
          <button className="action-button" type="button" onClick={() => setReloadToken((value) => value + 1)}>
            Retry
          </button>
        </div>
      )}

      {!loading && !error && orders.length === 0 && (
        <div className="state-card empty">
          No orders match the current status filter.
        </div>
      )}

      {!loading && !error && orders.length > 0 && (
        <>
          <p className="muted">Showing {orders.length} order(s).</p>
          <OrdersTable orders={orders} />
        </>
      )}
    </section>
  )
}
