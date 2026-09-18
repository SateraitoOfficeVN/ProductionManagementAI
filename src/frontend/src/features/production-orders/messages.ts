import type { ProductionOrderStatus } from './types'

// DD-001 message catalog. The API returns IDs only; all wording lives here.
const messages: Record<string, string> = {
  'MSG-E001': 'Select a product.',
  'MSG-E002': 'The selected product no longer exists.',
  'MSG-E003': 'Enter a whole number of 1 or more.',
  'MSG-E004': 'Enter a due date.',
  'MSG-E005': "Due date can't be in the past.",
  'MSG-E006': "Notes can't exceed 500 characters.",
  'MSG-E007': "This status change isn't allowed.",
  'MSG-E008': "Product and quantity can't be changed after the order leaves Draft.",
  'MSG-E009': 'This order was changed by someone else. Reload to see the latest version.',
  'MSG-E010': "Quantity can't exceed 999,999,999.",
  'MSG-E011': "This production order doesn't exist.",
  'MSG-E012': "You don't have permission to manage production orders.",
  'MSG-E013': 'Something went wrong. Try again.',
  'MSG-E014': 'No products available.',
  'MSG-I001': 'Production order {orderNumber} created.',
  'MSG-I002': 'Production order {orderNumber} saved.',
}

export function message(id: string, values: Record<string, string> = {}): string {
  const template = messages[id] ?? messages['MSG-E013']
  return template.replace(/\{(\w+)\}/g, (_, key: string) => values[key] ?? '')
}

// BD-001 M-01.
export const statusLabels: Record<ProductionOrderStatus, string> = {
  Draft: 'Draft',
  InProgress: 'In progress',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
}
