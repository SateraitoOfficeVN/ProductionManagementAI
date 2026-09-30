import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { GuardedLink } from '../../components/GuardedLink'
import { ApiError } from '../../lib/apiClient'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { useAuth } from '../auth/useAuth'
import { labels, message } from '../production-orders/messages'
import {
  createMaster, getMaster, listMaster, productUnits, retireMaster, updateMaster,
  type ProductDraft, type ProductMasterItem, type ProductPage,
} from './api'

const words = labels.products
const inputClass = 'min-h-12 w-full rounded border border-gray-400 px-3 py-2 text-base focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900'
const buttonClass = 'min-h-12 rounded border border-gray-400 px-4 py-2 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900'

function readProductFlash(id: string | undefined): string {
  if (!id) return ''
  return sessionStorage.getItem(`pmai:product-flash:${id}`) ?? ''
}

function useEditorRole() {
  const { user } = useAuth()
  return user?.roles.some((role) => role === 'Admin' || role === 'Operator') ?? false
}

/** Product catalog with URL backed search, paging and a native retirement dialog. */
export function ProductMasterPage() {
  const allowed = useEditorRole()
  const [params, setParams] = useSearchParams()
  const q = (params.get('q') ?? '').trim().slice(0, 100)
  const state = ['all', 'active', 'retired'].includes(params.get('state') ?? '') ? params.get('state')! : 'all'
  const parsedPage = Number(params.get('page') ?? '1')
  const page = Number.isSafeInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1
  const searchKey = `${q}\u0000${state}`
  const [searchDraft, setSearchDraft] = useState<{ key: string; search: string; filter: string } | null>(null)
  const search = searchDraft?.key === searchKey ? searchDraft.search : q
  const filter = searchDraft?.key === searchKey ? searchDraft.filter : state
  const [result, setResult] = useState<ProductPage | null>(null)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [retire, setRetire] = useState<ProductMasterItem | null>(null)
  const [busy, setBusy] = useState(false)
  const [refresh, setRefresh] = useState(0)
  const dialog = useRef<HTMLDialogElement>(null)
  const retireCancel = useRef<HTMLButtonElement>(null)
  const resultHeading = useRef<HTMLHeadingElement>(null)
  const returnFocus = useRef<HTMLButtonElement>(null)

  useEffect(() => { document.title = labels.app.title(words.heading) }, [])
  useEffect(() => {
    if (params.getAll('q').length > 1 || params.getAll('state').length > 1 ||
        params.getAll('page').length > 1 || q !== (params.get('q') ?? '') ||
        state !== (params.get('state') ?? 'all') || String(page) !== (params.get('page') ?? '1')) {
      setParams({ q, state, page: String(page) }, { replace: true })
    }
  }, [params, q, state, page, setParams])
  useEffect(() => {
    if (!allowed) return
    const controller = new AbortController()
    listMaster(q, state, page, controller.signal).then((value) => {
      setResult(value)
      setError('')
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      setResult(null)
      if (reason instanceof ApiError && reason.status === 401) return
      setError(reason instanceof ApiError && reason.status === 403 ? message('MSG-E012') : words.failed)
    })
    return () => controller.abort()
  }, [allowed, q, state, page, refresh])
  useEffect(() => {
    if (retire) { dialog.current?.showModal(); retireCancel.current?.focus() }
    else dialog.current?.close()
  }, [retire])

  function apply(event: FormEvent) {
    event.preventDefault()
    setParams({ q: search.trim().slice(0, 100), state: filter, page: '1' })
    requestAnimationFrame(() => resultHeading.current?.focus())
  }

  async function confirmRetire() {
    if (!retire || busy) return
    setBusy(true)
    try {
      await retireMaster(retire.id, retire.version)
      setNotice(words.retireDone)
    } catch (reason) {
      setNotice(reason instanceof ApiError && reason.problem?.code === 'PRODUCT_STALE' ? words.stale : words.failed)
    } finally {
      setBusy(false)
      setRetire(null)
      setRefresh((value) => value + 1)
      requestAnimationFrame(() => (returnFocus.current?.isConnected ? returnFocus.current : resultHeading.current)?.focus())
    }
  }

  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-5xl gap-5 px-4 py-6 sm:px-6">
    <div className="flex flex-wrap items-center justify-between gap-3"><h1 className="text-2xl font-semibold">{words.heading}</h1>
      {allowed && <Link className={buttonClass} to="/products/new">{words.new}</Link>}</div>
    {!allowed ? <p role="alert">{message('MSG-E012')}</p> : <>
      <form action="/products" method="get" onSubmit={apply} className="grid gap-3 rounded border border-gray-300 p-4 sm:grid-cols-[1fr_11rem_auto] sm:items-end">
        <div><label htmlFor="product-search" className="mb-1 block">{words.search}</label><input id="product-search" name="q" maxLength={100} value={search} onChange={(e) => setSearchDraft({ key: searchKey, search: e.target.value, filter })} className={inputClass} /></div>
        <div><label htmlFor="product-state" className="mb-1 block">{words.state}</label><select id="product-state" name="state" value={filter} onChange={(e) => setSearchDraft({ key: searchKey, search, filter: e.target.value })} className={inputClass}>
          <option value="all">{words.all}</option><option value="active">{words.active}</option><option value="retired">{words.retired}</option></select></div>
        <div className="flex gap-2"><button type="submit" className={buttonClass}>{words.apply}</button><button type="button" className={buttonClass} onClick={() => { setSearchDraft(null); setParams({}); resultHeading.current?.focus() }}>{labels.list.clear}</button></div>
      </form>
      <p aria-live="polite">{notice}</p>{error && <p role="alert">{error} <button type="button" className={buttonClass} onClick={() => setRefresh((n) => n + 1)}>{labels.common.retry}</button></p>}
      <h2 ref={resultHeading} tabIndex={-1} className="text-lg font-medium focus:outline-none">{result ? words.result(result.total) : ''}</h2>
      {result && (result.items.length === 0 ? <p>{words.noResults}</p> : <>
        <div className="hidden overflow-x-auto sm:block"><table className="w-full border-collapse text-left"><caption className="sr-only">{words.table}</caption><thead><tr className="border-b border-gray-300">
          <th scope="col" className="p-2">{words.sku}</th><th scope="col" className="p-2">{words.name}</th><th scope="col" className="p-2">{words.unit}</th><th scope="col" className="p-2">{words.drawing}</th><th scope="col" className="p-2">{words.state}</th><th scope="col" className="p-2">{words.editAction}</th></tr></thead><tbody>
          {result.items.map((item) => <tr key={item.id} className="border-b border-gray-200"><th scope="row" className="p-2 font-medium">{item.sku}</th><td className="p-2">{item.name}</td><td className="p-2">{item.unit}</td><td className="p-2">{item.drawingNumber ?? '—'}</td><td className="p-2">{item.isActive ? words.active : words.retired}</td><td className="flex flex-wrap gap-2 p-2"><Link aria-label={words.editNamed(item.sku)} to={`/products/${item.id}`} className={buttonClass}>{words.editAction}</Link>{item.isActive && <button type="button" aria-label={words.retireNamed(item.sku)} className={buttonClass} onClick={(e) => { returnFocus.current = e.currentTarget; setRetire(item) }}>{words.retire}</button>}</td></tr>)}
        </tbody></table></div>
        <ul className="grid gap-3 sm:hidden">{result.items.map((item) => <li key={item.id} className="grid gap-2 rounded border border-gray-300 p-4"><strong>{item.sku} — {item.name}</strong><span>{words.unit}: {item.unit}</span><span>{words.drawing}: {item.drawingNumber ?? '—'}</span><span>{words.state}: {item.isActive ? words.active : words.retired}</span><div className="flex flex-wrap gap-2"><Link aria-label={words.editNamed(item.sku)} to={`/products/${item.id}`} className={buttonClass}>{words.editAction}</Link>{item.isActive && <button type="button" aria-label={words.retireNamed(item.sku)} className={buttonClass} onClick={(e) => { returnFocus.current = e.currentTarget; setRetire(item) }}>{words.retire}</button>}</div></li>)}</ul>
      </>)}
      {result && <nav aria-label={labels.list.pagination} className="flex items-center justify-between gap-3"><button type="button" className={buttonClass} disabled={page <= 1} onClick={() => setParams({ q, state, page: String(page - 1) })}>{words.previous}</button><span>{page}</span><button type="button" className={buttonClass} disabled={page * 20 >= result.total} onClick={() => setParams({ q, state, page: String(page + 1) })}>{words.next}</button></nav>}
    </>}
    <dialog ref={dialog} aria-labelledby="retire-title" onClose={() => { setRetire(null); if (returnFocus.current?.isConnected) returnFocus.current.focus() }} aria-describedby="retire-description" className="max-w-md rounded border border-gray-400 p-5 shadow-xl backdrop:bg-black/40"><h2 id="retire-title" className="text-lg font-semibold">{words.retireQuestion}</h2><p id="retire-description" className="my-4">{retire?.sku} — {retire?.name}</p><div className="flex justify-end gap-2"><button ref={retireCancel} type="button" className={buttonClass} onClick={() => setRetire(null)}>{words.cancel}</button><button type="button" className={buttonClass} disabled={busy} onClick={() => void confirmRetire()}>{words.retire}</button></div></dialog>
  </main></div>
}

