import { fireEvent,render,screen,waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach,it,expect,vi } from 'vitest'
import { NavigationGuardProvider } from '../../../src/components/NavigationGuardProvider'
import { WeeklyPatternEditor } from '../../../src/features/plant-calendar/WeeklyPatternEditor'
import { CalendarWriteError,getWeekly,saveWeekly,type WeeklyPage } from '../../../src/features/plant-calendar/api'
import { labels } from '../../../src/features/production-orders/messages'
vi.mock('../../../src/features/plant-calendar/api',async importOriginal=>({...await importOriginal<typeof import('../../../src/features/plant-calendar/api')>(),getWeekly:vi.fn(),saveWeekly:vi.fn()}))
const w=labels.calendar
const baseline:WeeklyPage={context:{plantToday:'2031-06-11',timeZone:'Asia/Tokyo',activatedOn:'2031-06-10',version:'1'},snapshotVersion:'1',page:1,pageSize:20,totalCount:1,totalPages:1,applicableBeforeFrom:null,items:[{id:'11111111-1111-1111-1111-111111111111',effectiveFrom:'2031-06-11',workingDays:['Mon'],isWithdrawn:false,isCurrent:true,commitRevision:'1',createdAtUtc:'2031-06-11T00:00:00Z'}]}
beforeEach(()=>{vi.clearAllMocks();vi.mocked(getWeekly).mockResolvedValue(baseline)})
it.each([false,true])('TC-381/386 list refresh preserves the original weekly draft and settlement (unknown=%s)',async unknown=>{
  vi.mocked(saveWeekly).mockRejectedValue(new CalendarWriteError('Unknown','CALENDAR_WRITE_UNKNOWN'))
  render(<MemoryRouter><NavigationGuardProvider><WeeklyPatternEditor month="2031-06" date="2031-06-11" onSelect={vi.fn()} onSaved={vi.fn()} onCancel={vi.fn()} onAuth={vi.fn()}/></NavigationGuardProvider></MemoryRouter>)
  const monday=await screen.findByRole('checkbox',{name:w.weekdays.Mon})
  fireEvent.click(monday)
  expect(monday).not.toBeChecked()
  if(unknown){fireEvent.click(screen.getByRole('button',{name:w.save}));await screen.findByText(w.unknown)}
  const calls=vi.mocked(getWeekly).mock.calls.length
  screen.getByText(w.weeklyHistory).closest('details')?.setAttribute('open','')
  fireEvent.click(screen.getByRole('button',{name:w.restart}))
  await waitFor(()=>expect(vi.mocked(getWeekly).mock.calls.length).toBe(calls+1))
  expect(screen.getByRole('checkbox',{name:w.weekdays.Mon})).not.toBeChecked()
  expect(vi.mocked(getWeekly).mock.calls.filter(call=>call[0]==='2031-06-11'&&call[1]==='2031-06-11')).toHaveLength(1)
  if(unknown){expect(screen.getByRole('button',{name:w.save})).toBeDisabled();expect(screen.getByText(w.unknown)).toBeInTheDocument();expect(saveWeekly).toHaveBeenCalledTimes(1)}
  else expect(screen.getByRole('button',{name:w.save})).toBeEnabled()
})
