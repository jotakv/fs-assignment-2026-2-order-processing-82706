export const ORDER_STATUSES = [
  'Submitted',
  'InventoryPending',
  'InventoryConfirmed',
  'InventoryFailed',
  'PaymentPending',
  'PaymentApproved',
  'PaymentFailed',
  'ShippingPending',
  'ShippingCreated',
  'Completed',
  'Failed',
] as const

export type OrderStatusValue = (typeof ORDER_STATUSES)[number]

const FAILURE_STATUS_LOOKUP = new Set<OrderStatusValue>(['InventoryFailed', 'PaymentFailed', 'Failed'])

export function isFailureStatus(status: string): boolean {
  return FAILURE_STATUS_LOOKUP.has(status as OrderStatusValue)
}
