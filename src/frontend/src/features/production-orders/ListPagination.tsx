import { pageSizes, type PageSize } from './types'
import { labels } from './messages'

// 002_DD module 4 / 002_DD-SPD §5. Bounds come from the response's total and page size, not from what the client
// asked for, so the controls stay correct even after a superseded or retried request.

interface Props {
  total: number
  page: number
  /** From the response, so it is a plain number here — the allow-listed PageSize only describes what we may ask for. */
  pageSize: number
  onPage: (page: number) => void
  onPageSize: (pageSize: PageSize) => void
}

export function ListSummary({ total, page, pageSize }: Pick<Props, 'total' | 'page' | 'pageSize'>) {
  const first = (page - 1) * pageSize + 1
  const last = Math.min(page * pageSize, total)

  return (
    <p role="status" className="text-sm text-gray-700 tabular-nums">
      {total === 0 ? labels.list.summaryNone : labels.list.summary(first, last, total)}
    </p>
  )
}

export function PageSizeSelect({ pageSize, onPageSize }: Pick<Props, 'pageSize' | 'onPageSize'>) {
  return (
    <span className="flex items-center gap-2 text-sm text-gray-500">
      <label htmlFor="pageSize">{labels.list.rows}</label>
      <select
        id="pageSize"
        value={pageSize}
        onChange={(event) => onPageSize(Number(event.target.value) as PageSize)}
        className="rounded border border-gray-300 bg-white px-2 py-1 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
      >
        {pageSizes.map((size) => (
          <option key={size} value={size}>
            {size}
          </option>
        ))}
      </select>
    </span>
  )
}

export function ListPagination({ total, page, pageSize, onPage }: Omit<Props, 'onPageSize'>) {
  const lastPage = Math.max(1, Math.ceil(total / pageSize))

  return (
    <nav aria-label={labels.list.pagination} className="flex items-center justify-center gap-3.5 text-sm text-gray-700">
      <button
        type="button"
        onClick={() => onPage(page - 1)}
        disabled={page <= 1}
        className="rounded border border-gray-300 bg-white px-3 py-1.5 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none disabled:border-gray-200 disabled:text-gray-400"
      >
        <span aria-hidden="true">{labels.list.previous}</span><span className="sr-only">{labels.list.previousPage}</span>
      </button>
      <span className="tabular-nums">
        {labels.list.page(page, lastPage)}
      </span>
      <button
        type="button"
        onClick={() => onPage(page + 1)}
        disabled={page >= lastPage}
        className="rounded border border-gray-300 bg-white px-3 py-1.5 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none disabled:border-gray-200 disabled:text-gray-400"
      >
        <span aria-hidden="true">{labels.list.next}</span><span className="sr-only">{labels.list.nextPage}</span>
      </button>
    </nav>
  )
}
