import { ApiError } from '../../lib/apiClient'
import { labels } from '../production-orders/messages'
import { CalendarWriteError,type UnavailableReason } from './api'
import { dateValid } from './values'
const w=labels.calendar
export function errorText(error:unknown):string {
  const code=error instanceof CalendarWriteError?error.code:error instanceof ApiError?error.problem?.code:undefined
  if(error instanceof CalendarWriteError && error.outcome==='Unknown')return w.unknown
  if(error instanceof ApiError && error.status===403)return w.forbidden
  switch(code){
    case 'VALIDATION':return w.validation
    case 'REQUEST_TOO_LARGE':case 'UNSUPPORTED_MEDIA_TYPE':return w.requestError
    case 'NOT_FOUND':return w.notFound
    case 'CALENDAR_STALE':case 'CALENDAR_TARGET_CHANGED':return w.conflict
    case 'CALENDAR_PAST_DATE':return w.pastLocked
    case 'CALENDAR_LINE_RETIRED':return w.retiredLocked
    case 'CALENDAR_PROTECTED_RULE':return w.protectedRule
    case 'CALENDAR_NOT_ACTIVATED':return w.notActivated
    case 'CALENDAR_BUSY':return w.busy
    case 'CALENDAR_TIMEZONE_MISMATCH':return w.timeZoneError
    case 'UNEXPECTED':return w.unexpected
    default:return w.readError
  }
}
export function unavailableText(reason:UnavailableReason):string {
  switch(reason){
    case 'NotActivated':return w.notActivated
    case 'LineRetired':case 'ProductRetired':case 'PairRetired':return w.referenceStopped
    case 'PairMissing':return w.noTiming
    case 'UnitStale':return w.unitStale
    case 'CalendarUnavailable':return w.noCalendar
    default:return w.capacityUnavailable
  }
}
export function monthEnd(month:string):string {
  for(let n=31;n>=28;n--){const date=month+'-'+n;if(dateValid(date))return date}
  throw new Error('CALENDAR_MONTH_INVALID')
}
export function shiftMonth(month:string,step:-1|1):string|null {
  const [year,m]=month.split('-').map(Number),index=(year-1)*12+m-1+step
  if(index<0 || index>=9999*12)return null
  return String(Math.floor(index/12)+1).padStart(4,'0')+'-'+String(index%12+1).padStart(2,'0')
}
