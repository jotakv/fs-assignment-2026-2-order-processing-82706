import { NavLink, Route, Routes } from 'react-router-dom'
import { FailedOrdersPage } from './pages/FailedOrdersPage'
import { OrderDetailsPage } from './pages/OrderDetailsPage'
import { OrdersDashboardPage } from './pages/OrdersDashboardPage'

export default function App() {
  return (
    <div className="shell">
      <aside className="sidebar">
        <h1>SportsStore Admin</h1>
        <nav>
          <NavLink to="/">Orders Dashboard</NavLink>
          <NavLink to="/failed">Failed Orders</NavLink>
        </nav>
      </aside>

      <main className="content">
        <Routes>
          <Route path="/" element={<OrdersDashboardPage />} />
          <Route path="/orders/:orderId" element={<OrderDetailsPage />} />
          <Route path="/failed" element={<FailedOrdersPage />} />
        </Routes>
      </main>
    </div>
  )
}
