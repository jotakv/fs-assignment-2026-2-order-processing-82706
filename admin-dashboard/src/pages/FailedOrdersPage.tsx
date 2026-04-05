import { useEffect, useState } from 'react'
import { getApiConnectionHint, getOrders } from '../api'
import { OrdersTable } from '../components/OrdersTable'
import { isFailureStatus } from '../orderStatus'
import { OrderDto } from '../types'

export function FailedOrdersPage() {
  const [orders, setOrders] = useState<OrderDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    let isCurrent = true

    async function loadOrders() {
      setLoading(true)
      setError(null)

      try {
        const nextOrders = await getOrders()

        if (!isCurrent) {
          return
        }

        setOrders(nextOrders.filter((order) => isFailureStatus(order.status)))
      } catch (err) {
        if (!isCurrent) {
          return
        }

        setOrders([])
        setError(err instanceof Error ? err.message : 'Unable to load failed orders.')
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
  }, [reloadToken])

  return (
    <section>
      <h2>Failed Orders</h2>
      <p>Quick view of distributed pipeline failures.</p>

      {loading && <div className="state-card">Loading failed orders...</div>}

      {!loading && error && (
        <div className="state-card error">
          <p>Failed orders could not be loaded.</p>
          <p className="muted">{error}</p>
          <p className="muted">Connection: {getApiConnectionHint()}</p>
          <button className="action-button" type="button" onClick={() => setReloadToken((value) => value + 1)}>
            Retry
          </button>
        </div>
      )}

      {!loading && !error && orders.length === 0 && (
        <div className="state-card empty">
          There are no orders in a failure state right now.
        </div>
      )}

      {!loading && !error && orders.length > 0 && (
        <>
          <p className="muted">Showing {orders.length} failed order(s).</p>
          <OrdersTable orders={orders} />
        </>
      )}
    </section>
  )
}
