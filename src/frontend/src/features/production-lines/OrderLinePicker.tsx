import { useEffect, useRef, useState } from 'react'
import { ApiError } from '../../lib/apiClient'
import { labels } from '../production-orders/messages'
import type { OrderLine } from '../production-orders/types'
import { eligibleLines, type EligiblePage } from './api'
import { lineButtonClass as button, lineInputClass as input } from './LineDialog'
const w = labels.lines

/** Separates retained assignment from paged eligible hints; a missing page row never clears history. */
export function OrderLinePicker({ productId, selected, locked, pending, error, onChange, onForbidden }: {
  productId: string; selected: OrderLine | null; locked: boolean; pending: boolean; error?: string;
  onChange: (line: OrderLine | null) => void; onForbidden: () => void
}) {
  const [draft, setDraft] = useState('')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [refresh, setRefresh] = useState(0)
  const [result, setResult] = useState<{ key: string; value: EligiblePage } | null>(null)
  const [failure, setFailure] = useState('')
  const currentProduct = useRef(productId)
  const key = `${productId}\u0000${search}\u0000${page}\u0000${refresh}`
  const rows = result?.key === key ? result.value : null
  useEffect(() => {
    if (currentProduct.current !== productId) { currentProduct.current = productId; setPage(1); setSearch(''); setDraft('') }
  }, [productId])
  useEffect(() => {
    if (locked || !productId || [...search].length > 100) return
    const controller = new AbortController()
    eligibleLines(productId, search, page, controller.signal).then(value => {
      if (!controller.signal.aborted) { setResult({ key, value }); setFailure('') }
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      setResult(null)
      if (reason instanceof ApiError && reason.status === 403) onForbidden()
      else if (!(reason instanceof ApiError && reason.status === 401)) setFailure(w.failed)
    })
    return () => controller.abort()
  }, [locked, productId, search, page, key, onForbidden])
  return <section className="grid gap-3 rounded border border-gray-300 p-4" aria-labelledby="lineId-label" aria-describedby={error ? 'lineId-error' : undefined}>
    <h2 id="lineId-label" className="font-medium">{w.assigned}</h2>
    <p id="lineId" tabIndex={-1} aria-invalid={Boolean(error)}>{selected ? `${selected.code} — ${selected.name}${selected.isActive ? '' : ` (${w.retired})`}` : w.unassigned}</p>
    {selected && <p className="text-sm text-gray-600">{w.retained}</p>}
    {error && <p id="lineId-error" role="alert">{error}</p>}
    {!locked && <>
      <button type="button" className={button} disabled={pending || !selected} onClick={() => onChange(null)}>{w.clear}</button>
      {productId && <>
        <div className="flex flex-wrap items-end gap-2"><div className="min-w-0 flex-1"><label htmlFor="order-line-search">{w.search}</label><input id="order-line-search" className={input} value={draft} disabled={pending} onChange={e => setDraft(e.target.value)} /></div><button type="button" className={button} disabled={pending} onClick={() => { if ([...draft.trim()].length <= 100) { setSearch(draft.trim()); setPage(1) } else setFailure(w.invalid) }}>{w.apply}</button><button type="button" className={button} disabled={pending} onClick={() => { setSearch(''); setDraft(''); setPage(1) }}>{labels.list.clearFilters}</button></div>
        {failure && <p role="alert">{failure} <button type="button" className={button} onClick={() => setRefresh(n => n + 1)}>{labels.common.retry}</button></p>}
        <h3 className="font-medium">{w.eligible}</h3>
        {!rows && !failure && <p>{w.loading}</p>}{rows?.items.length === 0 && <p>{w.noEligible}</p>}
        {rows?.items.map(line => <div key={line.id} className="flex flex-wrap items-center justify-between gap-2 break-words"><span>{line.code} — {line.name} ({line.minutesPerUnit} {w.minutes}: {line.unit})</span><button type="button" className={button} disabled={pending} aria-pressed={line.id === selected?.id} onClick={() => onChange({ id: line.id, code: line.code, name: line.name, isActive: line.isActive })}>{line.id === selected?.id ? w.selected : w.select}</button></div>)}
        {rows && <nav aria-label={w.pagination} className="flex flex-wrap justify-between gap-3"><button type="button" className={button} disabled={pending || page <= 1} onClick={() => setPage(n => n - 1)}>{w.previous}</button><span>{w.page(page)}</span><button type="button" className={button} disabled={pending || page >= 10000 || page * 50 >= rows.total} onClick={() => setPage(n => n + 1)}>{w.next}</button></nav>}
      </>}
    </>}
  </section>
}
