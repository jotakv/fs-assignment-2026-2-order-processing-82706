import { AdminOrderDetailsDto, OrderDto } from './types'

type ApiErrorResponse = {
  error?: string
}

const API_BASE = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim() ?? ''

function normalizePath(path: string): string {
  if (path.startsWith('/api/')) {
    return path
  }

  return path.startsWith('/api')
    ? path
    : `/api${path.startsWith('/') ? path : `/${path}`}`
}

function ensureTrailingSlash(value: string): string {
  return value.endsWith('/') ? value : `${value}/`
}

function buildUrl(path: string): string {
  const normalizedPath = normalizePath(path)

  if (!API_BASE) {
    return normalizedPath
  }

  return new URL(normalizedPath, ensureTrailingSlash(API_BASE)).toString()
}

async function readErrorMessage(response: Response): Promise<string> {
  const contentType = response.headers.get('content-type') ?? ''

  if (contentType.includes('json')) {
    const body = (await response.json()) as ApiErrorResponse

    if (body.error?.trim()) {
      return body.error.trim()
    }

    return `The order API returned ${response.status} ${response.statusText}.`
  }

  const text = (await response.text()).trim()
  return text
    ? `The order API returned ${response.status}: ${text}`
    : `The order API returned ${response.status} ${response.statusText}.`
}

async function requestJson<T>(path: string): Promise<T> {
  const url = buildUrl(path)

  let response: Response

  try {
    response = await fetch(url, {
      headers: {
        Accept: 'application/json',
      },
    })
  } catch (error) {
    console.error('Admin dashboard network request failed.', {
      url,
      apiBase: API_BASE || 'vite-proxy',
      error,
    })

    throw new Error(
      API_BASE
        ? `Unable to reach the order API at ${API_BASE}. Confirm the backend is running and the base URL is correct.`
        : 'Unable to reach the order API through the configured /api proxy. Confirm the backend is running or set VITE_API_BASE_URL.',
    )
  }

  if (!response.ok) {
    const errorMessage = await readErrorMessage(response)

    console.error('Admin dashboard API request returned an error response.', {
      url,
      status: response.status,
      errorMessage,
    })

    throw new Error(errorMessage)
  }

  return response.json() as Promise<T>
}

export function getApiConnectionHint(): string {
  return API_BASE || 'Relative /api proxy'
}

export function getOrders(status?: string): Promise<OrderDto[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  return requestJson<OrderDto[]>(`/orders${query}`)
}

export function getOrderDetails(orderId: number): Promise<AdminOrderDetailsDto> {
  return requestJson<AdminOrderDetailsDto>(`/orders/${orderId}/admin`)
}
