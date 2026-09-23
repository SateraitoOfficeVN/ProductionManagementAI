import { Link, useNavigate } from 'react-router-dom'
import { formatDate, formatNumber, formatTimestamp } from '../../lib/format'
import { labels, statusLabels } from './messages'
import type { ProductionOrderListItem, ProductionOrderSort, SortDirection } from './types'

// 002_DD module 3 / 002_DD-SPD §4. A real <table> at sm and above, cards below it; the order-number cell is a real
// link, which is what makes every row keyboard-reachable (DEC-009).

interface Column {
  key: ProductionOrderSort
  label: string
  numeric?: boolean
}

const columns: Column[] = [
  { key: 'orderNumber', label: labels.list.columns.orderNumber },
  { key: 'product', label: labels.list.columns.product },
  { key: 'quantity', label: labels.list.columns.quantity, numeric: true },
  { key: 'dueDate', label: labels.list.columns.dueDate },
  { key: 'status', label: labels.list.columns.status },
  { key: 'updatedAt', label: labels.list.columns.updatedAt },
]

const statusBadge: Record<ProductionOrderListItem['status'], string> = {
  Draft: 'border-gray-300 bg-gray-100 text-gray-700',
  InProgress: 'border-blue-200 bg-blue-50 text-blue-800',
  Completed: 'border-green-200 bg-green-50 text-green-800',
  // gray-600, not gray-400: the lighter grey reads as 2.9:1 on white and fails WCAG 1.4.3 (caught by axe).
  Cancelled: 'border-gray-300 bg-gray-50 text-gray-600 line-through',
}

interface Props {
  items: ProductionOrderListItem[]
  sort: ProductionOrderSort
  dir: SortDirection
  onSort: (sort: ProductionOrderSort, dir: SortDirection) => void
}

export function ProductionOrderTable({ items, sort, dir, onSort }: Props) {
  const navigate = useNavigate()

  const handleSort = (key: ProductionOrderSort) =>
    onSort(key, key === sort && dir === 'asc' ? 'desc' : key === sort ? 'asc' : 'asc')

  // Mouse convenience only: the link above already covers keyboard and assistive tech, so removing this would
  // change nothing about what the screen can do (DEC-009).
  const handleRowClick = (event: React.MouseEvent, id: string) => {
    if (event.defaultPrevented || window.getSelection()?.toString()) {
      return
    }
    if ((event.target as HTMLElement).closest('a, button, input, select')) {
      return
    }
    navigate(`/production-orders/${encodeURIComponent(id)}`)
  }

  return (
    <>
      <div className="hidden overflow-x-auto rounded-lg border border-gray-200 sm:block">
        <table className="w-full border-collapse text-sm">
          <caption className="border-b border-gray-200 bg-gray-50 px-3.5 py-2 text-left text-xs text-gray-500">
            {labels.list.caption(columns.find((column) => column.key === sort)?.label ?? '', dir === 'asc')}
          </caption>
          <thead>
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  scope="col"
                  aria-sort={sort === column.key ? (dir === 'asc' ? 'ascending' : 'descending') : undefined}
                  className="border-b border-gray-200 bg-gray-50 p-0 text-left font-medium text-gray-700"
                >
                  <button
                    type="button"
                    onClick={() => handleSort(column.key)}
                    className={`flex w-full items-center gap-1.5 px-3.5 py-2.5 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-[-2px] focus-visible:outline-none ${
                      column.numeric ? 'justify-end' : ''
                    } ${sort === column.key ? 'font-semibold text-gray-900' : ''}`}
                  >
                    {column.label}
                    <span aria-hidden="true" className="text-[10px]">
                      {sort === column.key ? (dir === 'asc' ? '▲' : '▼') : '↕'}
                    </span>
                  </button>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr
                key={item.id}
                onClick={(event) => handleRowClick(event, item.id)}
                className="cursor-pointer border-b border-gray-100 last:border-b-0 hover:bg-gray-50"
              >
                <td className="px-3.5 py-2.5 tabular-nums">
                  <Link
                    to={`/production-orders/${encodeURIComponent(item.id)}`}
                    className="text-gray-900 underline underline-offset-4 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
                  >
                    {item.orderNumber}
                  </Link>
                </td>
                <td className="px-3.5 py-2.5">
                  <span className="text-gray-500 tabular-nums">{item.product.sku}</span> {item.product.name}
                </td>
                <td className="px-3.5 py-2.5 text-right tabular-nums">{formatNumber(item.quantity)}</td>
                <td className="px-3.5 py-2.5 whitespace-nowrap tabular-nums">
                  {formatDate(item.dueDate)}
                  {item.isOverdue && <OverdueMarker />}
                </td>
                <td className="px-3.5 py-2.5">
                  <StatusBadge status={item.status} />
                </td>
                <td className="px-3.5 py-2.5 whitespace-nowrap text-gray-500 tabular-nums">
                  {formatUpdated(item.updatedAt)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* SP: the whole card is the link (002_BD §1 SP). */}
      <ul className="grid gap-2.5 sm:hidden">
        {items.map((item) => (
          <li key={item.id}>
            <Link
              to={`/production-orders/${encodeURIComponent(item.id)}`}
              className="grid gap-1.5 rounded-lg border border-gray-200 bg-white p-3.5 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
            >
              <span className="flex items-baseline justify-between gap-2.5">
                <span className="text-gray-900 underline underline-offset-4 tabular-nums">{item.orderNumber}</span>
                <StatusBadge status={item.status} />
              </span>
              <span className="text-sm text-gray-700">
                <span className="text-gray-500 tabular-nums">{item.product.sku}</span> {item.product.name}
              </span>
              <span className="flex flex-wrap gap-x-3.5 gap-y-1 text-sm text-gray-500 tabular-nums">
                <span>{labels.list.cardQuantity(formatNumber(item.quantity))}</span>
                <span>
                  {labels.list.cardDue(formatDate(item.dueDate))}
                  {item.isOverdue && <OverdueMarker />}
                </span>
              </span>
              <span className="text-xs text-gray-500 tabular-nums">{labels.list.cardUpdated(formatUpdated(item.updatedAt))}</span>
            </Link>
          </li>
        ))}
      </ul>
    </>
  )
}

export function StatusBadge({ status }: { status: ProductionOrderListItem['status'] }) {
  return (
    <span className={`inline-block rounded-full border px-2 py-0.5 text-xs whitespace-nowrap ${statusBadge[status]}`}>
      {statusLabels[status]}
    </span>
  )
}

/** Text, never colour alone (002_BD M-07/M-08, WCAG 1.4.1). */
function OverdueMarker() {
  return (
    <span className="ml-1.5 rounded border border-red-200 bg-red-50 px-1.5 py-px text-xs text-red-800">{labels.list.overdue}</span>
  )
}

const formatUpdated = formatTimestamp
