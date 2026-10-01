import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useLocation, useSearchParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { GuardedLink } from '../../components/GuardedLink'
import { ApiError } from '../../lib/apiClient'
import { parseLineUrl, useLineRole } from './lineViewRules'
import { labels } from '../production-orders/messages'
import { listLines, retireLine, type LineSummary, type Page } from './api'
import { LineDialog, lineButtonClass as button, lineInputClass as input } from './LineDialog'

const w = labels.lines
/** URL-applied line search keeps input drafts separate and refuses malformed filters. */
export function ProductionLineListPage() {
  const allowed = useLineRole()
  const [params, setParams] = useSearchParams()
  const location = useLocation()
  const { q, state, page, valid } = parseLineUrl(params)
  const key = `${q}\u0000${state}`
  const [draft, setDraft] = useState<{ key: string; q: string; state: string } | null>(null)
  const current = draft?.key === key ? draft : { key, q, state }
  const [result, setResult] = useState<{ key: string; value: Page<LineSummary> } | null>(null)
  const [error, setError] = useState('')
  const [forbidden, setForbidden] = useState(false)
  const [refresh, setRefresh] = useState(0)
  const [target, setTarget] = useState<LineSummary | null>(null)
  const [pending, setPending] = useState(false)
  const [cooldown, setCooldown] = useState(false)
  const [unknown, setUnknown] = useState(false)
  const [notice, setNotice] = useState(() => location.state && typeof location.state === 'object' && 'lineSaved' in location.state ? w.saved : '')
  const heading = useRef<HTMLHeadingElement>(null)
  const requestKey = `${key}\u0000${page}\u0000${refresh}`
  const rows = result?.key === requestKey ? result.value : null
  useEffect(() => { document.title = labels.app.title(w.heading) }, [])
  useEffect(() => {
    if (!allowed || !valid || forbidden) return
    const controller = new AbortController()
    listLines(q, state, page, controller.signal).then(value => {
      if (!controller.signal.aborted) { setResult({ key: requestKey, value }); setError('') }
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      setResult(null)
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) setForbidden(true)
      setError(w.failed)
    })
    return () => controller.abort()
  }, [allowed, valid, forbidden, q, state, page, requestKey])
  function apply(event: FormEvent) {
    event.preventDefault()
    setParams({ q: current.q.trim(), state: current.state, page: '1' })
    setDraft(null)
    requestAnimationFrame(() => heading.current?.focus())
  }
  async function retire() {
    if (!target || pending || unknown || cooldown) return
    setPending(true)
    try {
      await retireLine(target.id, target.version)
      setTarget(null); setNotice(w.retiredDone); setRefresh(n => n + 1)
      requestAnimationFrame(() => heading.current?.focus())
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) { setForbidden(true); setTarget(null); return }
      if (reason instanceof ApiError && reason.status < 500) setNotice(reason.status === 409 ? w.conflict : w.invalid)
      else if (reason instanceof ApiError && reason.problem?.code === 'LINE_BUSY' && reason.status === 503) { setNotice(w.busy); setCooldown(true); setTimeout(() => setCooldown(false), 1000) }
      else { setUnknown(true); setNotice(w.unknown) }
    } finally { setPending(false) }
  }
  const actions = (line: LineSummary) => (<div className="flex flex-wrap gap-2"><GuardedLink className={button} aria-label={w.editNamed(line.code)} to={`/production-lines/${line.id}/edit`}>{w.editAction}</GuardedLink>
    {line.isActive && <button type="button" className={button} aria-label={w.retireNamed(line.code)} disabled={pending || unknown} onClick={() => { setTarget(line); setNotice('') }}>{w.retire}</button>}</div>)
  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-5xl gap-5 px-4 py-6 sm:px-6">
    <div className="flex flex-wrap items-center justify-between gap-3"><h1 className="text-2xl font-semibold">{w.heading}</h1>{allowed && !forbidden && <GuardedLink className={button} to="/production-lines/new">{w.new}</GuardedLink>}</div>
    {!allowed || forbidden ? <p role="alert">{w.forbidden}</p> : <>
      <form onSubmit={apply} className="grid gap-3 rounded border border-gray-300 p-4 sm:grid-cols-[1fr_10rem_auto] sm:items-end">
        <div><label htmlFor="line-search">{w.search}</label><input id="line-search" className={input} value={current.q} onChange={e => setDraft({ ...current, q: e.target.value })} /></div>
        <div><label htmlFor="line-state">{w.state}</label><select id="line-state" className={input} value={current.state} onChange={e => setDraft({ ...current, state: e.target.value })}><option value="active">{w.active}</option><option value="retired">{w.retired}</option><option value="all">{w.all}</option></select></div>
        <div className="flex gap-2"><button className={button} type="submit">{w.apply}</button><button type="button" className={button} onClick={() => { setParams({}); setDraft(null); heading.current?.focus() }}>{w.clear}</button></div>
      </form>
      {!valid && <p role="alert">{w.invalid}</p>}<p aria-live="polite">{target ? '' : notice}</p>
      {unknown && <button type="button" className={button} onClick={() => { setTarget(null); setRefresh(n => n + 1); setNotice(w.verified) }}>{w.verify}</button>}
      {error && <p role="alert">{error} <button type="button" className={button} onClick={() => setRefresh(n => n + 1)}>{labels.common.retry}</button></p>}
      <h2 ref={heading} tabIndex={-1} className="text-lg font-medium">{rows ? w.results(rows.total) : valid && !error ? w.loading : ''}</h2>
      {rows && (rows.items.length === 0 ? <p>{w.empty}</p> : <>
        <div className="hidden overflow-x-auto sm:block"><table className="w-full border-collapse text-left"><caption className="sr-only">{w.heading}</caption><thead><tr className="border-b border-gray-300">{[w.code, w.name, w.hours, w.state, w.actions].map(label => <th key={label} scope="col" className="p-2">{label}</th>)}</tr></thead><tbody>
          {rows.items.map(line => <tr key={line.id} className="border-b border-gray-200"><th scope="row" className="p-2 break-words">{line.code}</th><td className="p-2 break-words">{line.name}</td><td className="p-2">{line.workingHoursPerDay} {w.hoursUnit}</td><td className="p-2">{line.isActive ? w.active : w.retired}</td><td className="p-2">{actions(line)}</td></tr>)}
        </tbody></table></div>
        <ul className="grid gap-3 sm:hidden">{rows.items.map(line => <li key={line.id} className="grid gap-2 rounded border border-gray-300 p-4 break-words"><strong>{line.code} — {line.name}</strong><span>{w.hours}: {line.workingHoursPerDay} {w.hoursUnit}</span><span>{line.isActive ? w.active : w.retired}</span>{actions(line)}</li>)}</ul>
      </>)}
      {rows && <nav aria-label={w.pagination} className="flex flex-wrap justify-between gap-3"><button type="button" className={button} disabled={page <= 1} onClick={() => setParams({ q, state, page: String(page - 1) })}>{w.previous}</button><span>{w.page(page)}</span><button type="button" className={button} disabled={page >= 10000 || page * 50 >= rows.total} onClick={() => setParams({ q, state, page: String(page + 1) })}>{w.next}</button></nav>}
    </>}
    <LineDialog open={target !== null} title={w.retireQuestion} confirm={w.retire} busy={pending} confirmDisabled={unknown || cooldown} onCancel={() => setTarget(null)} onConfirm={() => void retire()}>{target?.code} — {target?.name}{target && <p aria-live="polite">{notice}</p>}</LineDialog>
  </main></div>
}
