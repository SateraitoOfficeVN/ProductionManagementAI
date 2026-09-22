import type { ComponentType } from 'react'
import { AttentionIcon, DueSoonIcon, EmptyIcon, iconProps, OverdueIcon } from '../../components/icons'
import { message } from '../production-orders/messages'
import { StatusBadge } from '../production-orders/ProductionOrderTable'
import { formatNumber, shownNote } from './dashboardFormat'
import type { OrderGroup } from './types'

function Group({ id, title, Icon, group, emptyMessageId }: {
  id: string
  title: string
  Icon: ComponentType<typeof iconProps>
  group: OrderGroup
  emptyMessageId: string
}) {
  const note = shownNote(group.orders.length, group.total)
  const heading = `${title} (${group.total})`
  return (
    <section aria-labelledby={id} className="grid gap-1.5">
      <h3 id={id} className="flex items-center gap-1.5 text-sm font-medium text-gray-700">
        <Icon {...iconProps} />
        {heading}
      </h3>
      {group.total === 0 ? (
        <p className="flex items-center gap-1.5 text-sm text-gray-600">
          <EmptyIcon {...iconProps} />
          {message(emptyMessageId)}
        </p>
      ) : (
        <>
          {/* Plain text, not links: the dashboard is read-only (DEC-006). */}
          <div className="hidden overflow-hidden rounded-md border border-gray-200 sm:block">
            <table className="w-full text-sm">
              <caption className="sr-only">{heading}</caption>
              <thead className="bg-gray-50 text-left text-gray-700">
                <tr>
                  <th scope="col" className="px-3 py-2 font-medium">Order no.</th>
                  <th scope="col" className="px-3 py-2 font-medium">Product</th>
                  <th scope="col" className="px-3 py-2 text-right font-medium">Qty</th>
                  <th scope="col" className="px-3 py-2 font-medium">Due date</th>
                  <th scope="col" className="px-3 py-2 font-medium">Status</th>
                </tr>
              </thead>
              <tbody>
                {group.orders.map((o) => (
                  <tr key={o.id} className="border-t border-gray-100">
                    <td className="px-3 py-1.5 whitespace-nowrap tabular-nums">{o.orderNumber}</td>
                    <td className="px-3 py-1.5">
                      <span className="text-gray-500 tabular-nums">{o.product.sku}</span> — {o.product.name}
                    </td>
                    <td className="px-3 py-1.5 text-right tabular-nums">{formatNumber(o.quantity)}</td>
                    <td className="px-3 py-1.5 whitespace-nowrap tabular-nums">{o.dueDate}</td>
                    <td className="px-3 py-1.5"><StatusBadge status={o.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <ul className="grid gap-2 sm:hidden">
            {group.orders.map((o) => (
              <li key={o.id} className="grid gap-1 rounded-md border border-gray-200 px-3 py-2 text-sm">
                <div className="flex items-baseline justify-between gap-2">
                  <span className="tabular-nums">{o.orderNumber}</span>
                  <StatusBadge status={o.status} />
                </div>
                <span className="text-gray-700">{o.product.sku} — {o.product.name}</span>
                <span className="text-xs text-gray-500 tabular-nums">Qty {formatNumber(o.quantity)} · Due {o.dueDate}</span>
              </li>
            ))}
          </ul>
          {note && <p className="text-xs text-gray-500">{note}</p>}
        </>
      )}
    </section>
  )
}

/** Items 16–19 (BD-003 D-01, D-02, M-15, M-19). */
export function AttentionList({ overdue, dueSoon }: { overdue: OrderGroup; dueSoon: OrderGroup }) {
  return (
    <section aria-labelledby="attention-title" className="grid content-start gap-3 rounded-lg border border-gray-200 bg-white p-3.5">
      <h2 id="attention-title" className="flex items-center gap-1.5 text-sm font-semibold text-gray-900">
        <AttentionIcon {...iconProps} />
        Needs attention
      </h2>
      <Group id="overdue-title" title="Overdue" Icon={OverdueIcon} group={overdue} emptyMessageId="MSG-I005" />
      <Group id="due-soon-title" title="Due in the next 7 days" Icon={DueSoonIcon} group={dueSoon} emptyMessageId="MSG-I006" />
    </section>
  )
}
