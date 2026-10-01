import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { GuardedLink } from '../../components/GuardedLink'
import { ApiError } from '../../lib/apiClient'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { labels } from '../production-orders/messages'
import { createLine, decimalValid, mutationWithinLimits, getLine, listLines, productChoices, updateLine, type LineDetail, type PairDetail, type Page, type ProductChoice, type ProductChange, type TimingInput } from './api'
import { LineDialog, lineButtonClass as button, lineInputClass as input } from './LineDialog'
import { useLineRole } from './lineViewRules'

const w = labels.lines
interface Row { product: ProductChoice; minutes: string; confirm: boolean; origin?: PairDetail; page: number; action?: 'add' | 'setTiming' | 'retire' }
interface Fields { code: string; name: string; hours: string }
export function ProductionLineFormPage() { const { id } = useParams(); return <LineEditor key={id ?? 'new'} /> }
function LineEditor() {
  const { id } = useParams()
  const allowed = useLineRole()
  const navigate = useNavigate()
  const guard = useNavigationGuard()
  const [loaded, setLoaded] = useState<LineDetail | null>(null)
  const [fields, setFields] = useState<Fields>({ code: '', name: '', hours: '' })
  const [baseline, setBaseline] = useState<Fields>({ code: '', name: '', hours: '' })
  const [rows, setRows] = useState<Record<string, Row>>({})
  const [pageIds, setPageIds] = useState<string[]>([])
  const [pairPage, setPairPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [reload, setReload] = useState(0)
  const [readError, setReadError] = useState('')
  const [forbidden, setForbidden] = useState(false)
  const [pending, setPending] = useState(false)
  const [blocked, setBlocked] = useState(false)
  const [notice, setNotice] = useState('')
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [choiceQuery, setChoiceQuery] = useState('')
  const [choiceSearch, setChoiceSearch] = useState('')
  const [choicePage, setChoicePage] = useState(1)
  const [choices, setChoices] = useState<Page<ProductChoice> | null>(null)
  const [choiceResultKey, setChoiceResultKey] = useState('')
  const choiceKey = `${choiceQuery}\u0000${choicePage}\u0000${reload}`
  const currentChoices = choiceResultKey === choiceKey ? choices : null
  const [choiceError, setChoiceError] = useState('')
  const [confirm, setConfirm] = useState<{ kind: 'leave'; to: string } | { kind: 'reload' } | { kind: 'pair'; id: string } | null>(null)
  const [verification, setVerification] = useState<LineDetail[] | null>(null)
  const [cooldown, setCooldown] = useState(false)
  const heading = useRef<HTMLHeadingElement>(null)
  const refs = useRef<Record<string, HTMLInputElement | null>>({})
  const version = useRef<string | null>(null)
  const pendingFocus = useRef<string | null>(null)
  const dirty = JSON.stringify(fields) !== JSON.stringify(baseline) || Object.values(rows).some(row => row.action !== undefined)
  const editable = allowed && !forbidden && (!id || loaded !== null)
  const title = id ? w.edit : w.new
  useEffect(() => { document.title = labels.app.title(title); heading.current?.focus() }, [title])
  useEffect(() => guard.register(to => {
    if (pending) return true
    if (!dirty) return false
    setConfirm({ kind: 'leave', to }); return true
  }), [guard, pending, dirty])
  useEffect(() => {
    if (!dirty && !pending) return
    const warn = (event: BeforeUnloadEvent) => { event.preventDefault(); event.returnValue = '' }
    window.addEventListener('beforeunload', warn)
    return () => window.removeEventListener('beforeunload', warn)
  }, [dirty, pending])
  useEffect(() => {
    if (!id || !allowed || forbidden) return
    const controller = new AbortController()
    getLine(id, pairPage, controller.signal).then(line => {
      if (controller.signal.aborted) return
      if (version.current !== null && version.current !== line.version) { setBlocked(true); setNotice(w.conflict); return }
      if (version.current === null) {
        version.current = line.version; setLoaded(line)
        const initial = { code: line.code, name: line.name, hours: line.workingHoursPerDay }
        setFields(initial); setBaseline(initial)
      }
      setRows(previous => {
        const next = { ...previous }
        for (const pair of line.pairs.items) {
          const old = next[pair.product.id]
          if (!old) next[pair.product.id] = { product: pair.product, minutes: pair.minutesPerUnit, confirm: false, origin: pair, page: pairPage }
          else {
            const changedObservation = old.product.unit !== pair.product.unit || old.product.unitRevision !== pair.product.unitRevision
            next[pair.product.id] = { ...old, product: pair.product, origin: pair, page: pairPage, confirm: changedObservation ? false : old.confirm }
          }
        }
        return next
      })
      setPageIds(line.pairs.items.map(pair => pair.product.id)); setTotal(line.pairs.total); setReadError('')
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) { setForbidden(true); setLoaded(null); setRows({}); return }
      setReadError(reason instanceof ApiError && reason.status === 404 ? w.notFound : w.failed)
    })
    return () => controller.abort()
  }, [id, allowed, forbidden, pairPage, reload])
  useEffect(() => {
    if (!editable || forbidden || [...choiceQuery].length > 100) return
    const controller = new AbortController()
    productChoices(choiceQuery, choicePage, id, controller.signal).then(value => {
      if (!controller.signal.aborted) { setChoices(value); setChoiceResultKey(choiceKey); setChoiceError('') }
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      setChoices(null)
      if (reason instanceof ApiError && reason.status === 403) { setForbidden(true); setLoaded(null); setRows({}) }
      else if (!(reason instanceof ApiError && reason.status === 401)) setChoiceError(w.failed)
    })
    return () => controller.abort()
  }, [editable, forbidden, choiceQuery, choicePage, id, reload, choiceKey])
  useEffect(() => {
    if (pendingFocus.current && refs.current[pendingFocus.current]) { refs.current[pendingFocus.current]?.focus(); pendingFocus.current = null }
  }, [rows, pageIds])
  function changeRow(rowId: string, patch: Partial<Row>) {
    setRows(previous => {
      const old = previous[rowId]
      if (!old) return previous
      const next = { ...old, ...patch, confirm: patch.minutes !== undefined && patch.minutes !== old.minutes ? false : patch.confirm ?? old.confirm }
      if (next.action !== 'add' && next.action !== 'retire') next.action = next.minutes !== next.origin?.minutesPerUnit || next.confirm ? 'setTiming' : undefined
      return { ...previous, [rowId]: next }
    })
  }
  function reveal(key: string) {
    const match = /^row:([^:]+):/.exec(key)
    if (match && rows[match[1]]) setPairPage(rows[match[1]].page)
    pendingFocus.current = key
    requestAnimationFrame(() => { if (refs.current[key]) { refs.current[key]?.focus(); pendingFocus.current = null } })
  }
  function discardAndReload() {
    version.current = null; setLoaded(null); setRows({}); setPageIds([]); setPairPage(1)
    setErrors({}); setBlocked(false); setVerification(null); setNotice(''); setReadError(''); setReload(n => n + 1)
    if (!id) { setFields({ code: '', name: '', hours: '' }); setBaseline({ code: '', name: '', hours: '' }) }
  }
  async function verify() {
    try {
      if (id) setVerification([await getLine(id)])
      else {
        const result = await listLines(fields.code.trim(), 'all', 1)
        const exact = result.items.filter(line => line.code.toLocaleLowerCase() === fields.code.trim().toLocaleLowerCase())
        setVerification(await Promise.all(exact.map(line => getLine(line.id))))
      }
      setNotice(w.verified)
    } catch { setNotice(w.failed) }
  }
  async function save(event: FormEvent) {
    event.preventDefault()
    if (!editable || pending || blocked || cooldown || !dirty) return
    const next: Record<string, string> = {}
    if (!fields.code.trim() || [...fields.code.trim()].length > 50) next.code = w.invalid
    if (!fields.name.trim() || [...fields.name.trim()].length > 200) next.name = w.invalid
    if (!decimalValid(fields.hours, '24')) next.hours = w.invalid
    const changed = Object.entries(rows).filter(([, row]) => row.action)
    for (const [key, row] of changed) if (row.action !== 'retire') {
      if (!decimalValid(row.minutes, '999999999.999')) next[`row:${key}:minutes`] = w.invalid
      if ((row.action === 'add' || row.origin?.requiresUnitConfirmation) && !row.confirm) next[`row:${key}:confirm`] = w.invalid
    }
    const timing = (row: Row): TimingInput => ({ productId: row.product.id, minutesPerUnit: row.minutes,
      expectedUnit: row.product.unit, expectedUnitRevision: row.product.unitRevision, confirmUnit: row.confirm })
    const changes: ProductChange[] = changed.map(([, row]) => row.action === 'retire' ? { action: 'retire', productId: row.product.id }
      : { action: row.action === 'add' ? 'add' : 'setTiming', ...timing(row) })
    const createBody = { code: fields.code.trim(), name: fields.name.trim(), workingHoursPerDay: fields.hours, products: changed.map(([, row]) => timing(row)) }
    const updateBody = { name: fields.name.trim(), workingHoursPerDay: fields.hours, version: loaded?.version ?? '', productChanges: changes }
    if (!mutationWithinLimits(id ? updateBody : createBody, changed.length)) next.body = w.invalid
    if (Object.keys(next).length) { setErrors(next); reveal(Object.keys(next)[0]); return }
    setPending(true); setErrors({}); setNotice('')
    try {
      if (id) await updateLine(id, updateBody); else await createLine(createBody)
      navigate('/production-lines', { state: { lineSaved: true } })
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) { setForbidden(true); setLoaded(null); setRows({}); return }
      if (reason instanceof ApiError && reason.problem?.code === 'LINE_BUSY' && reason.status === 503) {
        setNotice(w.busy); setCooldown(true); setTimeout(() => setCooldown(false), 1000)
      } else if (reason instanceof ApiError && reason.status < 500) {
        if (reason.status === 409 && reason.problem?.code !== 'LINE_CODE_CONFLICT') { setBlocked(true); setNotice(w.conflict) }
        else setNotice(reason.problem?.code === 'LINE_CODE_CONFLICT' ? w.codeConflict : w.invalid)
        const mapped: Record<string, string> = {}
        for (const field of Object.keys(reason.problem?.errors ?? {})) {
          const match = /^(?:products|productChanges)\[([0-9]+)\]\.(minutesPerUnit|expectedUnit|expectedUnitRevision|confirmUnit|productId|action)$/.exec(field)
          const row = match ? changed[Number(match[1])] : undefined
          const key = row && match ? `row:${row[0]}:${match[2] === 'minutesPerUnit' ? 'minutes' : 'confirm'}`
            : field === 'workingHoursPerDay' ? 'hours' : ['name', 'code'].includes(field) ? field : 'body'
          mapped[key] = reason.problem?.code === 'LINE_CODE_CONFLICT' ? w.codeConflict : w.invalid
        }
        setErrors(mapped); if (Object.keys(mapped).length) reveal(Object.keys(mapped)[0])
      } else { setBlocked(true); setNotice(w.unknown) }
    } finally { setPending(false) }
  }
  const visible = [...new Set([...pageIds, ...Object.keys(rows).filter(key => rows[key].action === 'add')])]
  const field = (key: keyof Fields, label: string, readOnly = false) => (<div><label htmlFor={`line-${key}`}>{label}</label>
    <input id={`line-${key}`} ref={element => { refs.current[key] = element }} className={input} value={fields[key]} readOnly={readOnly} disabled={pending}
      inputMode={key === 'hours' ? 'decimal' : 'text'} aria-invalid={Boolean(errors[key])} aria-describedby={errors[key] ? `line-${key}-error` : undefined}
      onChange={e => setFields(old => ({ ...old, [key]: e.target.value }))} />{errors[key] && <p id={`line-${key}-error`} role="alert">{errors[key]}</p>}</div>)
  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-5xl gap-5 px-4 py-6 sm:px-6">
    <h1 ref={heading} tabIndex={-1} className="text-2xl font-semibold">{title}</h1>
    {!allowed || forbidden ? <p role="alert">{w.forbidden}</p> : <>
      {readError && <p role="alert">{readError} <button type="button" className={button} onClick={() => setReload(n => n + 1)}>{labels.common.retry}</button></p>}
      {!editable && !readError && <p>{w.loading}</p>}
      {editable && <form onSubmit={event => void save(event)} className="grid gap-5">
        <div className="grid gap-4 rounded border border-gray-300 p-4 sm:grid-cols-2">{field('code', w.code, Boolean(id))}{field('name', w.name)}{field('hours', w.hours)}
          {loaded && <p>{w.state}: {loaded.isActive ? w.active : w.retired}</p>}</div>
        <p aria-live="polite">{notice}</p>
        {Object.keys(errors).length > 0 && <div role="alert"><p>{w.invalid}</p><ul>{Object.keys(errors).map(key => <li key={key}><button type="button" className="underline" onClick={() => reveal(key)}>{rows[key.split(':')[1]]?.product.sku ?? w.invalid}: {errors[key]}</button></li>)}</ul></div>}
        {blocked && <div className="flex flex-wrap gap-2"><button type="button" className={button} onClick={() => void verify()}>{w.verify}</button><button type="button" className={button} onClick={() => setConfirm({ kind: 'reload' })}>{w.reload}</button></div>}
        {verification && <section aria-label={w.verify}><p>{w.verified}</p>{verification.map(line => <p key={line.id}>{line.code} — {line.name} — {line.workingHoursPerDay} {w.hoursUnit} — {line.isActive ? w.active : w.retired}</p>)}</section>}
        <section className="grid gap-3 rounded border border-gray-300 p-4" aria-labelledby="line-products-title"><h2 id="line-products-title" className="text-lg font-medium">{w.products}</h2>
          {visible.length === 0 && <p>{w.noProducts}</p>}
          {visible.map(key => { const row = rows[key]; if (!row) return null; const locked = pending || row.action === 'retire' || row.origin?.isActive === false
            return <fieldset key={key} className="grid min-w-0 gap-3 rounded border border-gray-300 p-3 break-words"><legend>{row.product.sku} — {row.product.name}</legend>
              <p>{w.unit}: {row.product.unit} {!row.product.isActive || row.origin?.isActive === false || row.action === 'retire' ? w.retired : ''}</p>
              {row.origin?.requiresUnitConfirmation && <p>{w.needsConfirmation}</p>}
              <div><label htmlFor={`timing-${key}`}>{w.timing} ({w.minutes})</label><input id={`timing-${key}`} ref={element => { refs.current[`row:${key}:minutes`] = element }} className={input} inputMode="decimal" value={row.minutes} disabled={locked}
                aria-invalid={Boolean(errors[`row:${key}:minutes`])} aria-describedby={errors[`row:${key}:minutes`] ? `timing-${key}-error` : undefined} onChange={e => changeRow(key, { minutes: e.target.value })} />
                {errors[`row:${key}:minutes`] && <p id={`timing-${key}-error`}>{errors[`row:${key}:minutes`]}</p>}</div>
              <label className="flex min-h-12 items-center gap-2"><input type="checkbox" ref={element => { refs.current[`row:${key}:confirm`] = element }} checked={row.confirm} disabled={locked} aria-invalid={Boolean(errors[`row:${key}:confirm`])} onChange={e => changeRow(key, { confirm: e.target.checked })} />{w.confirmUnit}</label>
              {errors[`row:${key}:confirm`] && <p role="alert">{errors[`row:${key}:confirm`]}</p>}
              {row.action === 'add' ? <button type="button" className={button} disabled={pending} onClick={() => setRows(previous => { const next = { ...previous }; delete next[key]; return next })}>{w.remove}</button>
                : row.origin?.isActive && row.action !== 'retire' && <button type="button" className={button} disabled={pending} onClick={() => setConfirm({ kind: 'pair', id: key })}>{w.retire}</button>}
            </fieldset> })}
          {id && <nav aria-label={w.pairPagination} className="flex flex-wrap justify-between gap-3"><button type="button" disabled={pending || pairPage <= 1} className={button} onClick={() => { setPageIds([]); setPairPage(n => n - 1) }}>{w.previous}</button><span>{w.page(pairPage)}</span><button type="button" disabled={pending || pairPage >= 10000 || pairPage * 50 >= total} className={button} onClick={() => { setPageIds([]); setPairPage(n => n + 1) }}>{w.next}</button></nav>}
        </section>
        <section className="grid gap-3 rounded border border-gray-300 p-4" aria-labelledby="line-add-title"><h2 id="line-add-title" className="text-lg font-medium">{w.add}</h2>
          <div className="flex flex-wrap items-end gap-2"><div className="min-w-0 flex-1"><label htmlFor="line-product-search">{w.productSearch}</label><input id="line-product-search" className={input} value={choiceSearch} onChange={e => setChoiceSearch(e.target.value)} /></div><button type="button" className={button} onClick={() => { if ([...choiceSearch.trim()].length > 100) setChoiceError(w.invalid); else { setChoiceQuery(choiceSearch.trim()); setChoicePage(1) } }}>{w.apply}</button></div>
          {choiceError && <p role="alert">{choiceError}</p>}
          {currentChoices && currentChoices.items.filter(product => !rows[product.id]).map(product => <div key={product.id} className="flex flex-wrap items-center justify-between gap-2 break-words"><span>{product.sku} — {product.name} ({product.unit})</span><button type="button" className={button} disabled={pending} onClick={() => setRows(previous => ({ ...previous, [product.id]: { product, minutes: '', confirm: false, page: 1, action: 'add' } }))}>{w.add}</button></div>)}
          {currentChoices?.items.filter(product => !rows[product.id]).length === 0 && <p>{w.noChoices}</p>}
          {currentChoices && <nav aria-label={w.choicePagination} className="flex flex-wrap justify-between gap-3"><button type="button" className={button} disabled={choicePage <= 1} onClick={() => setChoicePage(n => n - 1)}>{w.previous}</button><span>{w.page(choicePage)}</span><button type="button" className={button} disabled={choicePage >= 10000 || choicePage * 50 >= currentChoices.total} onClick={() => setChoicePage(n => n + 1)}>{w.next}</button></nav>}
        </section>
        <div className="flex flex-wrap justify-end gap-3"><GuardedLink className={button} to="/production-lines">{w.cancel}</GuardedLink><button type="submit" className={button} disabled={!dirty || pending || blocked || cooldown}>{pending ? w.saving : w.save}</button></div>
      </form>}
    </>}
    <LineDialog open={confirm !== null} title={confirm?.kind === 'pair' ? w.pairQuestion : w.discardQuestion} confirm={confirm?.kind === 'pair' ? w.retire : w.discard} busy={pending} onCancel={() => setConfirm(null)} onConfirm={() => {
      if (confirm?.kind === 'pair') changeRow(confirm.id, { action: 'retire' })
      else if (confirm?.kind === 'reload') discardAndReload()
      else if (confirm?.kind === 'leave') navigate(confirm.to)
      setConfirm(null)
    }}>{confirm?.kind === 'pair' ? rows[confirm.id]?.product.sku : w.discardDescription}</LineDialog>
  </main></div>
}
