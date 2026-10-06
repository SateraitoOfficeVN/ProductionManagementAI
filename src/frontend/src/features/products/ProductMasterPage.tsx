import { useEffect, useRef, useState, type FormEvent, type ReactNode } from 'react'
import { Link, Navigate, useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { GuardedLink } from '../../components/GuardedLink'
import { ApiError } from '../../lib/apiClient'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { useAuth } from '../auth/useAuth'
import { PageSizeSelect } from '../production-orders/ListPagination'
import { labels } from '../production-orders/messages'
import { pageSizes, type PageSize } from '../production-orders/types'
import {
  createMaster, getMaster, listMaster, productUnits, retireMaster, updateMaster,
  type ProductDraft, type ProductMasterItem, type ProductPage,
} from './api'

const words = labels.products
// Colours follow the approved 004_DD mockup. Control heights match the production-order screens (WI-013 DEC-006):
// filled buttons 40 px (42 px beside an outlined one in the filter row), outlined buttons and fields 42 px,
// pager buttons as in ListPagination.
const focusRing = 'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900'
const inputClass = `w-full rounded border border-[#8ea0b5] bg-white px-3 py-2 text-base text-[#172334] aria-[invalid=true]:border-[#b73838] ${focusRing}`
const readOnlyClass = 'flex w-full items-center gap-3 rounded border border-[#8ea0b5] bg-[#eef2f6] px-3 text-[#536475] focus-within:outline-2 focus-within:outline-offset-2 focus-within:outline-gray-900'
const readOnlyInput = 'min-w-0 flex-1 bg-transparent py-2 text-base outline-none'
const buttonBase = `inline-flex items-center justify-center rounded px-4 py-2 text-center text-base font-bold disabled:opacity-50 ${focusRing}`
const primaryButton = `${buttonBase} bg-[#1f5fa8] text-white`
const secondaryButton = `${buttonBase} border border-[#1f5fa8] bg-white text-[#1f5fa8]`
const dangerButton = `${buttonBase} bg-[#a33232] text-white`
const pagerButton = `rounded border border-[#1f5fa8] bg-white px-3 py-1.5 text-sm font-bold text-[#1f5fa8] disabled:border-gray-200 disabled:text-gray-400 ${focusRing}`
const errorBox = 'rounded-lg border border-[#b73838] bg-[#fff0f0] p-3.5'
const successBox = 'rounded-lg border border-[#407a54] bg-[#eef8f0] p-3.5'
const labelClass = 'mb-1.5 block text-sm font-bold'
const hintClass = 'mt-1.5 text-sm text-[#607286]'
const fieldErrorClass = 'mt-1.5 text-sm font-bold text-[#a12424]'
const dialogClass = 'm-auto w-[28rem] max-w-[calc(100%-2rem)] max-h-[calc(100%-2rem)] overflow-y-auto rounded-xl p-7 break-words shadow-xl backdrop:bg-black/40'
// Headers, status and actions never wrap on PC; a long product name takes the remaining width (WI-013 DEC-012).
const headerCell = 'whitespace-nowrap border-b border-[#e2e8ef] bg-[#f6f8fb] px-2.5 py-3 font-bold text-[#485c74]'
const cell = 'border-b border-[#e2e8ef] px-2.5 py-3'

type Notice = { text: string; tone: 'success' | 'error' }
type SavedState = { id: string; created: boolean }

function NoticeBox({ notice }: { notice: Notice | null }) {
  return notice ? <p className={notice.tone === 'success' ? successBox : errorBox}>{notice.text}</p> : null
}

function StatusPill({ active }: { active: boolean }) {
  return <span className={`inline-block rounded-full px-2.5 py-0.5 font-bold ${active ? 'bg-[#e6f4ec] text-[#155833]' : 'bg-[#edf0f4] text-[#4d5a6a]'}`}>
    {active ? words.active : words.retired}</span>
}

function Forbidden() {
  return <><p role="alert" className={errorBox}>{words.forbidden}</p><p><Link className={secondaryButton} to="/">{words.backToDashboard}</Link></p></>
}

/** Accepts only an internal Product master list URL as the place to return to (004_DD-SPD shared state). */
function returnUrl(state: unknown): string {
  const value = state && typeof state === 'object' && 'returnTo' in state ? state.returnTo : null
  return typeof value === 'string' && (value === '/products' || value.startsWith('/products?')) ? value : '/products'
}

function savedProduct(state: unknown): SavedState | null {
  const value = state && typeof state === 'object' && 'productSaved' in state ? state.productSaved : null
  return value && typeof value === 'object' && 'id' in value && typeof value.id === 'string'
    ? { id: value.id, created: 'created' in value && value.created === true } : null
}

function useEditorRole() {
  const { user } = useAuth()
  return user?.roles.some((role) => role === 'Admin' || role === 'Operator') ?? false
}

/** Product catalog with URL backed search, page size, paging and a native retirement dialog. */
export function ProductMasterPage() {
  const allowed = useEditorRole()
  const location = useLocation()
  const [params, setParams] = useSearchParams()
  const q = (params.get('q') ?? '').trim().slice(0, 100)
  const state = ['all', 'active', 'retired'].includes(params.get('state') ?? '') ? params.get('state')! : 'all'
  const parsedPage = Number(params.get('page') ?? '1')
  const page = Number.isSafeInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1
  const parsedSize = Number(params.get('pageSize') ?? '20')
  const pageSize: PageSize = (pageSizes as readonly number[]).includes(parsedSize) ? parsedSize as PageSize : 20
  const canonical = { q, state, page: String(page), pageSize: String(pageSize) }
  const listUrl = `/products?${new URLSearchParams(canonical)}`
  const searchKey = `${q}\u0000${state}`
  const [searchDraft, setSearchDraft] = useState<{ key: string; search: string; filter: string } | null>(null)
  const search = searchDraft?.key === searchKey ? searchDraft.search : q
  const filter = searchDraft?.key === searchKey ? searchDraft.filter : state
  const [result, setResult] = useState<ProductPage | null>(null)
  const [error, setError] = useState('')
  const [forbidden, setForbidden] = useState(false)
  const [notice, setNotice] = useState<Notice | null>(null)
  const [saved, setSaved] = useState(() => savedProduct(location.state))
  const [retire, setRetire] = useState<ProductMasterItem | null>(null)
  const [busy, setBusy] = useState(false)
  const [refresh, setRefresh] = useState(0)
  const dialog = useRef<HTMLDialogElement>(null)
  const retireCancel = useRef<HTMLButtonElement>(null)
  const resultHeading = useRef<HTMLHeadingElement>(null)
  const returnFocus = useRef<HTMLButtonElement>(null)

  useEffect(() => { document.title = labels.app.title(words.heading) }, [])
  useEffect(() => {
    if (['q', 'state', 'page', 'pageSize'].some((key) => params.getAll(key).length > 1) ||
        q !== (params.get('q') ?? '') || state !== (params.get('state') ?? 'all') ||
        String(page) !== (params.get('page') ?? '1') || String(pageSize) !== (params.get('pageSize') ?? '20')) {
      setParams({ q, state, page: String(page), pageSize: String(pageSize) }, { replace: true })
    }
  }, [params, q, state, page, pageSize, setParams])
  useEffect(() => {
    if (!allowed || forbidden) return
    const controller = new AbortController()
    listMaster(q, state, page, pageSize, controller.signal).then((value) => {
      // A page past the end (e.g. after a retirement) moves to the last valid page once (SPD-01 step 4).
      if (value.items.length === 0 && value.total > 0 && page > 1) {
        setParams({ q, state, pageSize: String(pageSize), page: String(Math.ceil(value.total / value.pageSize)) }, { replace: true })
        return
      }
      setResult(value)
      setError('')
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      setResult(null)
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) setForbidden(true)
      else setError(words.failed)
    })
    return () => controller.abort()
  }, [allowed, forbidden, q, state, page, pageSize, refresh, setParams])
  useEffect(() => {
    if (retire) { dialog.current?.showModal(); retireCancel.current?.focus() }
    else dialog.current?.close()
  }, [retire])

  // A save returns here with the product it saved: announce it, and say when the filters hide it (SPD-03 step 4).
  const savedNotice: Notice | null = saved && result ? {
    tone: 'success',
    text: (saved.created ? words.created : words.saved) +
      (result.items.some((item) => item.id === saved.id) ? '' : words.hiddenAfterSave),
  } : null
  const filtered = q !== '' || state !== 'all'
  const pages = result ? Math.max(1, Math.ceil(result.total / result.pageSize)) : 1

  function go(next: Record<string, string>) {
    setSaved(null)
    setNotice(null)
    setParams(next)
  }

  function apply(event: FormEvent) {
    event.preventDefault()
    go({ q: search.trim().slice(0, 100), state: filter, page: '1', pageSize: String(pageSize) })
    requestAnimationFrame(() => resultHeading.current?.focus())
  }

  async function confirmRetire() {
    if (!retire || busy) return
    setBusy(true)
    setSaved(null)
    try {
      await retireMaster(retire.id, retire.version)
      setNotice({ text: words.retireDone, tone: 'success' })
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 403) setForbidden(true)
      setNotice({ text: reason instanceof ApiError && reason.problem?.code === 'PRODUCT_STALE' ? words.stale : words.failed, tone: 'error' })
    } finally {
      setBusy(false)
      setRetire(null)
      setRefresh((value) => value + 1)
      requestAnimationFrame(() => (returnFocus.current?.isConnected ? returnFocus.current : resultHeading.current)?.focus())
    }
  }

  function actions(item: ProductMasterItem, wrap: boolean): ReactNode {
    return <div className={`flex gap-2 ${wrap ? 'flex-wrap' : 'flex-nowrap'}`}>
      <Link aria-label={words.editNamed(item.sku)} to={`/products/${item.id}/edit`} state={{ returnTo: listUrl }} className={secondaryButton}>{words.editAction}</Link>
      {item.isActive && <button type="button" aria-label={words.retireNamed(item.sku)} className={dangerButton} onClick={(e) => { returnFocus.current = e.currentTarget; setRetire(item) }}>{words.retire}</button>}
    </div>
  }

  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-5xl gap-5 px-4 py-6 text-[#172334] sm:px-6">
    <div className="grid justify-items-start gap-3 sm:flex sm:items-center sm:justify-between">
      <div><h1 className="text-2xl font-bold">{words.heading}</h1>{allowed && !forbidden && <p className="hidden text-[#67788b] sm:block">{words.description}</p>}</div>
      {allowed && !forbidden && <Link className={primaryButton} to="/products/new" state={{ returnTo: listUrl }}>{words.add}</Link>}</div>
    {!allowed || forbidden ? <Forbidden /> : <>
      <form action="/products" method="get" onSubmit={apply} className="grid gap-3.5 rounded-[10px] border border-[#d8e2ec] bg-[#f5f8fb] p-[18px] sm:grid-cols-[1fr_11rem_auto] sm:items-end">
        <div><label htmlFor="product-search" className={labelClass}>{words.search}</label><input id="product-search" name="q" maxLength={100} value={search} onChange={(e) => setSearchDraft({ key: searchKey, search: e.target.value, filter })} className={inputClass} /></div>
        <div><label htmlFor="product-state" className={labelClass}>{words.state}</label><select id="product-state" name="state" value={filter} onChange={(e) => setSearchDraft({ key: searchKey, search, filter: e.target.value })} className={inputClass}>
          <option value="all">{words.all}</option><option value="active">{words.active}</option><option value="retired">{words.retired}</option></select></div>
        <div className="flex flex-col gap-2.5 sm:flex-row"><button type="submit" className={primaryButton}>{words.apply}</button>
        <button type="button" className={secondaryButton} onClick={() => { setSearchDraft(null); go({ pageSize: String(pageSize) }); resultHeading.current?.focus() }}>{labels.list.clear}</button></div>
      </form>
      <div role="status" aria-live="polite" className="empty:hidden"><NoticeBox notice={notice ?? savedNotice} /></div>
      {error && <div role="alert" className={`${errorBox} flex flex-wrap items-center gap-3`}><span>{error}</span><button type="button" className={secondaryButton} onClick={() => setRefresh((n) => n + 1)}>{labels.common.retry}</button></div>}
      <h2 ref={resultHeading} tabIndex={-1} className="sr-only">{result ? words.result(result.total) : words.table}</h2>
      {result && (result.items.length === 0 ? <p>{filtered ? words.noResults : words.empty}</p> : <>
        {/* Count top-left and rows per page top-right, as on the order list (WI-013 DEC-010). */}
        <div className="flex flex-wrap items-center justify-between gap-3 text-[#536475]"><span>{words.count(result.total)}</span>
          <PageSizeSelect pageSize={result.pageSize} onPageSize={(size) => go({ q, state, page: '1', pageSize: String(size) })} /></div>
        <div className="hidden overflow-x-auto sm:block"><table className="w-full border-collapse text-left text-sm"><caption className="sr-only">{words.table}</caption><thead><tr>
          <th scope="col" className={headerCell}>{words.sku}</th><th scope="col" className={headerCell}>{words.name}</th><th scope="col" className={headerCell}>{words.unit}</th><th scope="col" className={headerCell}>{words.drawing}</th><th scope="col" className={headerCell}>{words.state}</th><th scope="col" className={headerCell}>{words.actions}</th></tr></thead><tbody>
          {result.items.map((item) => <tr key={item.id}><th scope="row" className={`${cell} font-normal`}>{item.sku}</th><td className={cell}>{item.name}</td><td className={cell}>{item.unit}</td><td className={cell}>{item.drawingNumber ?? '—'}</td><td className={`${cell} whitespace-nowrap`}><StatusPill active={item.isActive} /></td><td className={`${cell} whitespace-nowrap`}>{actions(item, false)}</td></tr>)}
        </tbody></table></div>
        <ul className="grid gap-3 sm:hidden">{result.items.map((item) => <li key={item.id} className="grid min-w-0 gap-3 rounded-[10px] border border-[#d7e0e8] p-4">
          {/* A long SKU or name wraps anywhere so it never widens the phone layout (WI-013 DEC-013). */}
          <strong className="[overflow-wrap:anywhere]">{item.sku}　{item.name}</strong>
          <p className="grid justify-items-start gap-1"><span>{words.unit}：{item.unit}</span><span>{words.drawing}：{item.drawingNumber ?? '—'}</span><StatusPill active={item.isActive} /></p>
          {actions(item, true)}</li>)}</ul>
        <div className="flex justify-end text-[#536475]">
          <nav aria-label={labels.list.pagination} className="flex items-center gap-3"><button type="button" className={pagerButton} disabled={page <= 1} onClick={() => go({ ...canonical, page: String(page - 1) })}>{words.previous}</button><span className="tabular-nums">{words.pageOf(page, pages)}</span><button type="button" className={pagerButton} disabled={page >= pages} onClick={() => go({ ...canonical, page: String(page + 1) })}>{words.next}</button></nav></div>
      </>)}
    </>}
    <dialog ref={dialog} aria-labelledby="retire-title" onClose={() => { setRetire(null); if (returnFocus.current?.isConnected) returnFocus.current.focus() }} aria-describedby="retire-description" className={dialogClass}><h2 id="retire-title" className="text-xl font-bold">{words.retireQuestion}</h2><p id="retire-description" className="my-4">{retire ? words.retireBody(retire.name, retire.sku) : ''}</p><div className="flex flex-wrap gap-2.5"><button ref={retireCancel} type="button" className={secondaryButton} onClick={() => setRetire(null)}>{words.cancel}</button><button type="button" className={dangerButton} disabled={busy} onClick={() => void confirmRetire()}>{words.retire}</button></div></dialog>
  </main></div>
}

