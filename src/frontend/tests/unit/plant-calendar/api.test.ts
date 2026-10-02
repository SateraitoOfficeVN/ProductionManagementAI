import { afterEach,describe,it,expect,vi } from 'vitest'
import { setUnauthorizedHandler } from '../../../src/lib/apiClient'
import { getWeekly,getDay,saveWeekly,CalendarWriteError } from '../../../src/features/plant-calendar/api'
const context={plantToday:'2031-06-11',timeZone:'Asia/Tokyo',activatedOn:'2031-06-10',version:'9007199254740993'}
const target={id:'11111111-1111-1111-1111-111111111111',effectiveFrom:'2031-06-11',workingDays:['Mon'],isWithdrawn:false,isCurrent:true,commitRevision:context.version,createdAtUtc:'2031-06-11T03:00:00+00:00'}
const body={version:context.version,targetRevisionId:target.id,workingDays:[]}
const respond=(v:unknown,status=200)=>vi.stubGlobal('fetch',vi.fn().mockResolvedValue(new Response(JSON.stringify(v),{status,headers:{'Content-Type':'application/json'}})))
afterEach(()=>{vi.unstubAllGlobals();setUnauthorizedHandler(null)})
describe('TC-377/385 feature calendar settlement',()=>{
  it('returns acknowledged no-op with exact version strings and nullable shape',async()=>{
    respond({context,changed:false,target})
    const result=await saveWeekly('2031-06-11',body)
    expect(result.changed).toBe(false);expect(result.context.version).toBe(context.version)
    expect(fetch).toHaveBeenCalledTimes(1)
    const init=vi.mocked(fetch).mock.calls[0]?.[1]
    expect(init?.credentials).toBe('same-origin')
    expect(JSON.parse(String(init?.body)).version).toBe(context.version)
  })
  it.each([{}, {context,changed:true,target:{...target,isCurrent:false}}, {context,changed:true,target:{...target,workingDays:['Bogus']}}])('malformed success is unknown without replay',async response=>{
    respond(response)
    await expect(saveWeekly('2031-06-11',body)).rejects.toMatchObject({outcome:'Unknown'})
    expect(fetch).toHaveBeenCalledTimes(1)
  })
  it.each([
    [409,{code:'CALENDAR_STALE',writeOutcome:'NotApplied'},'NotApplied'],
    [503,{code:'CALENDAR_BUSY',writeOutcome:'NotApplied'},'NotApplied'],
    [503,{code:'CALENDAR_WRITE_UNKNOWN',writeOutcome:'Unknown'},'Unknown'],
    [500,{code:'UNEXPECTED',writeOutcome:'NotApplied'},'Unknown'],
    [409,{code:'CALENDAR_STALE'},'Unknown'],
    [409,{code:'UNRECOGNIZED',writeOutcome:'NotApplied'},'Unknown'],
  ])('classifies status %s safely',async(status,problem,outcome)=>{
    respond(problem,status)
    await expect(saveWeekly('2031-06-11',body)).rejects.toMatchObject({outcome})
    expect(fetch).toHaveBeenCalledTimes(1)
  })
  it('network loss remains unknown and does not retry',async()=>{
    vi.stubGlobal('fetch',vi.fn().mockRejectedValue(new TypeError('network')))
    await expect(saveWeekly('2031-06-11',body)).rejects.toBeInstanceOf(CalendarWriteError)
    expect(fetch).toHaveBeenCalledTimes(1)
  })
  it('delegates auth teardown and does not reinterpret authentication as save success',async()=>{
    const handler=vi.fn();setUnauthorizedHandler(handler);respond(null,401)
    await expect(saveWeekly('2031-06-11',body)).rejects.toMatchObject({status:401})
    expect(handler).toHaveBeenCalledOnce()
  })
  it('binds a history page to its returned exact snapshot token',async()=>{
    respond({context,snapshotVersion:context.version,page:2,pageSize:20,totalCount:21,totalPages:2,items:[target],applicableBeforeFrom:null})
    expect((await getWeekly('2031-06-01','2031-06-30','History',2,context.version)).snapshotVersion).toBe(context.version)
    expect(String(vi.mocked(fetch).mock.calls[0]?.[0])).toContain('snapshotVersion='+context.version)
  })
  it('rejects incoherent snapshot metadata',async()=>{
    respond({context,snapshotVersion:'1',page:1,pageSize:20,totalCount:1,totalPages:1,items:[target],applicableBeforeFrom:null})
    await expect(getWeekly('2031-06-01','2031-06-30')).rejects.toThrow('CALENDAR_RESPONSE_INVALID')
  })
})

it('TC-380/385 rejects a structurally valid mutation target for a different logical date',async()=>{
 respond({context,changed:true,target:{...target,effectiveFrom:'2031-06-12'}})
 await expect(saveWeekly('2031-06-11',body)).rejects.toMatchObject({outcome:'Unknown'})
 expect(fetch).toHaveBeenCalledTimes(1)
})
it('TC-380 rejects a relabeled day response from another date',async()=>{
 respond({context,scope:{lineId:null,line:null},exception:null,day:{date:'2031-06-12',state:'Closed',hours:'0',hoursBasis:'Closed',source:{kind:'Weekly',revisionId:target.id,effectiveFrom:'2031-06-10',reason:null},fallback:[],editable:true}})
 await expect(getDay('2031-06-11')).rejects.toThrow('CALENDAR_RESPONSE_INVALID')
})
