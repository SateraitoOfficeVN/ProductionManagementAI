import { useEffect,useRef,useState } from 'react'
/** Reveal the complete business label when the native select clips it. */
export function ChoiceLabel({text}:{text:string}) {
  const measure=useRef<HTMLSpanElement>(null),[clipped,setClipped]=useState(false)
  useEffect(()=>{
    const span=measure.current,select=span?.closest('.calendar-field')?.querySelector('select')
    if(!span||!select)return
    const update=()=>setClipped(span.getBoundingClientRect().width>select.clientWidth-40)
    const observer=new ResizeObserver(update)
    observer.observe(select)
    const frame=requestAnimationFrame(update)
    return()=>{observer.disconnect();cancelAnimationFrame(frame)}
  },[text])
  return <><span className="calendar-label-measure" aria-hidden="true"><span ref={measure}>{text}</span></span>{clipped&&<p className="text-sm text-gray-600" aria-hidden="true">{text}</p>}</>
}
