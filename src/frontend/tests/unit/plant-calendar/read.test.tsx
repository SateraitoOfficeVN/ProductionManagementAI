import { act,renderHook } from '@testing-library/react'
import { it,expect } from 'vitest'
import { useCalendarRead } from '../../../src/features/plant-calendar/useCalendarRead'
function deferred<T>() { let finish:(v:T)=>void=()=>{};const promise=new Promise<T>(r=>{finish=r});return {promise,finish} }
it('TC-380 ignores obsolete completions and immediately hides relabeled old data',async()=>{
  const old=deferred<string>(),next=deferred<string>()
  const {result,rerender}=renderHook(({key})=>useCalendarRead(key,()=>key==='old'?old.promise:next.promise),{initialProps:{key:'old'}})
  expect(result.current.status).toBe('loading')
  rerender({key:'new'})
  await act(async()=>old.finish('obsolete'))
  expect(result.current.status).toBe('loading')
  await act(async()=>next.finish('current'))
  expect(result.current).toEqual({status:'ready',value:'current'})
  rerender({key:'other'})
  expect(result.current.status).toBe('loading')
})
it('TC-380 aborts a reader on unmount without fabricating a write outcome',()=>{
  let signal:AbortSignal|undefined
  const {unmount}=renderHook(()=>useCalendarRead('one',s=>{signal=s;return new Promise<string>(()=>{})}))
  expect(signal?.aborted).toBe(false);unmount();expect(signal?.aborted).toBe(true)
})
