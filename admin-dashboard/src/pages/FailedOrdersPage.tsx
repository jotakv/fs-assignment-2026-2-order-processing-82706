import { useEffect, useState } from 'react'
import { getOrders } from '../api'
import { OrdersTable } from '../components/OrdersTable'
import { OrderDto } from '../types'

export function FailedOrdersPage() {
  const [orders, setOrders] = useState<OrderDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getOrders('Failed')
      .then(setOrders)
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  return (
    <section>
      <h2>Failed Orders</h2>
      <p>Quick view of distributed pipeline failures.</p>
      {loading && <p>Loading failed orders...</p>}
      {error && <p>{error}</p>}
      {!loading && !error && <OrdersTable orders={orders} />}
    </section>
  )
}
