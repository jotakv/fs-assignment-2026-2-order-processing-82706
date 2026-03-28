export type OrderDto = {
  orderID: number
  name?: string
  city?: string
  country?: string
  itemCount: number
  totalAmount: number
  status: string
  stripePaymentStatus?: string
}

export type AdminOrderDetailsDto = {
  orderID: number
  status: string
  customerName?: string
  paymentStatus?: string
  paymentReference?: string
  inventoryResult?: string
  inventoryReference?: string
  shipmentReference?: string
  trackingNumber?: string
  failureReason?: string
  updatedAtUtc: string
  lines: Array<{
    productID: number
    name: string
    price: number
    quantity: number
    lineTotal: number
  }>
}
