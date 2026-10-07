import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useLocation, useSearchParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { GuardedLink } from '../../components/GuardedLink'
import { ApiError } from '../../lib/apiClient'
import { PageSizeSelect } from '../production-orders/ListPagination'
import { labels } from '../production-orders/messages'
import { listLines, retireLine, type LineSummary, type Page } from './api'
import { LineDialog } from './LineDialog'
import { parseLineUrl, useLineRole } from './lineViewRules'
import {
  badge, cell, dangerButton, errorBox, headerCell, infoBox, inputClass, labelClass, lineListHeight, pagerButton,
  primaryButton, scrollBox, secondaryButton, successBox,
} from './lineStyles'

const w = labels.lines

/** 005_DD-SPD-REDESIGN §4: URL-applied line search with 「表示件数」, scroll box and immediate line retirement. */
export function ProductionLineListPage() {
  const allowed = useLineRole()
  const [params, setParams] = useSearchParams()
  const location = useLocation()
  const { q, state, page, pageSize, valid, canonicalSize } = parseLineUrl(params)
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
  const [notice, setNotice] = useState<{ text: string; tone: 'success' | 'error' | 'info' } | null>(() =>
    location.state && typeof location.state === 'object' && 'lineSaved' in location.state ? { text: w.saved, tone: 'success' } : null)
  const resultHeading = useRef<HTMLParagraphElement>(null)
  const requestKey = `${key}\u0000${page}\u0000${pageSize}\u0000${refresh}`
  const rows = result?.key === requestKey ? result.value : null
  const pages = rows ? Math.max(1, Math.ceil(rows.total / rows.pageSize)) : 1
  const go = (next: Record<string, string>) => setParams({ ...next, pageSize: String(pageSize) })

  useEffect(() => { document.title = labels.app.title(w.heading) }, [])
  // Keep 「表示件数」 in the URL (DEC-006): a missing or unknown value is replaced by the default.
  useEffect(() => {
    if (!valid || canonicalSize) return
    const next = new URLSearchParams(params); next.set('pageSize', String(pageSize))
    setParams(next, { replace: true })
  }, [valid, canonicalSize, params, pageSize, setParams])
  useEffect(() => {
    if (!allowed || !valid || forbidden) return
    const controller = new AbortController()
    listLines(q, state, page, controller.signal, pageSize).then(value => {
      if (controller.signal.aborted) return
      // A page past the end moves to the last page with the same filters.
      if (value.items.length === 0 && value.total > 0 && page > 1) {
        setParams({ q, state, page: String(Math.min(10000, Math.ceil(value.total / value.pageSize))), pageSize: String(pageSize) }, { replace: true })
        return
      }
      setResult({ key: requestKey, value }); setError('')
    }).catch((reason: unknown) => {
      if (controller.signal.aborted) return
      setResult(null)
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) setForbidden(true)
      setError(w.failed)
    })
    return () => controller.abort()
  }, [allowed, valid, forbidden, q, state, page, pageSize, requestKey, setParams])

  function apply(event: FormEvent) {
    event.preventDefault()
    go({ q: current.q.trim(), state: current.state, page: '1' })
    setDraft(null)
    requestAnimationFrame(() => resultHeading.current?.focus())
  }
  async function retire() {
    if (!target || pending || unknown || cooldown) return
    setPending(true)
    try {
      await retireLine(target.id, target.version)
      setTarget(null); setNotice({ text: w.retiredDone, tone: 'success' }); setRefresh(n => n + 1)
      requestAnimationFrame(() => resultHeading.current?.focus())
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 401) return
      if (reason instanceof ApiError && reason.status === 403) { setForbidden(true); setTarget(null); return }
      if (reason instanceof ApiError && reason.status < 500) setNotice({ text: reason.status === 409 ? w.conflict : w.invalid, tone: 'error' })
      else if (reason instanceof ApiError && reason.problem?.code === 'LINE_BUSY' && reason.status === 503) { setNotice({ text: w.busy, tone: 'error' }); setCooldown(true); setTimeout(() => setCooldown(false), 1000) }
      else { setUnknown(true); setNotice({ text: w.unknown, tone: 'error' }) }
    } finally { setPending(false) }
  }

  const stateBadge = (line: LineSummary) => <span className={badge(line.isActive ? 'active' : 'retired')}>{line.isActive ? w.active : w.retired}</span>
  const actions = (line: LineSummary, wrap: boolean) => <div className={`flex gap-2.5 ${wrap ? 'flex-wrap' : 'flex-nowrap'}`}>
    <GuardedLink className={`${secondaryButton} ${wrap ? 'max-sm:flex-1' : ''}`} aria-label={w.editNamed(line.code)} to={`/production-lines/${line.id}/edit`}>{w.editAction}</GuardedLink>
    {line.isActive && <button type="button" className={`${dangerButton} ${wrap ? 'max-sm:flex-1' : ''}`} aria-label={w.retireNamed(line.code)} disabled={pending || unknown} onClick={() => { setTarget(line); setNotice(null) }}>{w.retire}</button>}
  </div>
  const noticeClass = (tone: 'success' | 'error' | 'info') => tone === 'success' ? successBox : tone === 'error' ? errorBox : infoBox
  const filtered = q !== '' || state === 'retired'

  return <div className="min-h-screen bg-white"><AppHeader /><main className="mx-auto grid max-w-5xl min-w-0 gap-4 px-4 py-6 text-[#172334] sm:px-6">
    <div className="flex flex-col items-stretch gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div className="min-w-0"><h1 className="text-2xl font-bold">{w.heading}</h1><p className="mt-1 text-sm text-[#485c74]">{w.description}</p></div>
      {allowed && !forbidden && <GuardedLink className={primaryButton} to="/production-lines/new">{w.new}</GuardedLink>}
    </div>
    {!allowed || forbidden ? <><p role="alert" className={errorBox}>{w.forbidden}</p><p><GuardedLink className={secondaryButton} to="/">{w.backToDashboard}</GuardedLink></p></> : <>
      <form onSubmit={apply} className="grid min-w-0 gap-3 sm:grid-cols-[minmax(0,1fr)_14rem_auto] sm:items-end">
        <div className="grid min-w-0 gap-1.5"><label htmlFor="line-search" className={labelClass}>{w.search}</label><input id="line-search" className={inputClass} value={current.q} onChange={e => setDraft({ ...current, q: e.target.value })} /></div>
        <div className="grid min-w-0 gap-1.5"><label htmlFor="line-state" className={labelClass}>{w.state}</label><select id="line-state" className={inputClass} value={current.state} onChange={e => setDraft({ ...current, state: e.target.value })}><option value="active">{w.active}</option><option value="retired">{w.retired}</option><option value="all">{w.all}</option></select></div>
        <div className="flex flex-col gap-2.5 sm:flex-row"><button className={primaryButton} type="submit">{w.apply}</button><button type="button" className={secondaryButton} onClick={() => { go({}); setDraft(null); resultHeading.current?.focus() }}>{w.clear}</button></div>
      </form>
      {!valid && <p role="alert" className={errorBox}>{w.invalid}</p>}
      {notice && !target && <p role={notice.tone === 'error' ? 'alert' : 'status'} className={noticeClass(notice.tone)}>{notice.text}</p>}
      {unknown && <p><button type="button" className={secondaryButton} onClick={() => { setTarget(null); setRefresh(n => n + 1); setNotice({ text: w.verified, tone: 'info' }) }}>{w.verify}</button></p>}
      {error && <div role="alert" className={`${errorBox} flex flex-wrap items-center gap-3`}><span>{error}</span><button type="button" className={secondaryButton} onClick={() => setRefresh(n => n + 1)}>{labels.common.retry}</button></div>}
      {valid && !error && !rows && <p role="status">{w.loading}</p>}
      {rows && (rows.total === 0 ? <div className={infoBox}><p ref={resultHeading} tabIndex={-1} className="focus:outline-none">{filtered ? w.noMatch : w.noLines}</p>{filtered && <p className="text-sm text-[#536475]">{w.noMatchHint}</p>}</div> : <>
        <div className="flex flex-wrap items-center justify-between gap-3">
          <p ref={resultHeading} tabIndex={-1} className="focus:outline-none">{w.count(rows.total)}</p>
          <PageSizeSelect pageSize={pageSize} onPageSize={size => setParams({ q, state, page: '1', pageSize: String(size) })} />
        </div>
        <div tabIndex={0} role="region" aria-label={w.caption} className={`${scrollBox} ${lineListHeight} max-sm:p-2.5`}>
          <table className="hidden w-full border-collapse sm:table"><caption className="sr-only">{w.caption}</caption>
            <thead><tr>{[w.code, w.name, w.hours, w.state, w.actions].map(label => <th key={label} scope="col" className={headerCell}>{label}</th>)}</tr></thead>
            <tbody>{rows.items.map(line => <tr key={line.id}>
              <th scope="row" className={`${cell} text-left font-normal [overflow-wrap:anywhere]`}>{line.code}</th>
              <td className={`${cell} [overflow-wrap:anywhere]`}>{line.name}</td>
              <td className={`${cell} whitespace-nowrap`}>{w.hoursValue(line.workingHoursPerDay)}</td>
              <td className={`${cell} whitespace-nowrap`}>{stateBadge(line)}</td>
              <td className={cell}>{actions(line, false)}</td>
            </tr>)}</tbody>
          </table>
          <ul className="grid gap-2.5 sm:hidden">{rows.items.map(line => <li key={line.id} aria-label={line.code} className="grid min-w-0 gap-2 rounded-lg border border-[#d5dde6] p-3">
            <strong className="[overflow-wrap:anywhere]">{line.code}　{line.name}</strong>
            <span className="flex flex-wrap items-center gap-2">{w.hoursPerDay(line.workingHoursPerDay)}{stateBadge(line)}</span>
            {actions(line, true)}
          </li>)}</ul>
        </div>
        {pages > 1 && <nav aria-label={w.pagination} className="flex items-center justify-end gap-2.5">
          <button type="button" className={pagerButton} disabled={page <= 1} onClick={() => go({ q, state, page: String(page - 1) })}>{w.previous}</button>
          <span className="tabular-nums">{w.pageOf(page, pages)}</span>
          <button type="button" className={pagerButton} disabled={page >= pages || page >= 10000} onClick={() => go({ q, state, page: String(page + 1) })}>{w.next}</button>
        </nav>}
      </>)}
    </>}
    <LineDialog open={target !== null} title={w.retireQuestion} confirm={w.retire} busy={pending} confirmDisabled={unknown || cooldown} onCancel={() => setTarget(null)} onConfirm={() => void retire()}>
      {target && <><p className="[overflow-wrap:anywhere]">{w.target(target.code, target.name)}</p><p>{w.retireLineNote}</p></>}
      {target && notice && <p aria-live="polite" className={noticeClass(notice.tone)}>{notice.text}</p>}
    </LineDialog>
  </main></div>
}
