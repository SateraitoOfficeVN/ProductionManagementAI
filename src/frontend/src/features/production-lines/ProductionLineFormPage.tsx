import { useEffect, useRef, useState, type FormEvent, type ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { GuardedLink } from '../../components/GuardedLink'
import { ApiError } from '../../lib/apiClient'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { labels } from '../production-orders/messages'
import { createLine, decimalValid, mutationWithinLimits, getLine, listLines, updateLine, type LineDetail, type PairDetail, type ProductChoice, type ProductChange, type TimingInput } from './api'
import { AddProductDialog } from './AddProductDialog'
import { LineDialog } from './LineDialog'
import { formPageSize, useLineRole } from './lineViewRules'
import {
  badge, cell, dangerButton, errorBox, fieldError, headerCell, hintClass, infoBox, inputClass, labelClass, pagerButton,
  pairListHeight, primaryButton, readOnlyClass, readOnlyInput, scrollBox, secondaryButton, sectionClass, warnBox,
} from './lineStyles'

const w = labels.lines
interface Row { product: ProductChoice; minutes: string; confirm: boolean; origin?: PairDetail; page: number; action?: 'add' | 'setTiming' | 'retire' }
interface Fields { code: string; name: string; hours: string }
type Focusable = HTMLInputElement | HTMLButtonElement

export function ProductionLineFormPage() { const { id } = useParams(); return <LineEditor key={id ?? 'new'} /> }

/** 005_DD-SPD-REDESIGN §5: 基本情報, 生産可能な製品 (20 per page, scroll box), add dialog and recovery states. */
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
  const [notFound, setNotFound] = useState(false)
  const [forbidden, setForbidden] = useState(false)
  const [pending, setPending] = useState(false)
  const [blocked, setBlocked] = useState<'conflict' | 'unknown' | null>(null)
  const [notice, setNotice] = useState('')
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [adding, setAdding] = useState(false)
  const [confirm, setConfirm] = useState<{ kind: 'leave'; to: string } | { kind: 'reload' } | { kind: 'pair'; id: string } | null>(null)
  const [verification, setVerification] = useState<LineDetail[] | null>(null)
  const [cooldown, setCooldown] = useState(false)
  const heading = useRef<HTMLHeadingElement>(null)
  const refs = useRef<Record<string, Focusable | null>>({})
  const version = useRef<string | null>(null)
  const pendingFocus = useRef<string | null>(null)
  const dirty = JSON.stringify(fields) !== JSON.stringify(baseline) || Object.values(rows).some(row => row.action !== undefined)
  const editable = allowed && !forbidden && (!id || loaded !== null)
  const title = id ? w.edit : w.create
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
    getLine(id, pairPage, controller.signal, formPageSize).then(line => {
      if (controller.signal.aborted) return
      if (version.current !== null && version.current !== line.version) { setBlocked('conflict'); setNotice(w.conflict); return }
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
      if (reason instanceof ApiError && reason.status === 404) setNotFound(true)
      else setReadError(w.failed)
    })
    return () => controller.abort()
  }, [id, allowed, forbidden, pairPage, reload])
  useEffect(() => {
    const key = pendingFocus.current
    if (key && refs.current[key]) { refs.current[key]?.focus(); pendingFocus.current = null }
  }, [rows, pageIds])

  function changeRow(rowId: string, patch: Partial<Row>) {
    setRows(previous => {
      const old = previous[rowId]
      if (!old) return previous
      // A new pair's unit was confirmed when it was chosen in the add dialog (DEC-003); a saved pair's confirmation
      // is cleared by any later coefficient change (SPD 4.3 step 4).
      const confirmed = old.action === 'add' ? true
        : patch.minutes !== undefined && patch.minutes !== old.minutes ? false : patch.confirm ?? old.confirm
      const next = { ...old, ...patch, confirm: confirmed }
      if (next.action !== 'add' && next.action !== 'retire') next.action = next.minutes !== next.origin?.minutesPerUnit || next.confirm ? 'setTiming' : undefined
      return { ...previous, [rowId]: next }
    })
  }
  function undoRetire(rowId: string) {
    setRows(previous => {
      const old = previous[rowId]
      if (!old) return previous
      return { ...previous, [rowId]: { ...old, action: old.minutes !== old.origin?.minutesPerUnit || old.confirm ? 'setTiming' : undefined } }
    })
  }
  function addProduct(product: ProductChoice) {
    setAdding(false)
    setRows(previous => ({ ...previous, [product.id]: { product, minutes: '', confirm: true, page: 1, action: 'add' } }))
    if (pairPage !== 1) { setPageIds([]); setPairPage(1) }
    pendingFocus.current = `row:${product.id}:minutes`
  }
  function reveal(key: string) {
    const match = /^row:([^:]+):/.exec(key)
    if (match && rows[match[1]] && rows[match[1]].page !== pairPage) setPairPage(rows[match[1]].page)
    pendingFocus.current = key
    requestAnimationFrame(() => { if (refs.current[key]) { refs.current[key]?.focus(); pendingFocus.current = null } })
  }
  function discardAndReload() {
    version.current = null; setLoaded(null); setRows({}); setPageIds([]); setPairPage(1)
    setErrors({}); setBlocked(null); setVerification(null); setNotice(''); setReadError(''); setReload(n => n + 1)
    if (!id) { setFields({ code: '', name: '', hours: '' }); setBaseline({ code: '', name: '', hours: '' }) }
  }
  async function verify() {
    try {
      if (id) setVerification([await getLine(id, 1, undefined, formPageSize)])
      else {
        const result = await listLines(fields.code.trim(), 'all', 1)
        const exact = result.items.filter(line => line.code.toLocaleLowerCase() === fields.code.trim().toLocaleLowerCase())
        setVerification(await Promise.all(exact.map(line => getLine(line.id, 1, undefined, formPageSize))))
      }
      setNotice(w.verified)
    } catch { setNotice(w.failed) }
  }
  async function save(event: FormEvent) {
    event.preventDefault()
    if (!editable || pending || blocked || cooldown || !dirty) return
    const next: Record<string, string> = {}
    const code = fields.code.trim(); const name = fields.name.trim()
    if (!id && !code) next.code = w.codeRequired
    else if (!id && [...code].length > 50) next.code = w.codeTooLong
    if (!name) next.name = w.nameRequired
    else if ([...name].length > 200) next.name = w.nameTooLong
    if (!decimalValid(fields.hours, '24')) next.hours = w.hoursInvalid
    const changed = Object.entries(rows).filter(([, row]) => row.action)
    for (const [key, row] of changed) if (row.action !== 'retire') {
      if (!decimalValid(row.minutes, '999999999.999')) next[`row:${key}:minutes`] = w.minutesInvalid
      if ((row.action === 'add' || row.origin?.requiresUnitConfirmation) && !row.confirm) next[`row:${key}:confirm`] = w.confirmRequired
    }
    const timing = (row: Row): TimingInput => ({ productId: row.product.id, minutesPerUnit: row.minutes,
      expectedUnit: row.product.unit, expectedUnitRevision: row.product.unitRevision, confirmUnit: row.confirm })
    const changes: ProductChange[] = changed.map(([, row]) => row.action === 'retire' ? { action: 'retire', productId: row.product.id }
      : { action: row.action === 'add' ? 'add' : 'setTiming', ...timing(row) })
    const createBody = { code, name, workingHoursPerDay: fields.hours, products: changed.map(([, row]) => timing(row)) }
    const updateBody = { name, workingHoursPerDay: fields.hours, version: loaded?.version ?? '', productChanges: changes }
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
        const codeConflict = reason.problem?.code === 'LINE_CODE_CONFLICT'
        if (reason.status === 409 && !codeConflict) { setBlocked('conflict'); setNotice(w.conflict) }
        else setNotice(codeConflict ? w.codeConflict : w.invalid)
        const mapped: Record<string, string> = {}
        for (const field of Object.keys(reason.problem?.errors ?? {})) {
          const match = /^(?:products|productChanges)\[([0-9]+)\]\.(minutesPerUnit|expectedUnit|expectedUnitRevision|confirmUnit|productId|action)$/.exec(field)
          const row = match ? changed[Number(match[1])] : undefined
          if (row && match) {
            const minutes = match[2] === 'minutesPerUnit'
            mapped[`row:${row[0]}:${minutes ? 'minutes' : 'confirm'}`] = minutes ? w.minutesInvalid : reason.status === 409 ? w.unitChanged : w.confirmRequired
          } else {
            const key = field === 'workingHoursPerDay' ? 'hours' : ['name', 'code'].includes(field) ? field : 'body'
            mapped[key] = codeConflict ? w.codeConflict : key === 'hours' ? w.hoursInvalid : w.invalid
          }
        }
        setErrors(mapped); if (Object.keys(mapped).length) reveal(Object.keys(mapped)[0])
      } else { setBlocked('unknown'); setNotice(w.unknown) }
    } finally { setPending(false) }
  }

  const added = Object.keys(rows).filter(key => rows[key].action === 'add')
  const visible = [...(pairPage === 1 ? added : []), ...pageIds.filter(key => !added.includes(key))]
  const pages = Math.max(1, Math.ceil(total / formPageSize))
  const errorLabel = (key: string) => {
    if (key === 'code' || key === 'name' || key === 'hours') return { code: w.code, name: w.name, hours: w.hours }[key]
    const row = rows[key.split(':')[1]]
    return row ? `${row.product.sku} ${key.endsWith(':confirm') ? w.unit : w.timingColumn}` : w.invalid
  }
  const describedBy = (...ids: (string | false | undefined)[]) => ids.filter(Boolean).join(' ') || undefined
  const required = <span aria-hidden="true" className="ml-1 text-[#a12424]">{w.required}</span>
  const textField = (key: 'code' | 'name', label: string, hint?: string) => <div className="grid min-w-0 content-start gap-1.5">
    <div className="flex"><label htmlFor={`line-${key}`} className={labelClass}>{label}</label>{required}</div>
    {key === 'code' && id ? <div className={readOnlyClass}><input id="line-code" readOnly className={readOnlyInput} value={fields.code} />
      <span aria-hidden="true" className="shrink-0 text-sm">{w.readOnly}</span></div>
      : <input id={`line-${key}`} ref={element => { refs.current[key] = element }} className={inputClass} value={fields[key]} disabled={pending} aria-required="true"
        aria-invalid={Boolean(errors[key])} aria-describedby={describedBy(errors[key] && `line-${key}-error`, hint && `line-${key}-hint`)}
        onChange={e => setFields(old => ({ ...old, [key]: e.target.value }))} />}
    {errors[key] && <p id={`line-${key}-error`} className={fieldError}>{errors[key]}</p>}
    {hint && <p id={`line-${key}-hint`} className={hintClass}>{hint}</p>}
  </div>

  function pairBody(key: string) {
    const row = rows[key]
    if (!row) return null
    const retiredPair = row.origin?.isActive === false
    const locked = pending || row.action === 'retire' || retiredPair
    const stale = row.origin?.requiresUnitConfirmation === true && !retiredPair && row.action !== 'retire' && row.action !== 'add'
    const minutesError = errors[`row:${key}:minutes`]; const confirmError = errors[`row:${key}:confirm`]
    const state = row.action === 'add' ? <span className={badge('added')}>{w.toBeAdded}</span>
      : row.action === 'retire' ? <span className={badge('pending')}>{w.pendingRetire}</span>
        : retiredPair ? <span className={badge('retired')}>{w.retired}</span> : <span className={badge('active')}>{w.active}</span>
    const action = row.action === 'add' ? <button type="button" className={secondaryButton} disabled={pending} onClick={() => setRows(previous => { const next = { ...previous }; delete next[key]; return next })}>{w.remove}</button>
      : row.action === 'retire' ? <button type="button" className={secondaryButton} disabled={pending} onClick={() => undoRetire(key)}>{w.undo}</button>
        : retiredPair ? <span className="text-sm text-[#536475]">{w.noReAdd}</span>
          : <button type="button" className={dangerButton} disabled={pending} aria-label={w.retireNamed(row.product.sku)} onClick={() => setConfirm({ kind: 'pair', id: key })}>{w.retire}</button>
    let note: ReactNode = null
    if (stale) note = <div className={`${warnBox} grid gap-1`}><strong>{w.needsConfirmation}</strong>
      <span>{w.staleDetail(row.origin?.confirmedUnit ?? '', row.product.unit)}</span><span>{w.staleHelp(row.product.unit)}</span>
      {row.confirm ? <span className="flex flex-wrap items-center gap-2.5">{w.confirmedFor(row.product.unit)}<button type="button" className={secondaryButton} disabled={pending} onClick={() => changeRow(key, { confirm: false })}>{w.undo}</button></span>
        : <span><button type="button" ref={element => { refs.current[`row:${key}:confirm`] = element }} className={`${secondaryButton} scroll-mt-14`} disabled={pending}
          aria-describedby={confirmError ? `confirm-${key}-error` : undefined} onClick={() => changeRow(key, { confirm: true })}>{w.confirmUnit}</button></span>}
      {confirmError && <p id={`confirm-${key}-error`} className={fieldError}>{confirmError}</p>}</div>
    else if (confirmError) note = <div className={warnBox}><button type="button" ref={element => { refs.current[`row:${key}:confirm`] = element }} className="font-bold underline" onClick={() => setRows(previous => { const next = { ...previous }; delete next[key]; return next })}>{w.remove}</button>
      <p id={`confirm-${key}-error`} className={fieldError}>{confirmError}</p></div>
    return (<tbody key={key} className="max-sm:grid max-sm:gap-2 max-sm:rounded-lg max-sm:border max-sm:border-[#d5dde6] max-sm:p-3">
      <tr className="max-sm:flex max-sm:flex-wrap max-sm:items-center max-sm:gap-x-3 max-sm:gap-y-2">
        <td className={`${cell} max-sm:basis-full max-sm:border-0 max-sm:p-0`}><span className="grid min-w-0"><strong className="[overflow-wrap:anywhere]">{row.product.sku}</strong><span className="[overflow-wrap:anywhere]">{row.product.name}</span>
          {!row.product.isActive && <span className="text-sm text-[#536475]">{w.productRetired}</span>}</span></td>
        <td className={`${cell} whitespace-nowrap max-sm:order-3 max-sm:border-0 max-sm:p-0`}><span className="sm:hidden">{w.unitOf(row.product.unit)}</span><span className="max-sm:hidden">{row.product.unit}</span></td>
        <td className={`${cell} max-sm:order-4 max-sm:basis-full max-sm:border-0 max-sm:p-0`}>
          <span aria-hidden="true" className={`${labelClass} mb-1 sm:hidden`}>{w.timingColumn}</span>
          {locked ? <span>{row.minutes}</span> : <input id={`timing-${key}`} ref={element => { refs.current[`row:${key}:minutes`] = element }} className={`${inputClass} max-w-40 scroll-mt-14`} inputMode="decimal" value={row.minutes}
            aria-label={w.timingName(row.product.sku, row.product.unit)} aria-invalid={Boolean(minutesError)}
            aria-describedby={describedBy(minutesError && `timing-${key}-error`, `timing-${key}-hint`)} onChange={e => changeRow(key, { minutes: e.target.value })} />}
          {minutesError && <p id={`timing-${key}-error`} className={fieldError}>{minutesError}</p>}
          <p id={`timing-${key}-hint`} className={hintClass}>{w.perUnit(row.product.unit)}</p>
        </td>
        <td className={`${cell} whitespace-nowrap max-sm:order-2 max-sm:border-0 max-sm:p-0`}>{state}</td>
        <td className={`${cell} whitespace-nowrap max-sm:order-5 max-sm:basis-full max-sm:border-0 max-sm:p-0 max-sm:[&>button]:w-full`}>{action}</td>
      </tr>
      {note && <tr className="max-sm:block"><td colSpan={5} className={`${cell} max-sm:border-0 max-sm:p-0`}>{note}</td></tr>}
    </tbody>)
  }

  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-5xl min-w-0 gap-4 px-4 py-6 text-[#172334] sm:px-6">
    <h1 ref={heading} tabIndex={-1} className="text-2xl font-bold focus:outline-none">{title}</h1>
    {!allowed || forbidden ? <><p role="alert" className={errorBox}>{w.forbidden}</p><p><GuardedLink className={secondaryButton} to="/">{w.backToDashboard}</GuardedLink></p></> : <>
      {notFound && <><p role="alert" className={errorBox}>{w.notFound}</p><p><GuardedLink className={secondaryButton} to="/production-lines">{w.backToList}</GuardedLink></p></>}
      {readError && <div role="alert" className={`${errorBox} flex flex-wrap items-center gap-3`}><span>{readError}</span><button type="button" className={secondaryButton} onClick={() => { setReadError(''); setReload(n => n + 1) }}>{labels.common.retry}</button></div>}
      {!editable && !readError && !notFound && <p role="status">{w.loading}</p>}
      {editable && <form onSubmit={event => void save(event)} className="grid min-w-0 gap-4" noValidate>
        {loaded && !loaded.isActive && <p className={warnBox}>{w.retiredLine}</p>}
        {Object.keys(errors).length > 0 && <div role="alert" className={errorBox}><p className="font-bold">{w.summary}</p>
          <ul className="mt-1 list-disc pl-5">{Object.keys(errors).map(key => <li key={key}><button type="button" className="text-left underline" onClick={() => reveal(key)}>{errorLabel(key)}: {errors[key]}</button></li>)}</ul></div>}
        {blocked === 'conflict' && <div role="alert" className={`${errorBox} grid gap-2`}><strong>{w.conflict}</strong><span>{w.conflictDetail}</span>
          <span><button type="button" className={secondaryButton} onClick={() => setConfirm({ kind: 'reload' })}>{w.reload}</button></span></div>}
        {blocked === 'unknown' && <div role="alert" className={`${errorBox} grid gap-2`}><strong>{w.unknown}</strong>
          <span className="flex flex-wrap gap-2.5"><button type="button" className={secondaryButton} onClick={() => void verify()}>{w.verify}</button><button type="button" className={secondaryButton} onClick={() => setConfirm({ kind: 'reload' })}>{w.reload}</button></span></div>}
        {notice && notice !== w.conflict && notice !== w.unknown && <p aria-live="polite" className={notice === w.verified ? infoBox : errorBox}>{notice}</p>}
        {verification && <section aria-label={w.verify} className={`${infoBox} grid gap-1`}>{verification.map(line => <p key={line.id} className="[overflow-wrap:anywhere]">{line.code} — {line.name} — {w.hoursValue(line.workingHoursPerDay)} — {line.isActive ? w.active : w.retired}</p>)}</section>}

        <section aria-labelledby="line-basic-title" className={sectionClass}>
          <h2 id="line-basic-title" className="text-lg font-bold">{w.basic}</h2>
          <div className="grid min-w-0 gap-4 sm:grid-cols-2">
            {textField('code', w.code, id ? undefined : w.codeHint)}
            {textField('name', w.name)}
            <div className="grid min-w-0 content-start gap-1.5">
              <div className="flex"><label htmlFor="line-hours" className={labelClass}>{w.hours}</label>{required}</div>
              <div className="flex items-center gap-2"><input id="line-hours" ref={element => { refs.current.hours = element }} className={`${inputClass} max-w-56`} value={fields.hours} disabled={pending}
                inputMode="decimal" aria-required="true" aria-invalid={Boolean(errors.hours)} aria-describedby={describedBy(errors.hours && 'line-hours-error', 'line-hours-hint')}
                onChange={e => setFields(old => ({ ...old, hours: e.target.value }))} /><span aria-hidden="true">{w.hoursUnit}</span></div>
              {errors.hours && <p id="line-hours-error" className={fieldError}>{errors.hours}</p>}
              <p id="line-hours-hint" className={hintClass}>{w.hoursHint}</p>
            </div>
          </div>
        </section>

        <section className={sectionClass}>
          <div className="flex flex-col items-stretch gap-2.5 sm:flex-row sm:items-start sm:justify-between">
            <div><h2 id="line-products-title" className="text-lg font-bold">{w.products}</h2>
              {(total > 0 || added.length > 0) && <p className="text-sm text-[#536475]">{w.count(total + added.length)}</p>}</div>
            <button type="button" className={secondaryButton} disabled={pending || Boolean(blocked)} onClick={() => setAdding(true)}>{w.add}</button>
          </div>
          {visible.length === 0 ? <p>{w.noProducts}</p> :
            <div tabIndex={0} role="region" aria-label={w.products} className={`${scrollBox} ${pairListHeight} max-sm:p-2.5`}>
              <table className="w-full border-collapse max-sm:grid max-sm:gap-2.5">
                <caption className="sr-only">{w.products}</caption>
                <thead className="max-sm:hidden"><tr>{[w.product, w.unit, w.timingColumn, w.state, w.actions].map(label => <th key={label} scope="col" className={headerCell}>{label}</th>)}</tr></thead>
                {visible.map(pairBody)}
              </table>
            </div>}
          {id && pages > 1 && <nav aria-label={w.pairPagination} className="flex items-center justify-end gap-2.5">
            <button type="button" disabled={pending || pairPage <= 1} className={pagerButton} onClick={() => { setPageIds([]); setPairPage(n => n - 1) }}>{w.previous}</button>
            <span className="tabular-nums">{w.pageOf(pairPage, pages)}</span>
            <button type="button" disabled={pending || pairPage >= pages || pairPage >= 10000} className={pagerButton} onClick={() => { setPageIds([]); setPairPage(n => n + 1) }}>{w.next}</button></nav>}
        </section>

        <div className="flex flex-col gap-2.5 sm:flex-row">
          <button type="submit" className={primaryButton} disabled={!dirty || pending || Boolean(blocked) || cooldown}>{pending ? w.saving : w.save}</button>
          <GuardedLink className={secondaryButton} to="/production-lines">{w.cancel}</GuardedLink>
        </div>
      </form>}
    </>}
    <AddProductDialog open={adding} lineId={id} excluded={new Set(Object.keys(rows))} onAdd={addProduct} onClose={() => setAdding(false)} onForbidden={() => { setAdding(false); setForbidden(true) }} />
    <LineDialog open={confirm !== null} title={confirm?.kind === 'pair' ? w.pairQuestion : w.discardQuestion} confirm={confirm?.kind === 'pair' ? w.retire : w.discard}
      cancelLabel={confirm?.kind === 'pair' ? w.cancel : w.keep} busy={pending} onCancel={() => setConfirm(null)} onConfirm={() => {
        if (confirm?.kind === 'pair') changeRow(confirm.id, { action: 'retire' })
        else if (confirm?.kind === 'reload') discardAndReload()
        else if (confirm?.kind === 'leave') navigate(confirm.to)
        setConfirm(null)
      }}>
      {confirm?.kind === 'pair' && rows[confirm.id] ? <><p className="[overflow-wrap:anywhere]">{w.target(rows[confirm.id].product.sku, rows[confirm.id].product.name)}</p><p>{w.retirePairNote}</p><p className="font-bold">{w.retirePairPermanent}</p></> :<p>{w.discardDescription}</p>}
    </LineDialog>
  </main></div>
}
