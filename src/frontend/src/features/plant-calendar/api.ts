import { ApiError,getJson,sendJson } from '../../lib/apiClient'
import { wellFormed,dateValid,monthValid,uuidValid,versionValid,hoursValid,decimalValid,weekdays,type Weekday } from './values'
export interface Context { plantToday:string;timeZone:string;activatedOn:string|null;version:string|null }
export interface Line { id:string;code:string;name:string;isActive:boolean;workingHoursPerDay:string }
export interface Product { id:string;sku:string;name:string;unit:string;unitRevision:string }
export interface Scope { lineId:string|null;line:Line|null }
export interface Source { kind:'LineException'|'PlantException'|'Weekly'|'None';revisionId:string|null;effectiveFrom:string|null;reason:string|null }
export interface Day { date:string;state:'Working'|'Closed'|'Unavailable';hours:string|null;hoursBasis:'Explicit'|'LineCurrent'|'LineDependent'|'Closed'|'Unavailable';source:Source;fallback:Source[];editable:boolean }
export interface Weekly { id:string;effectiveFrom:string;workingDays:Weekday[]|null;isWithdrawn:boolean;isCurrent:boolean;commitRevision:string;createdAtUtc:string }
export interface Exception { id:string;lineId:string|null;date:string;isWorking:boolean|null;workingHours:string|null;reason:string|null;isRemoved:boolean;isCurrent:boolean;commitRevision:string;createdAtUtc:string }
export interface MonthResult { context:Context;scope:Scope;month:string;days:Day[] }
export interface DayResult { context:Context;scope:Scope;day:Day;exception:Exception|null }
export interface Page<T> { context:Context;page:number;pageSize:number;totalCount:number;totalPages:number;items:T[] }
export interface WeeklyPage extends Page<Weekly> { snapshotVersion:string|null;applicableBeforeFrom:Weekly|null }
export interface ExceptionPage extends Page<Exception> { scope:Scope;date:string;snapshotVersion:string|null;current:Exception|null }
export type UnavailableReason = 'None'|'NotActivated'|'LineRetired'|'ProductRetired'|'PairMissing'|'PairRetired'|'UnitStale'|'CalendarUnavailable'
export interface Capacity { context:Context;line:Line;product:Product;date:string;availability:'Available'|'Unavailable';unavailableReason:UnavailableReason;day:Day;minutesPerUnit:string|null;quantity:string|null }
export interface Mutation<T> { context:Context;changed:boolean;target:T }
export interface WeeklySave { version:string;targetRevisionId:string|null;workingDays:Weekday[] }
export interface ExceptionSave { version:string;targetRevisionId:string|null;lineId:string|null;date:string;isWorking:boolean;workingHours:string|null;reason:string|null }
export interface ExceptionRemove { version:string;targetRevisionId:string;lineId:string|null;date:string }
export class CalendarWriteError extends Error {
  readonly outcome: 'NotApplied'|'Unknown'
  readonly code: string
  readonly retryAfter: number
  constructor(outcome:'NotApplied'|'Unknown',code:string,retryAfter=0) { super('CALENDAR_WRITE_UNCONFIRMED'); this.outcome=outcome; this.code=code; this.retryAfter=retryAfter }
}
const obj = (v:unknown):v is Record<string,unknown> => typeof v==='object' && v!==null && !Array.isArray(v)
const text = (v:unknown):v is string => typeof v==='string' && wellFormed(v)
const nil = <T,>(v:unknown,guard:(v:unknown)=>v is T):v is T|null => v===null || guard(v)
const bool = (v:unknown):v is boolean => typeof v==='boolean'
const utc = (v:unknown):v is string => text(v) && /^\d{4}-\d{2}-\d{2}T.*(?:Z|\+00:00)$/.test(v) && Number.isFinite(Date.parse(v))
const context = (v:unknown):v is Context => obj(v) && dateValid(v.plantToday) && text(v.timeZone) && v.timeZone.length>0 && nil(v.activatedOn,dateValid) && nil(v.version,versionValid) && (v.activatedOn===null)===(v.version===null)
const line = (v:unknown):v is Line => obj(v) && uuidValid(v.id) && text(v.code) && text(v.name) && bool(v.isActive) && text(v.workingHoursPerDay) && hoursValid(v.workingHoursPerDay)
const product = (v:unknown):v is Product => obj(v) && uuidValid(v.id) && text(v.sku) && text(v.name) && text(v.unit) && ['個','本','枚','台','セット','kg','m'].includes(v.unit) && versionValid(v.unitRevision,true)
const scope = (v:unknown):v is Scope => obj(v) && nil(v.lineId,uuidValid) && nil(v.line,line) && (v.lineId===null ? v.line===null : v.line?.id===v.lineId)
const source = (v:unknown):v is Source => obj(v) && text(v.kind) && ['LineException','PlantException','Weekly','None'].includes(v.kind) && nil(v.revisionId,uuidValid) && nil(v.effectiveFrom,dateValid) && nil(v.reason,text) && (v.kind==='None' ? v.revisionId===null && v.effectiveFrom===null && v.reason===null : uuidValid(v.revisionId) && (v.kind==='Weekly' ? dateValid(v.effectiveFrom) && v.reason===null : v.effectiveFrom===null))
const day = (v:unknown):v is Day => obj(v) && dateValid(v.date) && text(v.state) && ['Working','Closed','Unavailable'].includes(v.state) && nil(v.hours,decimalValid) && text(v.hoursBasis) && ['Explicit','LineCurrent','LineDependent','Closed','Unavailable'].includes(v.hoursBasis) && source(v.source) && Array.isArray(v.fallback) && v.fallback.length<=2 && v.fallback.every(source) && bool(v.editable) && (v.state==='Closed' ? v.hours==='0' && v.hoursBasis==='Closed' : v.state==='Unavailable' ? v.hours===null && v.hoursBasis==='Unavailable' : v.hoursBasis==='LineDependent' ? v.hours===null : text(v.hours) && hoursValid(v.hours))
const weekdayArray = (v:unknown):v is Weekday[] => Array.isArray(v) && v.length<=7 && new Set(v).size===v.length && v.every(d=>typeof d==='string' && weekdays.some(w=>w===d))
const weekly = (v:unknown):v is Weekly => obj(v) && uuidValid(v.id) && dateValid(v.effectiveFrom) && bool(v.isWithdrawn) && bool(v.isCurrent) && versionValid(v.commitRevision) && utc(v.createdAtUtc) && (v.isWithdrawn ? v.workingDays===null : weekdayArray(v.workingDays))
const exception = (v:unknown):v is Exception => obj(v) && uuidValid(v.id) && nil(v.lineId,uuidValid) && dateValid(v.date) && nil(v.isWorking,bool) && nil(v.workingHours,(h):h is string=>text(h)&&hoursValid(h)) && nil(v.reason,text) && bool(v.isRemoved) && bool(v.isCurrent) && versionValid(v.commitRevision) && utc(v.createdAtUtc) && (v.isRemoved ? v.isWorking===null && v.workingHours===null && v.reason===null : bool(v.isWorking) && (v.isWorking || v.workingHours===null))
const page = <T,>(v:unknown,size:number,guard:(v:unknown)=>v is T):v is Page<T> => obj(v) && context(v.context) && Number.isSafeInteger(v.page) && Number(v.page)>=1 && Number(v.page)<=10000 && v.pageSize===size && Number.isSafeInteger(v.totalCount) && Number(v.totalCount)>=0 && Number.isSafeInteger(v.totalPages) && v.totalPages===Math.ceil(Number(v.totalCount)/size) && Array.isArray(v.items) && v.items.length<=size && v.items.every(guard)
const month = (v:unknown):v is MonthResult => obj(v) && context(v.context) && scope(v.scope) && monthValid(v.month) && Array.isArray(v.days) && v.days.length>=28 && v.days.length<=31 && v.days.every((d,i)=>day(d)&&d.date===`${v.month}-${String(i+1).padStart(2,'0')}`) && !dateValid(`${v.month}-${String(v.days.length+1).padStart(2,'0')}`)
const detail = (v:unknown):v is DayResult => obj(v) && context(v.context) && scope(v.scope) && day(v.day) && nil(v.exception,exception) && (v.exception===null || v.exception.date===v.day.date && v.exception.lineId===v.scope.lineId)
const weeklyPage = (v:unknown):v is WeeklyPage => page(v,20,weekly) && obj(v) && nil(v.snapshotVersion,versionValid) && v.snapshotVersion===v.context.version && nil(v.applicableBeforeFrom,weekly)
const exceptionPage = (v:unknown):v is ExceptionPage => page(v,20,exception) && obj(v) && scope(v.scope) && dateValid(v.date) && nil(v.snapshotVersion,versionValid) && v.snapshotVersion===v.context.version && nil(v.current,exception)
const reasons:UnavailableReason[] = ['None','NotActivated','LineRetired','ProductRetired','PairMissing','PairRetired','UnitStale','CalendarUnavailable']
const capacity = (v:unknown):v is Capacity => obj(v) && context(v.context) && line(v.line) && product(v.product) && dateValid(v.date) && day(v.day) && v.day.date===v.date && text(v.unavailableReason) && reasons.some(r=>r===v.unavailableReason) && (v.availability==='Available' ? v.unavailableReason==='None' && decimalValid(v.quantity) && decimalValid(v.minutesPerUnit) && !/^0(?:\.0+)?$/.test(v.minutesPerUnit) : v.availability==='Unavailable' && v.unavailableReason!=='None' && v.quantity===null && v.minutesPerUnit===null)
const mutation = <T,>(v:unknown,target:(v:unknown)=>v is T):v is Mutation<T> => obj(v) && context(v.context) && versionValid(v.context.version) && bool(v.changed) && target(v.target) && obj(v.target) && v.target.isCurrent===true
function decode<T>(v:unknown,guard:(v:unknown)=>v is T):T { if(!guard(v)) throw new Error('CALENDAR_RESPONSE_INVALID');return v }
const deadline = (signal?:AbortSignal) => signal ? AbortSignal.any([signal,AbortSignal.timeout(25000)]) : AbortSignal.timeout(25000)
const query = (q:Record<string,string|undefined>) => new URLSearchParams(Object.entries(q).filter((e):e is [string,string]=>e[1]!==undefined)).toString()
const root='/api/plant-calendar'
const read = async <T,>(path:string,q:Record<string,string|undefined>,guard:(v:unknown)=>v is T,signal?:AbortSignal)=>decode(await getJson<unknown>(`${root}/${path}?${query(q)}`,deadline(signal)),guard)
export const getMonth = (value:string,lineId?:string,signal?:AbortSignal)=>read('month',{month:value,lineId},(v):v is MonthResult=>month(v)&&v.month===value&&v.scope.lineId===(lineId?.toLowerCase()??null),signal)
export const getDay = (date:string,lineId?:string,signal?:AbortSignal)=>read('day',{date,lineId},(v):v is DayResult=>detail(v)&&v.day.date===date&&v.scope.lineId===(lineId?.toLowerCase()??null),signal)
export const getWeekly = (from:string,to:string,view:'Current'|'History'='Current',currentPage=1,snapshotVersion?:string,signal?:AbortSignal)=>read('weekly',{from,to,view,page:String(currentPage),snapshotVersion},(v):v is WeeklyPage=>weeklyPage(v)&&v.items.every(row=>row.effectiveFrom>=from&&row.effectiveFrom<=to&&(view==='History'||row.isCurrent))&&(v.applicableBeforeFrom===null||v.applicableBeforeFrom.effectiveFrom<from&&!v.applicableBeforeFrom.isWithdrawn&&v.applicableBeforeFrom.isCurrent),signal)
export const getExceptionHistory = (date:string,lineId?:string,currentPage=1,snapshotVersion?:string,signal?:AbortSignal)=>read('exception-history',{date,lineId,page:String(currentPage),snapshotVersion},(v):v is ExceptionPage=>exceptionPage(v)&&v.date===date&&v.scope.lineId===(lineId?.toLowerCase()??null)&&v.items.every(row=>row.date===date&&row.lineId===v.scope.lineId)&&(v.current===null||v.current.date===date&&v.current.lineId===v.scope.lineId&&v.current.isCurrent),signal)
export const getLines = (q='',currentPage=1,signal?:AbortSignal)=>read('line-choices',{q,page:String(currentPage)},(v):v is Page<Line>=>page(v,50,line),signal)
export const getProducts = (lineId:string,q='',currentPage=1,signal?:AbortSignal)=>read('product-choices',{lineId,q,page:String(currentPage)},(v):v is Page<Product>=>page(v,50,product),signal)
export const getCapacity = (lineId:string,productId:string,date:string,signal?:AbortSignal)=>read('capacity',{lineId,productId,date},(v):v is Capacity=>capacity(v)&&v.line.id===lineId.toLowerCase()&&v.product.id===productId.toLowerCase()&&v.date===date,signal)
const knownCodes=new Set(['VALIDATION','REQUEST_TOO_LARGE','UNSUPPORTED_MEDIA_TYPE','NOT_FOUND','CALENDAR_STALE','CALENDAR_TARGET_CHANGED','CALENDAR_PAST_DATE','CALENDAR_LINE_RETIRED','CALENDAR_PROTECTED_RULE','CALENDAR_NOT_ACTIVATED','CALENDAR_BUSY','CALENDAR_TIMEZONE_MISMATCH'])
async function write<T>(method:'PUT'|'POST',path:string,body:unknown,target:(v:unknown)=>v is T):Promise<Mutation<T>> {
  try{return decode(await sendJson<unknown>(method,`${root}/${path}`,body,deadline()),(v):v is Mutation<T>=>mutation(v,target)&&obj(v.target)&&(v.changed?v.target.commitRevision===v.context.version:obj(body)&&v.context.version===body.version))}
  catch(error){
    if(error instanceof ApiError && (error.status===401 || error.status===403)) throw error
    if(error instanceof ApiError && obj(error.problem) && error.problem.writeOutcome==='NotApplied' && text(error.problem.code) && knownCodes.has(error.problem.code) && (error.status<500 || error.status===503 && ['CALENDAR_BUSY','CALENDAR_TIMEZONE_MISMATCH'].includes(error.problem.code))) throw new CalendarWriteError('NotApplied',error.problem.code,error.problem.code==='CALENDAR_BUSY'?1:0)
    throw new CalendarWriteError('Unknown','CALENDAR_WRITE_UNKNOWN')
  }
}
export const saveWeekly = (date:string,body:WeeklySave)=>write('PUT',`weekly/${encodeURIComponent(date)}`,body,(v):v is Weekly=>weekly(v)&&v.effectiveFrom===date&&!v.isWithdrawn)
export const withdrawWeekly = (date:string,body:{version:string;targetRevisionId:string})=>write('POST',`weekly/${encodeURIComponent(date)}/withdraw`,body,(v):v is Weekly=>weekly(v)&&v.effectiveFrom===date&&v.isWithdrawn)
export const saveException = (body:ExceptionSave)=>write('PUT','exceptions',body,(v):v is Exception=>exception(v)&&v.date===body.date&&v.lineId===body.lineId&&!v.isRemoved)
export const removeException = (body:ExceptionRemove)=>write('POST','exceptions/remove',body,(v):v is Exception=>exception(v)&&v.date===body.date&&v.lineId===body.lineId&&v.isRemoved)
