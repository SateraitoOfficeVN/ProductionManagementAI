import { labels } from '../production-orders/messages'
import { lineButtonClass as button,lineInputClass as input } from '../production-lines/LineDialog'
const w=labels.calendar
/** Keep bounded paging inside the choice group to which it belongs. */
export function ChoiceSearch({title,value,onChange,onSearch,onClear,page,totalPages,totalCount,onPage,disabled=false,error,onRetry}:{title:string;value:string;onChange:(v:string)=>void;onSearch:()=>void;onClear:()=>void;page:number;totalPages:number;totalCount:number;onPage:(p:number)=>void;disabled?:boolean;error?:string;onRetry?:()=>void}) {
  return <details className="calendar-choice"><summary>{title}</summary><div className="calendar-search-body">
    <label>{w.search}<input className={input} value={value} maxLength={100} disabled={disabled} onChange={e=>onChange(e.target.value)}/></label>
    <div className="calendar-actions"><button type="button" className={button} disabled={disabled} onClick={onSearch}>{w.search}</button><button type="button" className={button} disabled={disabled} onClick={onClear}>{w.clearSearch}</button></div>
    {error?<><p role="alert">{error}</p><button type="button" className={button} onClick={onRetry}>{w.reload}</button></>:<p aria-live="polite">{w.choiceCount(totalCount===0?0:(page-1)*50+1,Math.min(page*50,totalCount),totalCount)}</p>}
    {totalPages>1&&<div className="calendar-actions"><button type="button" className={button} disabled={disabled||page<=1} onClick={()=>onPage(page-1)}>{w.previousPage}</button><button type="button" className={button} disabled={disabled||page>=totalPages} onClick={()=>onPage(page+1)}>{w.nextPage}</button></div>}
  </div></details>
}
