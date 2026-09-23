import { EmptyIcon, iconProps, TopProductsIcon } from '../../components/icons'
import { labels, message } from '../production-orders/messages'
import { formatNumber, formatOrders } from './dashboardFormat'
import type { DashboardSnapshot } from './types'

/** Item 22 (003_BD D-04). The rank comes from the <ol>; the bar behind each figure is decorative. */
export function TopProducts({ products }: { products: DashboardSnapshot['topProducts'] }) {
  const max = products[0]?.openQuantity ?? 0
  return (
    <section aria-labelledby="top-products-title" className="grid content-start gap-2.5 rounded-lg border border-gray-200 bg-white p-3.5">
      <h2 id="top-products-title" className="flex items-center gap-1.5 text-sm font-semibold text-gray-900">
        <TopProductsIcon {...iconProps} />
        {labels.dashboard.topProducts}
      </h2>
      {products.length === 0 ? (
        <p className="flex items-center gap-1.5 text-sm text-gray-600">
          <EmptyIcon {...iconProps} />
          {message('MSG-I007')}
        </p>
      ) : (
        <ol className="grid list-inside list-decimal gap-1.5 text-sm marker:text-gray-500">
          {products.map((p) => (
            <li key={p.product.id} className="grid grid-cols-[1fr_auto] items-center gap-2">
              <span className="truncate text-gray-900">
                <span className="text-gray-500 tabular-nums">{p.product.sku}</span> — {p.product.name}
              </span>
              <span className="relative min-w-32 px-1 text-right tabular-nums">
                <span aria-hidden="true" className="absolute inset-y-0 right-0 rounded bg-gray-100"
                  style={{ width: `${max === 0 ? 0 : Math.round((100 * p.openQuantity) / max)}%` }} />
                <span className="relative font-medium">{formatNumber(p.openQuantity)}</span>{' '}
                {/* gray-600, not 500: over the bar's gray-100, 500 is 4.39:1 and fails WCAG 1.4.3 (caught by axe in E2E). */}
                <span className="relative text-gray-600">({formatOrders(p.activeOrderCount)})</span>
              </span>
            </li>
          ))}
        </ol>
      )}
    </section>
  )
}