/** Create and versioned edit form; server field errors remain tied to their inputs. */
export function ProductFormPage() {
  const { id } = useParams()
  return <ProductEditor key={id ?? 'new'} />
}

function ProductEditor() {
  const { id } = useParams()
  const allowed = useEditorRole()
  const navigate = useNavigate()
  const [loaded, setLoaded] = useState<ProductMasterItem | null>(null)
  const [draft, setDraft] = useState<ProductDraft>({ sku: '', name: '', unit: '', drawingNumber: '' })
  const [initial, setInitial] = useState<ProductDraft>({ sku: '', name: '', unit: '', drawingNumber: '' })
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [notice, setNotice] = useState(() => readProductFlash(id))
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
  const pendingTo = useRef('/products')
  const pendingReload = useRef(false)
  const [needsRead, setNeedsRead] = useState(false)
  const returnFocus = useRef<HTMLElement | null>(null)
  const guard = useNavigationGuard()
  const dirty = JSON.stringify(draft) !== JSON.stringify(initial)
  const [reload, setReload] = useState(0)
  const heading = id ? words.edit : words.new

  useEffect(() => { document.title = labels.app.title(heading); headingRef.current?.focus() }, [heading])
  useEffect(() => {
    if (id) sessionStorage.removeItem(`pmai:product-flash:${id}`)
  }, [id])
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
      setLoadError(reason instanceof ApiError && reason.status === 404 ? words.noProduct : words.failed)
    })
    return () => controller.abort()
  }, [id, allowed, reload])

  function setField(field: keyof ProductDraft, value: string) {
    setDraft((old) => ({ ...old, [field]: value }))
    setErrors((old) => ({ ...old, [field]: '' }))
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
      if (next.sku) firstError.current?.focus()
      else if (next.name) nameInput.current?.focus()
      else if (next.unit) unitInput.current?.focus()
      else drawingInput.current?.focus()
      return
    }
    setBusy(true)
    setNotice('')
    try {
      const saved = id && loaded ? await updateMaster(id, draft, loaded.version) : await createMaster(draft)
      if (!id) {
        sessionStorage.setItem(`pmai:product-flash:${saved.id}`, words.created)
        navigate(`/products/${saved.id}`, { replace: true })
      }
      setLoaded(saved)
      const values = { sku: saved.sku, name: saved.name, unit: saved.unit, drawingNumber: saved.drawingNumber ?? '' }
      setDraft(values)
      setInitial(values)
      setErrors({})
      setNotice(id ? words.saved : words.created)
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) { setNotice(message('MSG-E012')); return }
      if (reason instanceof ApiError && reason.problem?.code === 'PRODUCT_STALE') { setNotice(words.stale); setNeedsRead(true) }
      else if (reason instanceof ApiError && reason.problem?.code === 'PRODUCT_UNIT_LOCKED') { setErrors({ unit: words.unitConflict }); setNotice(words.unitConflict); setNeedsRead(true); headingRef.current?.focus() }
      else if (reason instanceof ApiError && reason.problem?.code === 'PRODUCT_SKU_CONFLICT') { setErrors({ sku: words.skuConflict }); setNotice(words.skuConflict); firstError.current?.focus() }
      else if (reason instanceof ApiError && reason.status === 400) {
        setErrors(Object.fromEntries(Object.keys(reason.problem?.errors ?? {}).map((key) => [key, words.invalid])))
        setNotice(words.invalid)
        const fields = reason.problem?.errors ?? {}
        if (fields.sku) firstError.current?.focus()
        else if (fields.name) nameInput.current?.focus()
        else if (fields.unit) unitInput.current?.focus()
        else if (fields.drawingNumber) drawingInput.current?.focus()
        else headingRef.current?.focus()
      } else { setNotice(words.failed); setNeedsRead(true) }
    } finally { setBusy(false) }
  }

  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-2xl gap-5 px-4 py-6 sm:px-6"><nav><GuardedLink className="underline" to="/products">{words.heading}</GuardedLink></nav><h1 ref={headingRef} tabIndex={-1} className="text-2xl font-semibold focus:outline-none">{heading}</h1>
    {!allowed ? <p role="alert">{message('MSG-E012')}</p> : loadError ? <p role="alert">{loadError} <button type="button" className={buttonClass} onClick={() => setReload((n) => n + 1)}>{labels.common.retry}</button></p> : id && loaded?.id !== id ? <p>{labels.order.loading}</p> : <>
      <p role="status" aria-live="polite">{notice}</p>
      {needsRead && (id ? <button type="button" className={buttonClass} onClick={(event) => {
        if (dirty) { pendingReload.current = true; returnFocus.current = event.currentTarget; setConfirmingDiscard(true) }
        else setReload((n) => n + 1)
      }}>{labels.common.reload}</button> : <GuardedLink className="underline" to={`/products?q=${encodeURIComponent(draft.sku.trim())}`}>{words.search}</GuardedLink>)}
      {loaded && <p>{words.state}: {loaded.isActive ? words.active : words.retired}</p>}
      <form action={id ? `/api/product-master/${encodeURIComponent(id)}` : '/api/product-master'} method="post" noValidate onSubmit={(event) => void save(event)} className="grid gap-4">
        <div><label htmlFor="sku" className="mb-1 block">{words.sku} *</label><input ref={firstError} id="sku" name="sku" required maxLength={50} readOnly={!!id} value={draft.sku} onChange={(e) => setField('sku', e.target.value)} className={inputClass} aria-invalid={!!errors.sku} aria-describedby={errors.sku ? 'sku-error' : undefined} />{errors.sku && <p id="sku-error" className="text-red-700">{errors.sku}</p>}</div>
        <div><label htmlFor="name" className="mb-1 block">{words.name} *</label><input ref={nameInput} id="name" name="name" required maxLength={200} value={draft.name} onChange={(e) => setField('name', e.target.value)} className={inputClass} aria-invalid={!!errors.name} aria-describedby={errors.name ? 'name-error' : undefined} />{errors.name && <p id="name-error" className="text-red-700">{errors.name}</p>}</div>
        <div><label htmlFor="unit" className="mb-1 block">{words.unit} *</label>{loaded?.unitLocked ? <input id="unit" name="unit" readOnly value={draft.unit} className={inputClass} aria-describedby="unit-help" /> : <select ref={unitInput} id="unit" name="unit" required value={draft.unit} onChange={(e) => setField('unit', e.target.value)} className={inputClass} aria-invalid={!!errors.unit} aria-describedby={loaded?.unitLocked ? 'unit-help' : errors.unit ? 'unit-error' : undefined}><option value="">—</option>{productUnits.map((unit) => <option key={unit}>{unit}</option>)}</select>}{loaded?.unitLocked && <p id="unit-help">{words.unitLocked}</p>}{errors.unit && <p id="unit-error" className="text-red-700">{errors.unit}</p>}</div>
        <div><label htmlFor="drawingNumber" className="mb-1 block">{words.drawing}</label><input ref={drawingInput} id="drawingNumber" name="drawingNumber" maxLength={100} value={draft.drawingNumber} onChange={(e) => setField('drawingNumber', e.target.value)} className={inputClass} aria-invalid={!!errors.drawingNumber} aria-describedby={errors.drawingNumber ? 'drawing-error' : undefined} />{errors.drawingNumber && <p id="drawing-error" className="text-red-700">{errors.drawingNumber}</p>}</div>
        <div className="flex justify-end gap-3"><button type="button" className={buttonClass} onClick={(event) => { pendingReload.current = false; pendingTo.current = '/products'; returnFocus.current = event.currentTarget; if (dirty) setConfirmingDiscard(true); else navigate('/products') }}>{words.cancel}</button><button type="submit" disabled={busy} className={buttonClass}>{busy ? words.saving : words.save}</button></div>
      </form>
    </>}
    <dialog ref={dialogRef} aria-labelledby="product-discard-title" aria-describedby="product-discard-body" onCancel={(event) => { event.preventDefault(); setConfirmingDiscard(false); returnFocus.current?.focus() }} className="max-w-md rounded border border-gray-400 p-5 shadow-xl backdrop:bg-black/40"><h2 id="product-discard-title" className="text-lg font-semibold">{words.discardTitle}</h2><p id="product-discard-body" className="my-4">{words.discardBody}</p><div className="flex justify-end gap-2"><button ref={keepButton} type="button" className={buttonClass} onClick={() => { setConfirmingDiscard(false); returnFocus.current?.focus() }}>{words.keepEditing}</button><button type="button" className={buttonClass} onClick={() => { setConfirmingDiscard(false); if (pendingReload.current) { pendingReload.current = false; setReload((n) => n + 1) } else navigate(pendingTo.current) }}>{words.discard}</button></div></dialog>
  </main></div>
}
