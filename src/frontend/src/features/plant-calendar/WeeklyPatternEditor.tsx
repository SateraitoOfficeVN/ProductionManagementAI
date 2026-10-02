import { useEffect,useRef,useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { labels } from '../production-orders/messages'
import { LineDialog,lineButtonClass as button,lineInputClass as input } from '../production-lines/LineDialog'
import { CalendarWriteError,getWeekly,saveWeekly,withdrawWeekly,type WeeklyPage } from './api'
import { bodyWithinLimit,dateValid,weekdays,type Weekday } from './values'
import { authFailure,useCalendarRead } from './useCalendarRead'
import { errorText,monthEnd } from './viewRules'
const w=labels.calendar
/** Shows bounded retained definitions and initializes each edit from an exact one-date read. */
export function WeeklyPatternEditor({month,date,onSelect,onSaved,onCancel,onAuth}:{month:string;date:string;onSelect:(date:string)=>void;onSaved:(changed:boolean)=>void;onCancel:()=>void;onAuth:()=>void}) {
  const [range,setRange]=useState({from:month+'-01',to:monthEnd(month)}),[draftRange,setDraftRange]=useState(range)
  const [view,setView]=useState<'Current'|'History'>('Current'),[page,setPage]=useState(1),[token,setToken]=useState<string|undefined>(),[generation,setGeneration]=useState(0),[listGeneration,setListGeneration]=useState(0),[rangeError,setRangeError]=useState(false)
  const list=useCalendarRead(`weekly-list:${JSON.stringify(range)}:${view}:${page}:${token}:${generation}:${listGeneration}`,s=>getWeekly(range.from,range.to,view,page,token,s))
  const exact=useCalendarRead(`weekly-edit:${date}:${generation}`,s=>getWeekly(date,date,'Current',1,undefined,s))
  useEffect(()=>{for(const r of [list,exact])if(r.status==='error'&&authFailure(r.error))onAuth()},[list,exact,onAuth])
  return <section>
    <h2 className="text-xl font-semibold">{w.weekly}</h2>
    <form className="grid gap-3 sm:grid-cols-3" onSubmit={e=>{e.preventDefault();if(!dateValid(draftRange.from)||!dateValid(draftRange.to)||draftRange.from>draftRange.to){setRangeError(true);return}setRangeError(false);setRange(draftRange);setPage(1);setToken(undefined)}}><label>{w.from}<input type="date" className={input} value={draftRange.from} onChange={e=>setDraftRange(r=>({...r,from:e.target.value}))}/></label><label>{w.to}<input type="date" className={input} value={draftRange.to} onChange={e=>setDraftRange(r=>({...r,to:e.target.value}))}/></label><button className={button}>{w.apply}</button></form>
    {rangeError&&<p role="alert">{w.validation}</p>}
    <label>{w.history}<select className={input} value={view} onChange={e=>{if(e.target.value==='Current'||e.target.value==='History'){setView(e.target.value);setPage(1);setToken(undefined)}}}><option value="Current">{w.current}</option><option value="History">{w.historyView}</option></select></label>
    <div aria-live="polite" className="my-3">{list.status==='loading'?w.loading:list.status==='error'?errorText(list.error):<>
      {list.value.applicableBeforeFrom&&<p>{w.fallback}: {list.value.applicableBeforeFrom.effectiveFrom} {list.value.applicableBeforeFrom.workingDays?.map(day=>w.weekdays[day]).join('・')}</p>}
      <ol className="space-y-2">{list.value.items.map(row=><li key={row.id} className="rounded border border-gray-300 p-3"><p>{row.effectiveFrom} — {row.isWithdrawn?w.historyMarker:row.workingDays?.map(day=>w.weekdays[day]).join('・')||w.closed}</p><p>{row.isCurrent?w.current:w.retained} / {row.commitRevision} / {row.createdAtUtc}</p>{view==='Current'&&<button className={button} disabled={row.effectiveFrom<list.value.context.plantToday} onClick={()=>onSelect(row.effectiveFrom)}>{w.edit}</button>}</li>)}</ol>
      <div className="flex flex-wrap gap-2"><button className={button} disabled={page===1} onClick={()=>{setToken(list.value.snapshotVersion??undefined);setPage(n=>n-1)}}>{w.previousPage}</button><button className={button} disabled={page>=list.value.totalPages} onClick={()=>{setToken(list.value.snapshotVersion??undefined);setPage(n=>n+1)}}>{w.nextPage}</button></div>
    </>}<button className={button} onClick={()=>{setPage(1);setToken(undefined);setListGeneration(n=>n+1)}}>{w.restart}</button></div>
    {exact.status==='loading'?<p>{w.loading}</p>:exact.status==='error'?<p role="alert">{errorText(exact.error)}</p>:<WeeklyDraft key={date+':'+generation} date={date} baseline={exact.value} onSelect={onSelect} onSaved={onSaved} onCancel={onCancel} onAuth={onAuth} onReload={()=>setGeneration(n=>n+1)}/>}
  </section>
}
function WeeklyDraft({date,baseline,onSelect,onSaved,onCancel,onAuth,onReload}:{date:string;baseline:WeeklyPage;onSelect:(date:string)=>void;onSaved:(changed:boolean)=>void;onCancel:()=>void;onAuth:()=>void;onReload:()=>void}) {
  const current=baseline.items.find(row=>row.effectiveFrom===date)??null
  const initial=current&&!current.isWithdrawn?current.workingDays??[]:baseline.applicableBeforeFrom?.workingDays??weekdays.slice(0,5)
  const [days,setDays]=useState<Weekday[]>([...initial]),[state,setState]=useState<'editing'|'saving'|'conflict'|'unknown'|'restricted'>('editing'),[notice,setNotice]=useState('')
  const [confirm,setConfirm]=useState<{kind:'withdraw'|'reload'|'cancel'|'accept'}|{kind:'leave';to:string}|null>(null)
  const [observation,setObservation]=useState<{current:WeeklyPage;history:WeeklyPage}|null>(null),[verifying,setVerifying]=useState(false)
  const guard=useNavigationGuard(),navigate=useNavigate(),heading=useRef<HTMLHeadingElement>(null),summary=useRef<HTMLParagraphElement>(null),mounted=useRef(true),pending=useRef(false),readController=useRef<AbortController|null>(null)
  const newIntent=!current||current.isWithdrawn,dirty=newIntent||JSON.stringify(days)!==JSON.stringify(initial),guarded=dirty||state==='unknown'||state==='conflict'
  const editable=baseline.context.version!==null&&date>=baseline.context.plantToday&&date>=(baseline.context.activatedOn??date)
  const withdrawable=editable&&current&&!current.isWithdrawn&&date>baseline.context.plantToday&&date!==baseline.context.activatedOn
  useEffect(()=>{mounted.current=true;heading.current?.focus();return()=>{mounted.current=false;readController.current?.abort()}},[])
  useEffect(()=>guard.register(to=>{if(state==='saving')return true;if(!guarded)return false;setConfirm({kind:'leave',to});return true}),[guard,state,guarded])
  useEffect(()=>{if(!guarded&&state!=='saving')return;const warn=(e:BeforeUnloadEvent)=>{e.preventDefault();e.returnValue=''};window.addEventListener('beforeunload',warn);return()=>window.removeEventListener('beforeunload',warn)},[guarded,state])
  async function perform(withdraw=false){
    if(pending.current||state!=='editing'||!editable||!baseline.context.version||withdraw&&!withdrawable||!withdraw&&!dirty)return
    const body={version:baseline.context.version,targetRevisionId:current?.id??null,workingDays:days}
    if(!bodyWithinLimit(body)){setNotice(w.requestError);return}
    pending.current=true;setState('saving');setNotice(w.saving)
    try{const result=withdraw&&current?await withdrawWeekly(date,{version:baseline.context.version,targetRevisionId:current.id}):await saveWeekly(date,body);if(mounted.current)onSaved(result.changed)}
    catch(error){if(!mounted.current)return;if(authFailure(error)){onAuth();return}setNotice(errorText(error));setState(error instanceof CalendarWriteError&&error.outcome==='Unknown'?'unknown':error instanceof CalendarWriteError&&['CALENDAR_STALE','CALENDAR_TARGET_CHANGED'].includes(error.code)?'conflict':error instanceof CalendarWriteError&&['CALENDAR_PAST_DATE','CALENDAR_LINE_RETIRED','CALENDAR_NOT_ACTIVATED','CALENDAR_TIMEZONE_MISMATCH','CALENDAR_PROTECTED_RULE'].includes(error.code)?'restricted':'editing');if(error instanceof CalendarWriteError&&error.retryAfter)await new Promise(r=>window.setTimeout(r,error.retryAfter*1000));summary.current?.focus()}
    finally{pending.current=false}
  }
  async function verify(){
    if(verifying)return
    const controller=new AbortController();readController.current=controller;setVerifying(true);setObservation(null)
    try{const [current,history]=await Promise.all([getWeekly(date,date,'Current',1,undefined,controller.signal),getWeekly(date,date,'History',1,undefined,controller.signal)]);if(mounted.current&&!controller.signal.aborted){setObservation({current,history});setNotice(w.observation)}}
    catch(error){if(mounted.current&&!controller.signal.aborted){if(authFailure(error))onAuth();else setNotice(errorText(error))}}
    finally{if(mounted.current)setVerifying(false)}
  }
  return <div className="rounded border border-gray-300 p-4" aria-busy={state==='saving'}><h3 ref={heading} tabIndex={-1} className="text-lg font-semibold">{w.edit}: {date}</h3>
    {notice&&<p ref={summary} tabIndex={-1} role={state==='unknown'||state==='conflict'?'alert':'status'} className="my-3">{notice}</p>}{!editable&&<p>{baseline.context.version===null?w.notActivated:w.pastLocked}</p>}
    <label>{w.effectiveFrom}<input className={input} type="date" value={date} readOnly={!newIntent} disabled={state==='saving'} min={baseline.context.plantToday} onChange={e=>{if(dateValid(e.target.value))onSelect(e.target.value)}}/></label>
    <fieldset className="my-4" disabled={!editable||state!=='editing'}><legend>{w.working}</legend><div className="flex flex-wrap gap-4">{weekdays.map(day=><label className="flex min-h-12 items-center gap-2" key={day}><input type="checkbox" checked={days.includes(day)} onChange={e=>setDays(previous=>weekdays.filter(d=>d===day?e.target.checked:previous.includes(d)))}/>{w.weekdays[day]}</label>)}</div></fieldset>
    <div className="flex flex-wrap gap-2"><button className={`${button} bg-orange-700 text-white`} disabled={!editable||state!=='editing'||!dirty} onClick={()=>void perform()}>{state==='saving'?w.saving:w.save}</button><button className={button} disabled={state==='saving'} onClick={()=>guarded?setConfirm({kind:'cancel'}):onCancel()}>{w.cancel}</button><button className={button} disabled={!withdrawable||state!=='editing'} onClick={()=>setConfirm({kind:'withdraw'})}>{w.withdraw}</button></div>
    {(state==='conflict'||state==='restricted')&&<button className={button} onClick={()=>setConfirm({kind:'reload'})}>{w.reload}</button>}
    {state==='unknown'&&<><button className={button} disabled={verifying} onClick={()=>void verify()}>{verifying?w.loading:w.verify}</button><button className={button} disabled={!observation||verifying} onClick={()=>setConfirm({kind:'accept'})}>{w.acceptCurrent}</button>{observation&&<div><p>{w.observation}</p><p>{w.current}: {observation.current.context.version} / {w.history}: {observation.history.context.version}</p>{observation.current.items.map(row=><p key={row.id}>{row.effectiveFrom} {row.isWithdrawn?w.historyMarker:row.workingDays?.map(day=>w.weekdays[day]).join('・')}</p>)}</div>}</>}
    <LineDialog open={confirm!==null} title={confirm?.kind==='withdraw'?w.withdrawTitle:w.discardTitle} confirm={confirm?.kind==='withdraw'?w.withdraw:w.discard} onCancel={()=>setConfirm(null)} onConfirm={()=>{const action=confirm;setConfirm(null);if(!action)return;if(action.kind==='withdraw')void perform(true);else if(action.kind==='leave')void navigate(action.to);else if(action.kind==='cancel')onCancel();else if(action.kind!=='accept'||observation)onReload()}}>{confirm?.kind==='withdraw'?w.removeBody:w.discardBody}</LineDialog>
  </div>
}
