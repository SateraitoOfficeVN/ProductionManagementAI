import { useCallback,useEffect,useRef,useState } from 'react'
import { useNavigate,useSearchParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { useNavigationGuard } from '../../lib/navigationGuard'
import { labels } from '../production-orders/messages'
import { lineButtonClass as button,lineInputClass as input } from '../production-lines/LineDialog'
import { useLineRole } from '../production-lines/lineViewRules'
import { getDay,getLines,getMonth,type Day } from './api'
import { parseCalendarUrl,monthValid,normalizeText,weekdays,type CalendarUrl } from './values'
import { authFailure,useCalendarRead } from './useCalendarRead'
import { errorText,shiftMonth } from './viewRules'
import { CapacityPanel } from './CapacityPanel'
import { DateExceptionEditor } from './DateExceptionEditor'
import { WeeklyPatternEditor } from './WeeklyPatternEditor'
const w=labels.calendar
function url(v:CalendarUrl):string {const query=new URLSearchParams();for(const key of ['month','lineId','date','mode','productId'] as const){if(v[key])query.set(key,v[key])}return '/plant-calendar?'+query.toString()}
export function PlantCalendarPage(){
  const allowed=useLineRole(),[forbidden,setForbidden]=useState(false)
  const onAuth=useCallback(()=>setForbidden(true),[])
  return <><AppHeader/><main className="mx-auto max-w-7xl p-4 sm:p-6">{!allowed||forbidden?<><h1 className="text-2xl font-semibold">{w.title}</h1><p role="alert">{w.forbidden}</p></>:<CalendarContent onAuth={onAuth}/>}</main></>
}
function CalendarContent({onAuth}:{onAuth:()=>void}){
  const [search]=useSearchParams(),navigate=useNavigate(),guard=useNavigationGuard()
  const parsed=parseCalendarUrl(search.toString())
  const [generation,setGeneration]=useState(0),[notice,setNotice]=useState('')
  const bootstrap=useCalendarRead(parsed?'calendar-bootstrap:'+generation:null,s=>getLines('',1,s))
  const context=bootstrap.status==='ready'?bootstrap.value.context:null
  const month=parsed?.month??context?.plantToday.slice(0,7)
  const date=parsed?.date??(month&&context?(context.plantToday.startsWith(month+'-')?context.plantToday:month+'-01'):undefined)
  const applied:CalendarUrl|null=parsed&&month&&date?{...parsed,month,date}:null
  const filterKey=month+':'+parsed?.lineId
  const [filterDraft,setFilterDraft]=useState<{key:string;month:string;line:string}|null>(null)
  const filter=filterDraft?.key===filterKey?filterDraft:{key:filterKey,month:month??'',line:parsed?.lineId??''}
  const draftMonth=filter.month,draftLine=filter.line
  const setDraftMonth=(value:string)=>setFilterDraft({...filter,month:value})
  const setDraftLine=(value:string)=>setFilterDraft({...filter,line:value})
  const [lineSearch,setLineSearch]=useState(''),[lineQuery,setLineQuery]=useState(''),[linePage,setLinePage]=useState(1),[validation,setValidation]=useState(false)
  const [monthRefresh,setMonthRefresh]=useState(0)
  const refreshed=useRef<string|null>(null),heading=useRef<HTMLHeadingElement>(null)
  const choices=useCalendarRead(parsed?'calendar-choices:'+lineQuery+':'+linePage+':'+generation:null,s=>getLines(lineQuery,linePage,s))
  const readMonth=useCalendarRead(applied?`month:${month}:${parsed?.lineId}:${generation}:${monthRefresh}`:null,s=>getMonth(month??'',parsed?.lineId,s))
  const readDay=useCalendarRead(applied?`day:${date}:${parsed?.lineId}:${generation}`:null,s=>getDay(date??'',parsed?.lineId,s))
  const mode=parsed?.mode??'month'
  useEffect(()=>{document.title=labels.app.title(w.title);heading.current?.focus()},[mode])
  const canonical=applied?url(applied):null
  useEffect(()=>{if(canonical&&(!search.has('month')||!search.has('date')))void navigate(canonical,{replace:true})},[canonical,search,navigate])
  useEffect(()=>{for(const state of [bootstrap,choices,readMonth,readDay])if(state.status==='error'&&authFailure(state.error))onAuth()},[bootstrap,choices,readMonth,readDay,onAuth])
  useEffect(()=>{
    const key=`${month}:${date}:${parsed?.lineId}:${generation}`
    if(readMonth.status==='ready'&&readDay.status==='ready'&&readMonth.value.context.version!==readDay.value.context.version&&refreshed.current!==key){refreshed.current=key;setMonthRefresh(n=>n+1)}
  },[readMonth,readDay,month,date,parsed?.lineId,generation])
  const go=(next:CalendarUrl)=>{const target=url(next);if(!guard.intercepts(target)){setNotice('');void navigate(target)}}
  const refresh=()=>{setGeneration(n=>n+1)}
  const saved=(changed:boolean)=>{setNotice(changed?w.saved:w.unchanged);refresh();if(applied)void navigate(url({...applied,mode:'month'}))}
  if(!parsed)return <><h1 ref={heading} tabIndex={-1} className="text-2xl font-semibold">{w.title}</h1><p role="alert" className="my-3">{w.invalidUrl}</p><button className={button} onClick={()=>void navigate('/plant-calendar',{replace:true})}>{w.reload}</button></>
  if(bootstrap.status==='error')return <><h1 ref={heading} tabIndex={-1} className="text-2xl font-semibold">{w.title}</h1>{notice&&<p role="status">{notice}</p>}{notice&&<p role="alert">{w.refreshAfterSave}</p>}<p role="alert" className="my-3">{errorText(bootstrap.error)}</p><button className={button} onClick={refresh}>{w.reload}</button></>
  if(!applied||!month||!date||!context)return <><h1 ref={heading} tabIndex={-1} className="text-2xl font-semibold">{w.title}</h1>{notice&&<p role="status">{notice}</p>}<p role="status">{w.loading}</p></>
  const previous=shiftMonth(month,-1),next=shiftMonth(month,1)
  const mismatch=readMonth.status==='ready'&&readDay.status==='ready'&&readMonth.value.context.version!==readDay.value.context.version
  return <>
    <h1 ref={heading} tabIndex={-1} className="mb-4 text-2xl font-semibold">{w.title}</h1>
    {notice&&<p role="status" className="my-3">{notice}</p>}{notice&&readMonth.status==='error'&&<p role="alert">{w.refreshAfterSave}</p>}
    {context.activatedOn===null&&<p>{w.notActivated}</p>}
    {mode==='weekly'?<WeeklyPatternEditor key={date} month={month} date={date} onSelect={d=>go({...applied,month:d.slice(0,7),date:d,mode:'weekly'})} onSaved={saved} onCancel={()=>{setNotice('');void navigate(url({...applied,mode:'month'}))}} onAuth={onAuth}/>:mode==='day'||mode==='history'?<DateExceptionEditor key={date+':'+parsed.lineId} date={date} lineId={parsed.lineId} initialHistory={mode==='history'} onSaved={saved} onCancel={()=>{setNotice('');void navigate(url({...applied,mode:'month'}))}} onAuth={onAuth}/>:<>
      <form className="grid items-end gap-3 sm:grid-cols-3" onSubmit={e=>{e.preventDefault();if(!monthValid(draftMonth)){setValidation(true);return}setValidation(false);go({month:draftMonth,lineId:draftLine||undefined,date:context.plantToday.startsWith(draftMonth+'-')?context.plantToday:draftMonth+'-01',mode:'month'})}}>
        <label>{w.month}<input className={input} type="month" min="0001-01" max="9999-12" value={draftMonth} onChange={e=>setDraftMonth(e.target.value)} aria-invalid={validation}/></label>
        <label>{w.line}<select className={input} value={draftLine} onChange={e=>setDraftLine(e.target.value)}><option value="">{w.plant}</option>{readMonth.status==='ready'&&readMonth.value.scope.line&&!((choices.status==='ready'?choices.value.items:[]).some(l=>l.id===readMonth.value.scope.lineId))&&<option value={readMonth.value.scope.line.id}>{readMonth.value.scope.line.code} {readMonth.value.scope.line.name}</option>}{choices.status==='ready'&&choices.value.items.map(line=><option key={line.id} value={line.id}>{line.code} {line.name}</option>)}</select></label>
        <button className={button}>{w.apply}</button>
      </form>
      {validation&&<p role="alert">{w.validation}</p>}
      <div className="mt-3 flex flex-wrap items-end gap-2"><label>{w.search}<input className={input} value={lineSearch} onChange={e=>setLineSearch(e.target.value)}/></label><button className={button} onClick={()=>{const q=normalizeText(lineSearch,100);if(q===undefined){setValidation(true);return}setLineQuery(q??'');setLinePage(1)}}>{w.search}</button><button className={button} disabled={linePage===1} onClick={()=>setLinePage(n=>n-1)}>{w.previousPage}</button><button className={button} disabled={choices.status!=='ready'||linePage>=choices.value.totalPages} onClick={()=>setLinePage(n=>n+1)}>{w.nextPage}</button></div>
      {choices.status==='error'&&<p role="alert">{errorText(choices.error)}</p>}
      <div className="my-4 flex flex-wrap items-center gap-3"><button className={button} disabled={!previous} onClick={()=>previous&&go({...applied,month:previous,date:previous+'-01',productId:undefined})}>{w.previous}</button><h2 className="text-xl font-semibold" aria-live="polite">{month}</h2><button className={button} disabled={!next} onClick={()=>next&&go({...applied,month:next,date:next+'-01',productId:undefined})}>{w.next}</button><button className={button} onClick={refresh}>{w.reload}</button></div>
      <p className="my-3">{w.working} / {w.closed} / {w.unavailable}</p>
      {mismatch&&<p role="status">{w.monthOlder}</p>}
      <div aria-busy={readMonth.status==='loading'}>{readMonth.status==='loading'?<p role="status">{w.loading}</p>:readMonth.status==='error'?<p role="alert">{errorText(readMonth.error)}</p>:<MonthView days={readMonth.value.days} selected={date} today={readMonth.value.context.plantToday} onSelect={d=>go({...applied,date:d,productId:undefined})}/>}</div>
      <section className="mt-4 rounded border border-gray-300 p-4" aria-busy={readDay.status==='loading'}><h2 className="text-xl font-semibold">{date}</h2>{readDay.status==='loading'?<p>{w.loading}</p>:readDay.status==='error'?<p role="alert">{errorText(readDay.error)}</p>:<>
        <p>{stateText(readDay.value.day)} / {w.source}: {w[readDay.value.day.source.kind]}</p><p>{w.hours}: {readDay.value.day.hours??w.hoursDependent}</p><p className="break-words">{readDay.value.day.source.reason}</p><p>{w.fallback}: {readDay.value.day.fallback.map(s=>w[s.kind]).join(' → ')}</p>
        {date<readDay.value.context.plantToday&&<p>{w.pastLocked}</p>}
        <div className="mt-3 flex flex-wrap items-end gap-2"><button className={button} onClick={()=>go({...applied,mode:'day'})}>{w.exception}</button><button className={button} onClick={()=>go({...applied,mode:'history'})}>{w.history}</button><button className={button} onClick={()=>go({...applied,mode:'weekly',date:readDay.value.context.plantToday,month:readDay.value.context.plantToday.slice(0,7)})}>{w.weekly}</button></div>
      </>}</section>
      <CapacityPanel key={month+':'+date+':'+parsed.lineId} today={context.plantToday} initialLine={parsed.lineId} initialProduct={parsed.productId} initialDate={date} generation={generation} onAuth={onAuth}/>
    </>}
  </>
}
const stateText=(day:Day)=>day.state==='Working'?w.working:day.state==='Closed'?w.closed:w.unavailable
function MonthView({days,selected,today,onSelect}:{days:Day[];selected:string;today:string;onSelect:(d:string)=>void}){
  const [year,month]=days[0]?.date.split('-').map(Number)??[1,1]
  const y=year-1,leap=year%4===0&&(year%100!==0||year%400===0)
  const offset=(365*y+Math.floor(y/4)-Math.floor(y/100)+Math.floor(y/400)+[31,leap?29:28,31,30,31,30,31,31,30,31,30,31].slice(0,month-1).reduce((a,b)=>a+b,0))%7
  const cells:Array<Day|null>=[...Array.from({length:offset},()=>null),...days]
  while(cells.length%7)cells.push(null)
  const dayButton=(day:Day)=><button type="button" className={`${button} w-full text-left ${day.date===selected?'ring-2 ring-gray-900':''} ${day.state==='Closed'?'bg-gray-100':''}`} aria-label={`${day.date} ${stateText(day)} ${w[day.source.kind]}`} aria-current={day.date===today?'date':undefined} aria-pressed={day.date===selected} onClick={()=>onSelect(day.date)}><span className="block font-semibold">{day.date.slice(-2)}</span><span className="block text-sm">{stateText(day)}</span><span className="block text-xs">{w[day.source.kind]}</span></button>;
  return <><table className="hidden w-full table-fixed sm:table"><caption className="sr-only">{w.title}</caption><thead><tr>{weekdays.map(day=><th key={day} scope="col">{w.weekdays[day]}</th>)}</tr></thead><tbody>{Array.from({length:cells.length/7},(_,row)=><tr key={row}>{cells.slice(row*7,row*7+7).map((day,column)=><td key={column} className="p-1 align-top">{day&&dayButton(day)}</td>)}</tr>)}</tbody></table><ol className="space-y-2 sm:hidden">{days.map(day=><li key={day.date}>{dayButton(day)}</li>)}</ol></>
}
