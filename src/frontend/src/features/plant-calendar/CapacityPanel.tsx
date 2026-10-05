import { ChoiceLabel } from './ChoiceLabel'
import { ChoiceSearch } from './ChoiceSearch'
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
  const [choiceRefresh,setChoiceRefresh]=useState(0)
  const lines=useCalendarRead('capacity-lines:'+lineQuery+':'+linePage+':'+generation+':'+choiceRefresh,s=>getLines(lineQuery,linePage,s))
  const choices=useCalendarRead(line?'capacity-products:'+line+':'+query+':'+page+':'+generation+':'+choiceRefresh:null,s=>getProducts(line,query,page,s))
  const [lineLabels,setLineLabels]=useState<Record<string,string>>({}),[productLabels,setProductLabels]=useState<Record<string,string>>({})
  const validRequest=request && request.line===line && request.product===product && request.date===date && request.generation===generation
  const capacity=useCalendarRead(validRequest?JSON.stringify(request):null,s=>getCapacity(line,product,date,s).then(result=>{if(!s.aborted){setLineLabels(previous=>({...previous,[result.line.id]:`${result.line.code} ${result.line.name}`}));setProductLabels(previous=>({...previous,[result.product.id]:`${result.product.sku} ${result.product.name} (${result.product.unit})`}))}return result}))
  useEffect(()=>{
    for(const state of [lines,choices,capacity])if(state.status==='error' && authFailure(state.error))onAuth()
  },[lines,choices,capacity,onAuth])
  const rememberLabels=()=>{if(lines.status==='ready')setLineLabels(previous=>({...previous,...Object.fromEntries(lines.value.items.map(l=>[l.id,`${l.code} ${l.name}`]))}));if(choices.status==='ready')setProductLabels(previous=>({...previous,...Object.fromEntries(choices.value.items.map(p=>[p.id,`${p.sku} ${p.name} (${p.unit})`]))}))}
  const [invalid,setInvalid]=useState(false)
  const selectedLine=lines.status==='ready'?lines.value.items.find(l=>l.id===line):undefined
  const selectedProduct=choices.status==='ready'?choices.value.items.find(p=>p.id===product):undefined
  const lineLabel=selectedLine?`${selectedLine.code} ${selectedLine.name}`:lineLabels[line]??''
  const productLabel=selectedProduct?`${selectedProduct.sku} ${selectedProduct.name} (${selectedProduct.unit})`:productLabels[product]??''
  return <section className="calendar-capacity rounded border border-gray-300 p-4" aria-labelledby="capacity-title">
    <h2 id="capacity-title" className="text-xl font-semibold">{w.capacity}</h2><p className="text-sm text-gray-700">{w.currentSettings}</p>
    <div className="calendar-capacity-fields">
      <div className="calendar-field"><label>{w.line}<select className={input} value={line} onChange={e=>{rememberLabels();setLine(e.target.value);setProduct('');setPage(1);setQuery('');setSearch('');setRequest(null)}}><option value="">{w.noChoices}</option>{line&&!(lines.status==='ready'&&lines.value.items.some(l=>l.id===line))&&<option value={line}>{lineLabels[line]??w.selectionUnconfirmed}</option>}{lines.status==='ready'&&lines.value.items.map(l=><option key={l.id} value={l.id}>{l.code} {l.name}</option>)}</select></label>
      {lineLabel&&<ChoiceLabel text={lineLabel}/>}
      <ChoiceSearch title={w.lineSearch} value={lineSearch} onChange={v=>{setLineSearch(v);setRequest(null)}} onSearch={()=>{rememberLabels();const v=normalizeText(lineSearch,100);if(v!==undefined){setLineQuery(v??'');setLinePage(1);setRequest(null)}}} onClear={()=>{rememberLabels();setLineSearch('');setLineQuery('');setLinePage(1)}} page={linePage} totalPages={lines.status==='ready'?lines.value.totalPages:0} totalCount={lines.status==='ready'?lines.value.totalCount:0} onPage={p=>{rememberLabels();setLinePage(p);setRequest(null)}} error={lines.status==='error'?errorText(lines.error):undefined} onRetry={()=>setChoiceRefresh(n=>n+1)}/></div></div>
    <div className="calendar-row">
      <div className="calendar-field"><label>{w.product}<select className={input} value={product} disabled={!line} onChange={e=>{rememberLabels();setProduct(e.target.value);setRequest(null)}}><option value="">{choices.status==='ready'&&choices.value.totalCount===0?w.noMatches:w.noChoices}</option>{product&&!(choices.status==='ready'&&choices.value.items.some(p=>p.id===product))&&<option value={product}>{productLabels[product]??w.selectionUnconfirmed}</option>}{choices.status==='ready'&&choices.value.items.map(p=><option key={p.id} value={p.id}>{p.sku} {p.name} ({p.unit})</option>)}</select></label>
      {productLabel&&<ChoiceLabel text={productLabel}/>}
      <ChoiceSearch title={w.productSearch} value={search} onChange={v=>{setSearch(v);setRequest(null)}} disabled={!line} onSearch={()=>{rememberLabels();const v=normalizeText(search,100);if(v!==undefined){setQuery(v??'');setPage(1);setRequest(null)}}} onClear={()=>{rememberLabels();setSearch('');setQuery('');setPage(1)}} page={page} totalPages={choices.status==='ready'?choices.value.totalPages:0} totalCount={choices.status==='ready'?choices.value.totalCount:0} onPage={p=>{rememberLabels();setPage(p);setRequest(null)}} error={choices.status==='error'?errorText(choices.error):undefined} onRetry={()=>setChoiceRefresh(n=>n+1)}/></div>
      <label>{w.date}<input type="date" className={input} value={date} min={today} onChange={e=>{setDate(e.target.value);setRequest(null)}} aria-invalid={invalid} aria-describedby={invalid?'capacity-date-error':undefined}/></label><button type="button" className={`${button} calendar-submit`} disabled={!line||!product} onClick={()=>{if(!dateValid(date)||date<today){setInvalid(true);return}setInvalid(false);setRequest({line,product,date,seq:++sequence.current,generation})}}>{w.lookup}</button>
    </div>
    {invalid && <p id="capacity-date-error" role="alert">{w.validation}</p>}

    {validRequest && <div aria-live="polite" className="calendar-result w-full">{capacity.status==='loading'?w.loading:capacity.status==='error'?<div className="grid justify-items-start gap-3"><p role="alert">{errorText(capacity.error)}</p><button className={button} onClick={()=>setRequest({line,product,date,seq:++sequence.current,generation})}>{w.reload}</button></div>:<CapacityResult result={capacity.value}/>}</div>}
  </section>
}
function CapacityResult({result}:{result:Capacity}) {
  return result.availability==='Unavailable'?<p>{unavailableText(result.unavailableReason)}</p>:<div><p className="text-2xl font-semibold">{result.quantity!==null?formatExact(result.quantity):''} {result.product.unit}</p><p>{result.line.code} / {result.product.sku} / {result.date}</p><p>{w.hours}: {result.day.hours} {w.hoursUnit} × 60 ÷ {result.minutesPerUnit} {w.minutesUnit} / {result.product.unit}</p><p>{w.source}: {w[result.day.source.kind]}</p>{result.day.source.reason && <p>{result.day.source.reason}</p>}</div>
}
