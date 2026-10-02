import { useEffect,useEffectEvent,useRef,useState } from 'react'
import { ApiError } from '../../lib/apiClient'

export type ReadState<T> = {status:'loading'}|{status:'ready';value:T}|{status:'error';error:unknown}
/** A mounted read owns one key and sequence; cancellation never implies write rollback. */
export function useCalendarRead<T>(key:string|null,read:(signal:AbortSignal)=>Promise<T>):ReadState<T> {
  const execute=useEffectEvent(read)
  const sequence=useRef(0)
  const [result,setResult]=useState<{key:string;state:ReadState<T>}|null>(null)
  useEffect(()=>{
    const seq=++sequence.current
    if(key===null)return
    const controller=new AbortController()
    execute(controller.signal).then(value=>{
      if(!controller.signal.aborted && seq===sequence.current)setResult({key,state:{status:'ready',value}})
    }).catch((error:unknown)=>{
      if(!controller.signal.aborted && seq===sequence.current)setResult({key,state:{status:'error',error}})
    })
    return()=>controller.abort()
  },[key])
  return key!==null && result?.key===key ? result.state : {status:'loading'}
}
export const authFailure=(error:unknown)=>error instanceof ApiError && (error.status===401 || error.status===403)