/** Keeps pre-WI-013 `/products/:id` links working by moving them to the designed edit route (DEC-004). */
export function LegacyProductRedirect() {
  const { id } = useParams()
  return <Navigate to={`/products/${encodeURIComponent(id ?? '')}/edit`} replace />
}

/** Create and versioned edit form; server field errors remain tied to their inputs. */
export function ProductFormPage() {
  const { id } = useParams()
  return <ProductEditor key={id ?? 'new'} />
}

function ProductEditor() {
  const { id } = useParams()
  const location = useLocation()
  const [returnTo] = useState(() => returnUrl(location.state))
  const allowed = useEditorRole()
  const navigate = useNavigate()
  const [loaded, setLoaded] = useState<ProductMasterItem | null>(null)
  const [draft, setDraft] = useState<ProductDraft>({ sku: '', name: '', unit: '', drawingNumber: '' })
  const [initial, setInitial] = useState<ProductDraft>({ sku: '', name: '', unit: '', drawingNumber: '' })
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [notice, setNotice] = useState<Notice | null>(null)
  const [forbidden, setForbidden] = useState(false)
  const [busy, setBusy] = useState(false)
  const [loadError, setLoadError] = useState('')
  const firstError = useRef<HTMLInputElement>(null)
  const nameInput = useRef<HTMLInputElement>(null)
  const drawingInput = useRef<HTMLInputElement>(null)
  const unitInput = useRef<HTMLSelectElement>(null)
  const headingRef = useRef<HTMLHeadingElement>(null)
  const dialogRef = useRef<HTMLDialogElement>(null)
  const keepButton = useRef<HTMLButtonElement>(null)
  const [confirmingDiscard, setConfirmingDiscard] = useState(false)
  const pendingTo = useRef(returnTo)
  const pendingReload = useRef(false)
  const [needsRead, setNeedsRead] = useState(false)
  const returnFocus = useRef<HTMLElement | null>(null)
  const guard = useNavigationGuard()
  const dirty = JSON.stringify(draft) !== JSON.stringify(initial)
  const [reload, setReload] = useState(0)
  const heading = id ? words.edit : words.new

  useEffect(() => { document.title = labels.app.title(heading); headingRef.current?.focus() }, [heading])
  useEffect(() => guard.register((to) => {
    if (!dirty) return false
    pendingReload.current = false
    pendingTo.current = to
    returnFocus.current = document.activeElement instanceof HTMLElement ? document.activeElement : null
    setConfirmingDiscard(true)
    return true
  }), [dirty, guard])
  useEffect(() => {
    if (confirmingDiscard && !dialogRef.current?.open) {
      dialogRef.current?.showModal()
      keepButton.current?.focus()
    } else if (!confirmingDiscard && dialogRef.current?.open) dialogRef.current.close()
  }, [confirmingDiscard])
  useEffect(() => {
    if (!id || !allowed) return
    const controller = new AbortController()
    getMaster(id, controller.signal).then((item) => {
      if (controller.signal.aborted) return
      setLoaded(item)
      const values = { sku: item.sku, name: item.name, unit: item.unit, drawingNumber: item.drawingNumber ?? '' }
      setDraft(values)
      setInitial(values)
      setLoadError('')
      setNeedsRead(false)
      setErrors({})
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) { setForbidden(true); return }
      setLoadError(reason instanceof ApiError && reason.status === 404 ? words.noProduct : words.failed)
    })
    return () => controller.abort()
  }, [id, allowed, reload])

  function setField(field: keyof ProductDraft, value: string) {
    setDraft((old) => ({ ...old, [field]: value }))
    setErrors((old) => ({ ...old, [field]: '' }))
  }

  function focusFirst(fields: Record<string, unknown>) {
    if (fields.sku) firstError.current?.focus()
    else if (fields.name) nameInput.current?.focus()
    else if (fields.unit) unitInput.current?.focus()
    else if (fields.drawingNumber) drawingInput.current?.focus()
    else headingRef.current?.focus()
  }

  async function save(event: FormEvent) {
    event.preventDefault()
    const next: Record<string, string> = {}
    if (!draft.sku.trim() || [...draft.sku.trim()].length > 50) next.sku = words.invalid
    if (!draft.name.trim() || [...draft.name.trim()].length > 200) next.name = words.invalid
    if (!productUnits.includes(draft.unit as typeof productUnits[number])) next.unit = words.invalid
    if ([...draft.drawingNumber.trim()].length > 100) next.drawingNumber = words.invalid
    if (Object.keys(next).length) {
      setErrors(next)
      setNotice({ text: words.invalid, tone: 'error' })
      focusFirst(next)
      return
    }
    setBusy(true)
    setNotice(null)
    try {
      const saved = id && loaded ? await updateMaster(id, draft, loaded.version) : await createMaster(draft)
      // Back to the list the user came from, which announces the save (004_BD flow, SPD-03 step 4).
      navigate(returnTo, { state: { productSaved: { id: saved.id, created: !id } } })
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) { setForbidden(true); return }
      if (reason instanceof ApiError && reason.problem?.code === 'PRODUCT_STALE') { setNotice({ text: words.stale, tone: 'error' }); setNeedsRead(true) }
      else if (reason instanceof ApiError && reason.problem?.code === 'PRODUCT_UNIT_LOCKED') { setErrors({ unit: words.unitConflict }); setNotice({ text: words.unitConflict, tone: 'error' }); setNeedsRead(true); headingRef.current?.focus() }
      else if (reason instanceof ApiError && reason.problem?.code === 'PRODUCT_SKU_CONFLICT') { setErrors({ sku: words.skuConflict }); setNotice({ text: words.invalid, tone: 'error' }); firstError.current?.focus() }
      else if (reason instanceof ApiError && reason.status === 400) {
        const fields = reason.problem?.errors ?? {}
        setErrors(Object.fromEntries(Object.keys(fields).map((key) => [key, words.invalid])))
        setNotice({ text: words.invalid, tone: 'error' })
        focusFirst(fields)
      } else { setNotice({ text: words.failed, tone: 'error' }); setNeedsRead(true) }
    } finally { setBusy(false) }
  }

  function leave(trigger: HTMLElement) {
    pendingReload.current = false
    pendingTo.current = returnTo
    returnFocus.current = trigger
    if (dirty) setConfirmingDiscard(true)
    else navigate(returnTo)
  }

  // A red asterisk marks required fields (WI-013 DEC-011); the inputs carry `required`, so it is hidden from screen readers.
  const required = <span aria-hidden="true" className="ml-1 text-[#a12424]">{words.required}</span>
  const describedBy = (...ids: (string | false)[]) => ids.filter(Boolean).join(' ') || undefined

  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-2xl gap-5 px-4 py-6 text-[#172334] sm:px-6"><nav><GuardedLink className="underline" to={returnTo}>{words.heading}</GuardedLink></nav><h1 ref={headingRef} tabIndex={-1} className="text-2xl font-bold focus:outline-none">{heading}</h1>
    {!allowed || forbidden ? <Forbidden /> : loadError ? <div role="alert" className={`${errorBox} flex flex-wrap items-center gap-3`}><span>{loadError}</span><button type="button" className={secondaryButton} onClick={() => setReload((n) => n + 1)}>{labels.common.retry}</button></div> : id && loaded?.id !== id ? <p>{labels.order.loading}</p> : <>
      {loaded && !loaded.isActive && <p className="flex flex-wrap items-center gap-3"><StatusPill active={false} /><span>{words.retiredHint}</span></p>}
      <div role="status" aria-live="polite" className="empty:hidden"><NoticeBox notice={notice} /></div>
      {needsRead && <p>{id ? <button type="button" className={secondaryButton} onClick={(event) => {
        if (dirty) { pendingReload.current = true; returnFocus.current = event.currentTarget; setConfirmingDiscard(true) }
        else setReload((n) => n + 1)
      }}>{labels.common.reload}</button> : <GuardedLink className="underline" to={`/products?${new URLSearchParams({ q: draft.sku.trim() })}`}>{words.lookup}</GuardedLink>}</p>}
      <form action={id ? `/api/product-master/${encodeURIComponent(id)}` : '/api/product-master'} method="post" noValidate onSubmit={(event) => void save(event)} className="grid gap-[18px]">
        <div><label htmlFor="sku" className={labelClass}>{words.sku}{!id && required}</label>
          {id ? <div className={readOnlyClass}><input ref={firstError} id="sku" name="sku" readOnly value={draft.sku} className={readOnlyInput} aria-invalid={!!errors.sku} aria-describedby={describedBy('sku-fixed', !!errors.sku && 'sku-error')} /><span id="sku-fixed">{words.readOnly}</span></div>
            : <input ref={firstError} id="sku" name="sku" required maxLength={50} value={draft.sku} onChange={(e) => setField('sku', e.target.value)} className={inputClass} aria-invalid={!!errors.sku} aria-describedby={describedBy(!!errors.sku && 'sku-error')} />}
          {errors.sku && <p id="sku-error" className={fieldErrorClass}>{errors.sku}</p>}</div>
        <div><label htmlFor="name" className={labelClass}>{words.name}{required}</label><input ref={nameInput} id="name" name="name" required maxLength={200} value={draft.name} onChange={(e) => setField('name', e.target.value)} className={inputClass} aria-invalid={!!errors.name} aria-describedby={describedBy(!!errors.name && 'name-error')} />{errors.name && <p id="name-error" className={fieldErrorClass}>{errors.name}</p>}</div>
        <div><label htmlFor="unit" className={labelClass}>{words.unit}{required}</label>
          {loaded?.unitLocked ? <div className={readOnlyClass}><input id="unit" name="unit" readOnly value={draft.unit} className={readOnlyInput} aria-invalid={!!errors.unit} aria-describedby={describedBy('unit-fixed', 'unit-help', !!errors.unit && 'unit-error')} /><span id="unit-fixed">{words.readOnly}</span></div>
            : <select ref={unitInput} id="unit" name="unit" required value={draft.unit} onChange={(e) => setField('unit', e.target.value)} className={inputClass} aria-invalid={!!errors.unit} aria-describedby={describedBy(!!errors.unit && 'unit-error')}><option value="">—</option>{productUnits.map((unit) => <option key={unit}>{unit}</option>)}</select>}
          {loaded?.unitLocked && <p id="unit-help" className={hintClass}>{words.unitLocked}</p>}{errors.unit && <p id="unit-error" className={fieldErrorClass}>{errors.unit}</p>}</div>
        <div><label htmlFor="drawingNumber" className={labelClass}>{words.drawing}</label><input ref={drawingInput} id="drawingNumber" name="drawingNumber" maxLength={100} value={draft.drawingNumber} onChange={(e) => setField('drawingNumber', e.target.value)} className={inputClass} aria-invalid={!!errors.drawingNumber} aria-describedby={describedBy('drawing-hint', !!errors.drawingNumber && 'drawing-error')} /><p id="drawing-hint" className={hintClass}>{words.optional}</p>{errors.drawingNumber && <p id="drawing-error" className={fieldErrorClass}>{errors.drawingNumber}</p>}</div>
        <div className="flex flex-wrap gap-2.5"><button type="submit" disabled={busy} className={primaryButton}>{busy ? words.saving : words.save}</button><button type="button" className={secondaryButton} onClick={(event) => leave(event.currentTarget)}>{words.cancel}</button></div>
      </form>
    </>}
    <dialog ref={dialogRef} aria-labelledby="product-discard-title" aria-describedby="product-discard-body" onCancel={(event) => { event.preventDefault(); setConfirmingDiscard(false); returnFocus.current?.focus() }} className={dialogClass}><h2 id="product-discard-title" className="text-xl font-bold">{words.discardTitle}</h2><p id="product-discard-body" className="my-4">{words.discardBody}</p><div className="flex flex-wrap gap-2.5"><button ref={keepButton} type="button" className={secondaryButton} onClick={() => { setConfirmingDiscard(false); returnFocus.current?.focus() }}>{words.keepEditing}</button><button type="button" className={dangerButton} onClick={() => { setConfirmingDiscard(false); if (pendingReload.current) { pendingReload.current = false; setReload((n) => n + 1) } else navigate(pendingTo.current) }}>{words.discard}</button></div></dialog>
  </main></div>
}
