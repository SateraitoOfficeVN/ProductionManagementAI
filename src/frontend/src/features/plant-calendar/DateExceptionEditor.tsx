import { useEffect,useRef,useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { labels } from '../production-orders/messages'
import { LineDialog,lineButtonClass as button,lineInputClass as input } from '../production-lines/LineDialog'
import { CalendarWriteError,getDay,getExceptionHistory,saveException,removeException,type DayResult,type ExceptionPage,type ExceptionSave } from './api'
import { bodyWithinLimit,hoursValid,normalizeText,trimText } from './values'
import { authFailure,useCalendarRead } from './useCalendarRead'
import { errorText } from './viewRules'

const w=labels.calendar
interface Draft { working:''|'yes'|'no';inherit:boolean;hours:string;reason:string }
const initial=(v:DayResult):Draft=>v.exception && !v.exception.isRemoved ? {working:v.exception.isWorking?'yes':'no',inherit:v.exception.workingHours===null,hours:v.exception.workingHours??'',reason:v.exception.reason??''} : {working:'',inherit:true,hours:'',reason:''}
/** Edits the exact date/scope head; winning fallback IDs are never used as save targets. */
export function DateExceptionEditor({date,lineId,initialHistory=false,onSaved,onCancel,onAuth}:{date:string;lineId?:string;initialHistory?:boolean;onSaved:(changed:boolean)=>void;onCancel:()=>void;onAuth:()=>void}) {
  const [generation,setGeneration]=useState(0)
  const read=useCalendarRead(`exception:${date}:${lineId}:${generation}`,s=>getDay(date,lineId,s))
  useEffect(()=>{if(read.status==='error'&&authFailure(read.error))onAuth()},[read,onAuth])
  if(read.status==='loading')return <p aria-live="polite">{w.loading}</p>
  if(read.status==='error')return <div className="grid gap-3"><p role="alert">{errorText(read.error)}</p><div className="flex flex-wrap items-start gap-2"><button className={button} onClick={()=>setGeneration(n=>n+1)}>{w.reload}</button><button className={button} onClick={onCancel}>{w.cancel}</button></div></div>;
  return <ExceptionDraft key={generation} baseline={read.value} initialHistory={initialHistory} onSaved={onSaved} onCancel={onCancel} onAuth={onAuth} onReload={()=>setGeneration(n=>n+1)}/>
}
function ExceptionDraft({baseline,initialHistory,onSaved,onCancel,onAuth,onReload}:{baseline:DayResult;initialHistory:boolean;onSaved:(changed:boolean)=>void;onCancel:()=>void;onAuth:()=>void;onReload:()=>void}) {
  const navigate=useNavigate(),guard=useNavigationGuard()
  const [draft,setDraft]=useState(()=>initial(baseline))
  const [state,setState]=useState<'editing'|'saving'|'conflict'|'unknown'|'restricted'>('editing')
  const [notice,setNotice]=useState(''),[errors,setErrors]=useState<Record<string,string>>({})
  const [confirm,setConfirm]=useState<{kind:'remove'|'reload'|'cancel'|'accept'}|{kind:'leave';to:string}|null>(null)
  const [observation,setObservation]=useState<{day:DayResult;history:ExceptionPage}|null>(null)
  const [verifyPending,setVerifyPending]=useState(false)
  const [historyPage,setHistoryPage]=useState(1),[historyToken,setHistoryToken]=useState<string|undefined>(),[historyOpen,setHistoryOpen]=useState(initialHistory),[historyGeneration,setHistoryGeneration]=useState(0)
  const date=baseline.day.date,lineId=baseline.scope.lineId??undefined
  const history=useCalendarRead(historyOpen?`history:${date}:${lineId}:${historyPage}:${historyToken}:${historyGeneration}`:null,s=>getExceptionHistory(date,lineId,historyPage,historyToken,s))
  const heading=useRef<HTMLHeadingElement>(null),summary=useRef<HTMLParagraphElement>(null),working=useRef<HTMLSelectElement>(null),hours=useRef<HTMLInputElement>(null),reason=useRef<HTMLTextAreaElement>(null)
  const pending=useRef(false),mounted=useRef(true),verificationController=useRef<AbortController|null>(null)
  const original=initial(baseline),newIntent=!baseline.exception||baseline.exception.isRemoved
  const reasonValue=normalizeText(draft.reason,500)
  const normalized={working:draft.working,inherit:draft.working==='no'?true:draft.inherit,hours:draft.working==='yes'&&!draft.inherit?trimText(draft.hours):'',reason:reasonValue}
  const originalNormalized={working:original.working,inherit:original.working==='no'?true:original.inherit,hours:original.hours,reason:normalizeText(original.reason,500)}
  const dirty=JSON.stringify(normalized)!==JSON.stringify(originalNormalized)||newIntent&&draft.working!==''
  const guarded=dirty||state==='unknown'||state==='conflict'
  const editable=baseline.day.editable&&baseline.context.version!==null&&baseline.scope.line?.isActive!==false
  const removable=baseline.context.version!==null&&date>=baseline.context.plantToday&&baseline.exception!==null&&!baseline.exception.isRemoved
  useEffect(()=>{mounted.current=true;heading.current?.focus();return()=>{mounted.current=false;verificationController.current?.abort()}},[])
  useEffect(()=>guard.register(to=>{if(state==='saving')return true;if(!guarded)return false;setConfirm({kind:'leave',to});return true}),[guard,guarded,state])
  useEffect(()=>{if(!guarded&&state!=='saving')return;const warn=(e:BeforeUnloadEvent)=>{e.preventDefault();e.returnValue=''};window.addEventListener('beforeunload',warn);return()=>window.removeEventListener('beforeunload',warn)},[guarded,state])
  useEffect(()=>{if(history.status==='error'&&authFailure(history.error))onAuth()},[history,onAuth])
  async function perform(remove=false) {
    if(pending.current||state!=='editing'||!baseline.context.version)return
    let body:ExceptionSave|undefined
    if(!remove){
      const nextErrors:Record<string,string>={}
      if(!draft.working)nextErrors.working=w.requiredState
      if(draft.working==='yes'&&!draft.inherit&&!hoursValid(draft.hours))nextErrors.hours=w.hoursError
      if(reasonValue===undefined)nextErrors.reason=w.reasonError
      body={version:baseline.context.version,targetRevisionId:baseline.exception?.id??null,lineId:baseline.scope.lineId,date,isWorking:draft.working==='yes',workingHours:draft.working==='yes'&&!draft.inherit?trimText(draft.hours):null,reason:reasonValue??null}
      if(!bodyWithinLimit(body))nextErrors.reason=w.requestError
      setErrors(nextErrors)
      if(Object.keys(nextErrors).length){setNotice(w.validation);(nextErrors.working?working.current:nextErrors.hours?hours.current:reason.current)?.focus();return}
      if(!dirty||!editable)return
    }else if(!removable||!baseline.exception)return
    pending.current=true;setState('saving');setNotice(w.saving)
    try{
      const result=remove&&baseline.exception?await removeException({version:baseline.context.version,targetRevisionId:baseline.exception.id,lineId:baseline.scope.lineId,date}):body?await saveException(body):null
      if(mounted.current&&result)onSaved(result.changed)
    }catch(error){
      if(!mounted.current)return
      if(authFailure(error)){onAuth();return}
      setNotice(errorText(error));setState(error instanceof CalendarWriteError&&error.outcome==='Unknown'?'unknown':error instanceof CalendarWriteError&&['CALENDAR_STALE','CALENDAR_TARGET_CHANGED'].includes(error.code)?'conflict':error instanceof CalendarWriteError&&['CALENDAR_PAST_DATE','CALENDAR_LINE_RETIRED','CALENDAR_NOT_ACTIVATED','CALENDAR_TIMEZONE_MISMATCH','CALENDAR_PROTECTED_RULE'].includes(error.code)?'restricted':'editing')
      if(error instanceof CalendarWriteError&&error.retryAfter)await new Promise(r=>window.setTimeout(r,error.retryAfter*1000))
      summary.current?.focus()
    }finally{pending.current=false}
  }
  async function verify(){
    if(verifyPending)return
    const controller=new AbortController();verificationController.current=controller;setVerifyPending(true);setObservation(null)
    try{const [day,history]=await Promise.all([getDay(date,lineId,controller.signal),getExceptionHistory(date,lineId,1,undefined,controller.signal)]);if(mounted.current&&!controller.signal.aborted){setObservation({day,history});setNotice(w.observation)}}
    catch(error){if(mounted.current&&!controller.signal.aborted){if(authFailure(error))onAuth();else setNotice(errorText(error))}}
    finally{if(mounted.current)setVerifyPending(false)}
  }
  const blocked=state!=='editing'
  return <section aria-busy={state==='saving'} className="flex flex-col items-start gap-3 rounded border border-gray-300 p-4">
    <h2 ref={heading} tabIndex={-1} className="text-xl font-semibold">{w.exception} — {date}</h2><p>{baseline.scope.line?`${baseline.scope.line.code} ${baseline.scope.line.name}`:w.plant}</p>
    {notice&&<p ref={summary} tabIndex={-1} role={state==='unknown'||state==='conflict'?'alert':'status'} className="my-3">{notice}</p>}
    {baseline.context.version===null&&<p>{w.notActivated}</p>}{date<baseline.context.plantToday&&<p>{w.pastLocked}</p>}{baseline.scope.line?.isActive===false&&<p>{w.retiredLocked}</p>}
    <form onSubmit={e=>{e.preventDefault();void perform()}} noValidate className="grid w-full gap-4">
      <label>{w.date}<input className={input} value={date} readOnly/></label>
      <label>{w.exception}<select ref={working} className={input} value={draft.working} disabled={!editable||blocked} aria-invalid={!!errors.working} aria-describedby={errors.working?'working-error':undefined} onChange={e=>{const v=e.target.value;if(v==='yes'||v==='no'||v==='')setDraft(d=>({...d,working:v,inherit:true,hours:''}))}}><option value="">{w.requiredState}</option><option value="yes">{w.working}</option><option value="no">{w.closed}</option></select></label>
      {errors.working&&<p id="working-error">{errors.working}</p>}
      {draft.working==='yes'&&<><label className="flex min-h-12 items-center gap-2"><input type="checkbox" checked={draft.inherit} disabled={!editable||blocked} onChange={e=>setDraft(d=>({...d,inherit:e.target.checked,hours:''}))}/>{w.inherit}</label>{draft.inherit?<p>{w.hoursDependent}</p>:<label>{w.hours}<input ref={hours} className={input} inputMode="decimal" value={draft.hours} disabled={!editable||blocked} aria-invalid={!!errors.hours} aria-describedby={errors.hours?'hours-error':undefined} onChange={e=>setDraft(d=>({...d,hours:e.target.value}))}/></label>}</>}
      {errors.hours&&<p id="hours-error">{errors.hours}</p>}
      <label htmlFor="calendar-reason">{w.reason}</label><textarea id="calendar-reason" ref={reason} className={input} value={draft.reason} disabled={!editable||blocked} aria-invalid={!!errors.reason} aria-describedby={errors.reason?'reason-error':undefined} onChange={e=>setDraft(d=>({...d,reason:e.target.value}))}/>{errors.reason&&<p id="reason-error">{errors.reason}</p>}
      <div className="flex flex-wrap items-start gap-2"><button className={`${button} bg-orange-700 text-white`} disabled={!editable||blocked||!dirty} type="submit">{state==='saving'?w.saving:w.save}</button><button type="button" className={button} disabled={state==='saving'} onClick={()=>guarded?setConfirm({kind:'cancel'}):onCancel()}>{w.cancel}</button><button type="button" className={button} disabled={!removable||blocked} onClick={()=>setConfirm({kind:'remove'})}>{w.remove}</button></div>
    </form>
    {(state==='conflict'||state==='restricted')&&<button className={button} onClick={()=>setConfirm({kind:'reload'})}>{w.reload}</button>}
    {state==='unknown'&&<div className="grid gap-3"><div className="flex flex-wrap items-start gap-2"><button className={button} disabled={verifyPending} onClick={()=>void verify()}>{verifyPending?w.loading:w.verify}</button><button className={button} disabled={!observation||verifyPending} onClick={()=>setConfirm({kind:'accept'})}>{w.acceptCurrent}</button></div>{observation&&<div className="my-3"><p>{w.observation}</p><p>{w.current}: {observation.day.context.version} / {w.history}: {observation.history.context.version}</p><p>{observation.day.day.date}: {w[observation.day.day.state==='Working'?'working':observation.day.day.state==='Closed'?'closed':'unavailable']}</p><p>{observation.day.exception?.reason}</p></div>}</div>}
    <button className={button} onClick={()=>setHistoryOpen(v=>!v)}>{w.history}</button>
    {historyOpen&&<div aria-live="polite" className="grid w-full justify-items-start gap-3">{history.status==='loading'?w.loading:history.status==='error'?errorText(history.error):<><ol className="w-full space-y-3">{history.value.items.map(row=><li key={row.id} className="rounded border p-3"><p>{row.isCurrent?w.current:w.retained} / {row.commitRevision} / {row.createdAtUtc}</p><p>{row.isRemoved?w.historyMarker:row.isWorking?w.working:w.closed} {row.workingHours}</p><p className="break-words">{row.reason}</p></li>)}</ol><div className="flex flex-wrap items-start gap-2"><button className={button} disabled={historyPage===1} onClick={()=>{setHistoryToken(history.value.snapshotVersion??undefined);setHistoryPage(n=>n-1)}}>{w.previousPage}</button><button className={button} disabled={historyPage>=history.value.totalPages} onClick={()=>{setHistoryToken(history.value.snapshotVersion??undefined);setHistoryPage(n=>n+1)}}>{w.nextPage}</button></div></>}<button className={button} onClick={()=>{setHistoryPage(1);setHistoryToken(undefined);setHistoryGeneration(n=>n+1)}}>{w.restart}</button></div>}
    <LineDialog open={confirm!==null} title={confirm?.kind==='remove'?w.removeTitle:w.discardTitle} confirm={confirm?.kind==='remove'?w.remove:w.discard} onCancel={()=>setConfirm(null)} onConfirm={()=>{const action=confirm;setConfirm(null);if(!action)return;if(action.kind==='remove')void perform(true);else if(action.kind==='leave')void navigate(action.to);else if(action.kind==='cancel')onCancel();else if(action.kind!=='accept'||observation)onReload()}}>{confirm?.kind==='remove'?w.removeBody:w.discardBody}</LineDialog>
  </section>
}
