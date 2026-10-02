import { useEffect,useRef,useState } from 'react'
import { labels } from '../production-orders/messages'
import { lineButtonClass as button,lineInputClass as input } from '../production-lines/LineDialog'
import { getCapacity,getLines,getProducts,type Capacity } from './api'
import { authFailure,useCalendarRead } from './useCalendarRead'
import { errorText,unavailableText } from './viewRules'
import { dateValid,formatExact,normalizeText } from './values'

const w=labels.calendar
/** Independent current reference lookup never borrows calendar/editor response versions. */
export function CapacityPanel({today,initialLine,initialProduct,initialDate,generation,onAuth}:{today:string;initialLine?:string;initialProduct?:string;initialDate:string;generation:number;onAuth:()=>void}) {
  const [line,setLine]=useState(initialLine??'')
  const [product,setProduct]=useState(initialProduct??'')
  const [date,setDate]=useState(initialDate>=today?initialDate:today)
  const [lineSearch,setLineSearch]=useState(''),[lineQuery,setLineQuery]=useState(''),[linePage,setLinePage]=useState(1)
  const [search,setSearch]=useState(''),[query,setQuery]=useState(''),[page,setPage]=useState(1)
  const [request,setRequest]=useState<{line:string;product:string;date:string;seq:number;generation:number}|null>(null)
  const sequence=useRef(0)
  const lines=useCalendarRead('capacity-lines:'+lineQuery+':'+linePage+':'+generation,s=>getLines(lineQuery,linePage,s))
  const choices=useCalendarRead(line?'capacity-products:'+line+':'+query+':'+page+':'+generation:null,s=>getProducts(line,query,page,s))
  const validRequest=request && request.line===line && request.product===product && request.date===date && request.generation===generation
  const capacity=useCalendarRead(validRequest?JSON.stringify(request):null,s=>getCapacity(line,product,date,s))
  useEffect(()=>{
    for(const state of [lines,choices,capacity])if(state.status==='error' && authFailure(state.error))onAuth()
  },[lines,choices,capacity,onAuth])
  const [invalid,setInvalid]=useState(false)
  return <section className="mt-6 flex flex-col items-start gap-3 rounded border border-gray-300 p-4" aria-labelledby="capacity-title">
    <h2 id="capacity-title" className="text-xl font-semibold">{w.capacity}</h2><p className="text-sm text-gray-700">{w.currentSettings}</p>
    <div className="grid w-full items-start gap-3 sm:grid-cols-2">
      <label>{w.line}<select className={input} value={line} onChange={e=>{setLine(e.target.value);setProduct('');setPage(1);setQuery('');setSearch('');setRequest(null)}}><option value="">{w.noChoices}</option>
        {initialLine && lines.status==='ready' && !lines.value.items.some(l=>l.id===initialLine) && <option value={initialLine}>{initialLine}</option>}
        {lines.status==='ready' && lines.value.items.map(l=><option key={l.id} value={l.id}>{l.code} {l.name}</option>)}
      </select></label>
      <div className="grid justify-items-start gap-2"><label className="w-full">{w.search}<input className={input} value={lineSearch} onChange={e=>setLineSearch(e.target.value)}/></label><button className={button} type="button" onClick={()=>{const v=normalizeText(lineSearch,100);if(v!==undefined){setLineQuery(v??'');setLinePage(1)}}}>{w.search}</button></div>
      <div className="flex flex-wrap items-start gap-2"><button className={button} disabled={linePage===1} onClick={()=>setLinePage(p=>p-1)}>{w.previousPage}</button><button className={button} disabled={lines.status!=='ready'||linePage>=lines.value.totalPages} onClick={()=>setLinePage(p=>p+1)}>{w.nextPage}</button></div>
      <label>{w.product}<select className={input} value={product} disabled={!line} onChange={e=>{setProduct(e.target.value);setRequest(null)}}><option value="">{choices.status==='ready'&&choices.value.totalCount===0?w.noMatches:w.noChoices}</option>
        {product && choices.status==='ready' && !choices.value.items.some(p=>p.id===product) && <option value={product}>{product}</option>}
        {choices.status==='ready' && choices.value.items.map(p=><option key={p.id} value={p.id}>{p.sku} {p.name} ({p.unit})</option>)}
      </select></label>
      <div className="grid justify-items-start gap-2"><label className="w-full">{w.search}<input className={input} value={search} onChange={e=>{setSearch(e.target.value);setRequest(null)}} disabled={!line}/></label><button type="button" className={button} disabled={!line} onClick={()=>{const v=normalizeText(search,100);if(v!==undefined){setQuery(v??'');setPage(1)}}}>{w.search}</button></div>
      <div className="flex flex-wrap items-start gap-2"><button className={button} disabled={page===1} onClick={()=>setPage(p=>p-1)}>{w.previousPage}</button><button className={button} disabled={choices.status!=='ready'||page>=choices.value.totalPages} onClick={()=>setPage(p=>p+1)}>{w.nextPage}</button></div>
      <label>{w.date}<input type="date" className={input} value={date} min={today} onChange={e=>{setDate(e.target.value);setRequest(null)}} aria-invalid={invalid}/></label>
    </div>
    {invalid && <p role="alert">{w.validation}</p>}
    {[lines,choices].map((state,i)=>state.status==='error'?<p key={i} role="alert">{errorText(state.error)}</p>:null)}
    <button type="button" className={button} disabled={!line||!product} onClick={()=>{if(!dateValid(date)||date<today){setInvalid(true);return}setInvalid(false);setRequest({line,product,date,seq:++sequence.current,generation})}}>{w.lookup}</button>
    {validRequest && <div aria-live="polite" className="w-full">{capacity.status==='loading'?w.loading:capacity.status==='error'?errorText(capacity.error):<CapacityResult result={capacity.value}/>}</div>}
  </section>
}
function CapacityResult({result}:{result:Capacity}) {
  return result.availability==='Unavailable'?<p>{unavailableText(result.unavailableReason)}</p>:<div><p className="text-2xl font-semibold">{result.quantity!==null?formatExact(result.quantity):''} {result.product.unit}</p><p>{w.hours}: {result.day.hours} / {w.source}: {w[result.day.source.kind]}</p>{result.day.source.reason && <p>{result.day.source.reason}</p>}</div>
}
