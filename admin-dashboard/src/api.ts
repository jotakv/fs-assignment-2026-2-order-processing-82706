import { AdminOrderDetailsDto, OrderDto } from './types'

const API_BASE = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'https://localhost:5001/'

async function readResponse<T>(input: Promise<Response>): Promise<T> {
  const response = await input
  if (!response.ok) {
    throw new Error(`API request failed: ${response.status}`)
  }
  return response.json() as Promise<T>
}

export function getOrders(status?: string): Promise<OrderDto[]> {
  const suffix = status ? `?status=${encodeURIComponent(status)}` : ''
  return readResponse(fetch(`${API_BASE}api/orders${suffix}`))
}

export function getOrderDetails(orderId: number): Promise<AdminOrderDetailsDto> {
  return readResponse(fetch(`${API_BASE}api/orders/${orderId}/admin`))
}
