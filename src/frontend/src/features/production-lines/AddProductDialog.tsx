import { useEffect, useRef, useState, type FormEvent } from 'react'
import { ApiError } from '../../lib/apiClient'
import { labels } from '../production-orders/messages'
import { productChoices, type Page, type ProductChoice } from './api'
import { formPageSize } from './lineViewRules'
import { choiceListHeight, errorBox, inputClass, labelClass, pagerButton, primaryButton, scrollBox, secondaryButton } from './lineStyles'

const w = labels.lines

/**
 * 005_DD-SPD-REDESIGN §5.3: searches active products 20 at a time (DEC-008) and adds one. Choosing a product here,
 * with its unit shown, is the explicit confirmation of that unit for the new pair (DEC-003).
 */
export function AddProductDialog({ open, lineId, excluded, onAdd, onClose, onForbidden }: {
  open: boolean; lineId?: string; excluded: ReadonlySet<string>
  onAdd: (product: ProductChoice) => void; onClose: () => void; onForbidden: () => void
}) {
  const dialog = useRef<HTMLDialogElement>(null)
  const search = useRef<HTMLInputElement>(null)
  const invoker = useRef<HTMLElement | null>(null)
  const added = useRef(false)
  const [draft, setDraft] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)
  const [refresh, setRefresh] = useState(0)
  const [result, setResult] = useState<{ key: string; value: Page<ProductChoice> } | null>(null)
  const [error, setError] = useState('')
  const [chosen, setChosen] = useState<string | null>(null)
  const key = `${query}\u0000${page}\u0000${refresh}`
  const rows = result?.key === key ? result.value : null
  const items = rows?.items.filter(product => !excluded.has(product.id)) ?? []
  const pages = rows ? Math.max(1, Math.ceil(rows.total / rows.pageSize)) : 1

  useEffect(() => {
    if (open && !dialog.current?.open) {
      invoker.current = document.activeElement instanceof HTMLElement ? document.activeElement : null
      added.current = false
      setDraft(''); setQuery(''); setPage(1); setChosen(null); setError('')
      dialog.current?.showModal()
      search.current?.focus()
    } else if (!open && dialog.current?.open) {
      dialog.current.close()
      if (!added.current && invoker.current?.isConnected) invoker.current.focus()
    }
  }, [open])

  useEffect(() => {
    if (!open) return
    const controller = new AbortController()
    productChoices(query, page, lineId, controller.signal, formPageSize).then(value => {
      if (!controller.signal.aborted) { setResult({ key, value }); setError('') }
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      setResult(null)
      if (reason instanceof ApiError && reason.status === 403) onForbidden()
      else if (!(reason instanceof ApiError && reason.status === 401)) setError(w.failed)
    })
    return () => controller.abort()
  }, [open, query, page, lineId, key, onForbidden])

  function apply(event: FormEvent) {
    event.preventDefault()
    if ([...draft.trim()].length > 100) { setError(w.invalid); return }
    setQuery(draft.trim()); setPage(1); setChosen(null)
  }
  function add() {
    const product = items.find(item => item.id === chosen)
    if (!product) return
    added.current = true
    onAdd(product)
  }

  return <dialog ref={dialog} aria-labelledby="add-product-title" aria-describedby={lineId ? 'add-product-note add-product-excluded' : 'add-product-note'}
    onCancel={event => { event.preventDefault(); onClose() }}
    style={{ position: 'fixed', inset: 0, margin: 'auto', width: '640px', maxWidth: 'calc(100vw - 32px)', maxHeight: 'calc(100dvh - 32px)' }}
    className="overflow-auto rounded-lg border-0 bg-white p-5 text-[#172334] [overflow-wrap:anywhere] shadow-xl backdrop:bg-black/40">
    <h2 id="add-product-title" className="text-xl font-bold">{w.add}</h2>
    <form onSubmit={apply} className="mt-4 grid gap-1.5">
      <label htmlFor="add-product-search" className={labelClass}>{w.productSearch}</label>
      <div className="flex min-w-0 gap-2.5"><input ref={search} id="add-product-search" className={inputClass} value={draft} onChange={event => setDraft(event.target.value)} />
        <button type="submit" className={primaryButton}>{w.apply}</button></div>
    </form>
    {error && <div role="alert" className={`${errorBox} mt-3 flex flex-wrap items-center gap-3`}><span>{error}</span><button type="button" className={secondaryButton} onClick={() => setRefresh(n => n + 1)}>{labels.common.retry}</button></div>}
    {!rows && !error && <p role="status" className="mt-3">{w.loading}</p>}
    {rows && <>
      <p className="mt-3">{w.count(rows.total)}</p>
      {items.length === 0 ? <p className="mt-2">{w.noChoices}</p> :
        <div tabIndex={0} role="region" aria-label={w.choices} className={`${scrollBox} ${choiceListHeight} mt-2 p-2`}>
          <fieldset className="grid gap-2"><legend className="sr-only">{w.choices}</legend>
            {items.map(product => <label key={product.id} className={`flex min-w-0 cursor-pointer items-start gap-2.5 rounded border px-3 py-2 ${chosen === product.id ? 'border-[#1f5fa8] bg-[#e8f0fb]' : 'border-[#d5dde6]'}`}>
              <input type="radio" name="add-product-choice" value={product.id} checked={chosen === product.id} onChange={() => setChosen(product.id)} className="mt-1.5 size-4 shrink-0 accent-[#1f5fa8]" />
              <span className="grid min-w-0"><span className="font-bold [overflow-wrap:anywhere]">{product.sku}　{product.name}</span><span className="text-sm text-[#536475]">{w.unitOf(product.unit)}</span></span>
            </label>)}
          </fieldset>
        </div>}
      {pages > 1 && <nav aria-label={w.choicePagination} className="mt-2.5 flex items-center justify-end gap-2.5">
        <button type="button" className={pagerButton} disabled={page <= 1} onClick={() => { setPage(n => n - 1); setChosen(null) }}>{w.previous}</button>
        <span className="tabular-nums">{w.pageOf(page, pages)}</span>
        <button type="button" className={pagerButton} disabled={page >= pages || page >= 10000} onClick={() => { setPage(n => n + 1); setChosen(null) }}>{w.next}</button></nav>}
    </>}
    <p id="add-product-note" className="mt-3">{w.addNote}</p>
    {lineId && <p id="add-product-excluded" className="mt-1 text-sm text-[#536475]">{w.addExcludedNote}</p>}
    <div className="mt-4 flex flex-col gap-2.5 sm:flex-row">
      <button type="button" className={primaryButton} disabled={!items.some(item => item.id === chosen)} onClick={add}>{w.addConfirm}</button>
      <button type="button" className={secondaryButton} onClick={onClose}>{w.cancel}</button>
    </div>
  </dialog>
}
