import { useRef, useState } from 'react'
import { filterFieldOrder, validateFilters, type FilterErrors } from './listValidation'
import { hasFilters, maxOrderNumberFilterLength } from './listViewState'
import { message, statusLabels } from './messages'
import type { ListFilters, Product, ProductionOrderStatus } from './types'

// DD-002 module 2 / DD-002-SPD §3. The panel holds its own draft while the user edits and publishes it only on
// Search (DEC-008) — that is what keeps "the URL reflects what is displayed" true.

const statuses: ProductionOrderStatus[] = ['Draft', 'InProgress', 'Completed', 'Cancelled']

const fieldClass =
  'rounded border border-gray-300 bg-white px-3 py-2 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none'

interface Props {
  filters: ListFilters
  products: Product[]
  productsFailed: boolean
  busy: boolean
  onApply: (filters: ListFilters) => void
  onClear: () => void
}

export function ProductionOrderFilters({ filters, products, productsFailed, busy, onApply, onClear }: Props) {
  const [draft, setDraft] = useState<ListFilters>(filters)
  const [errors, setErrors] = useState<FilterErrors>({})
  const dueFromRef = useRef<HTMLInputElement>(null)
  const dueToRef = useRef<HTMLInputElement>(null)
  const orderNumberRef = useRef<HTMLInputElement>(null)

  // Re-base the draft whenever a different set of filters becomes the applied one (Back, a shared URL, Clear), so the
  // panel always shows what produced the rows. Adjusting state during render rather than in an effect, per React's
  // "derive state from props" guidance: no extra commit, no flash of stale values.
  const appliedKey = JSON.stringify(filters)
  const [renderedKey, setRenderedKey] = useState(appliedKey)
  if (renderedKey !== appliedKey) {
    setRenderedKey(appliedKey)
    setDraft(filters)
    setErrors({})
  }

  const toggleStatus = (status: ProductionOrderStatus) =>
    setDraft((current) => ({
      ...current,
      statuses: current.statuses.includes(status)
        ? current.statuses.filter((value) => value !== status)
        : [...current.statuses, status],
    }))

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault()
    const found = validateFilters(draft)
    setErrors(found)
    const firstInvalid = filterFieldOrder.find((field) => found[field])
    if (firstInvalid) {
      const refs = { dueFrom: dueFromRef, dueTo: dueToRef, orderNumber: orderNumberRef }
      refs[firstInvalid].current?.focus()
      return
    }
    onApply(draft)
  }

  const describedBy = (field: keyof FilterErrors) => (errors[field] ? `${field}-error` : undefined)
  const errorSlot = (field: keyof FilterErrors) => (
    <p id={`${field}-error`} className="min-h-[1.25rem] text-xs text-red-600">
      {errors[field] ? message(errors[field]!) : ''}
    </p>
  )

  return (
    <form
      onSubmit={handleSubmit}
      aria-label="Filter production orders"
      className="grid gap-4 rounded-lg border border-gray-200 bg-white p-4 sm:p-5"
    >
      <div className="grid gap-4 sm:grid-cols-[1.4fr_1fr]">
        <fieldset className="grid gap-1.5">
          <legend className="text-sm font-medium text-gray-700">Status</legend>
          <div className="flex flex-wrap gap-x-5 gap-y-2 pt-1">
            {statuses.map((status) => (
              <label key={status} className="flex items-center gap-2 text-sm text-gray-900">
                <input
                  type="checkbox"
                  id={`status-${status}`}
                  checked={draft.statuses.includes(status)}
                  onChange={() => toggleStatus(status)}
                  className="size-4 accent-gray-900"
                />
                {statusLabels[status]}
              </label>
            ))}
          </div>
        </fieldset>

        <div className="grid content-start gap-1.5">
          <label htmlFor="productId" className="text-sm font-medium text-gray-700">
            Product
          </label>
          <select
            id="productId"
            value={draft.productId ?? ''}
            disabled={productsFailed}
            onChange={(event) => setDraft({ ...draft, productId: event.target.value || null })}
            className={`${fieldClass} disabled:bg-gray-100 disabled:text-gray-500`}
          >
            <option value="">All products</option>
            {products.map((product) => (
              <option key={product.id} value={product.id}>
                {product.sku} — {product.name}
              </option>
            ))}
          </select>
          <p className="min-h-[1.25rem] text-xs text-gray-500">
            {productsFailed ? 'Product list unavailable. The other filters still work.' : ''}
          </p>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <div className="grid gap-1.5">
          <label htmlFor="dueFrom" className="text-sm font-medium text-gray-700">
            Due from
          </label>
          <input
            ref={dueFromRef}
            id="dueFrom"
            type="date"
            value={draft.dueFrom ?? ''}
            aria-invalid={errors.dueFrom ? true : undefined}
            aria-describedby={describedBy('dueFrom')}
            onChange={(event) => setDraft({ ...draft, dueFrom: event.target.value || null })}
            className={`${fieldClass} ${errors.dueFrom ? 'border-red-600 ring-1 ring-red-600' : ''}`}
          />
          {errorSlot('dueFrom')}
        </div>

        <div className="grid gap-1.5">
          <label htmlFor="dueTo" className="text-sm font-medium text-gray-700">
            Due to
          </label>
          <input
            ref={dueToRef}
            id="dueTo"
            type="date"
            value={draft.dueTo ?? ''}
            aria-invalid={errors.dueTo ? true : undefined}
            aria-describedby={describedBy('dueTo')}
            onChange={(event) => setDraft({ ...draft, dueTo: event.target.value || null })}
            className={`${fieldClass} ${errors.dueTo ? 'border-red-600 ring-1 ring-red-600' : ''}`}
          />
          {errorSlot('dueTo')}
        </div>

        <div className="grid gap-1.5">
          <label htmlFor="orderNumber" className="text-sm font-medium text-gray-700">
            Order number
          </label>
          <input
            ref={orderNumberRef}
            id="orderNumber"
            type="search"
            inputMode="search"
            maxLength={maxOrderNumberFilterLength}
            placeholder="PO-2026-…"
            value={draft.orderNumber ?? ''}
            aria-invalid={errors.orderNumber ? true : undefined}
            aria-describedby={describedBy('orderNumber')}
            onChange={(event) => setDraft({ ...draft, orderNumber: event.target.value || null })}
            className={`${fieldClass} ${errors.orderNumber ? 'border-red-600 ring-1 ring-red-600' : ''}`}
          />
          {errorSlot('orderNumber')}
        </div>
      </div>

      <div className="flex flex-col-reverse gap-2.5 sm:flex-row sm:justify-end">
        <button
          type="button"
          onClick={onClear}
          disabled={!hasFilters(draft) && !hasFilters(filters)}
          className="rounded border border-gray-300 bg-white px-4 py-2 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none disabled:opacity-50"
        >
          Clear
        </button>
        <button
          type="submit"
          disabled={busy}
          className="rounded bg-gray-900 px-4 py-2 text-white focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none disabled:opacity-50"
        >
          Search
        </button>
      </div>
    </form>
  )
}
