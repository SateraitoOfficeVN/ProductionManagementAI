import { fireEvent,render,screen,waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach,it,expect,vi } from 'vitest'
import { NavigationGuardProvider } from '../../../src/components/NavigationGuardProvider'
import { DateExceptionEditor } from '../../../src/features/plant-calendar/DateExceptionEditor'
import { CalendarWriteError,getDay,getExceptionHistory,saveException } from '../../../src/features/plant-calendar/api'
import { labels } from '../../../src/features/production-orders/messages'
vi.mock('../../../src/features/plant-calendar/api',async importOriginal=>({...await importOriginal<typeof import('../../../src/features/plant-calendar/api')>(),getDay:vi.fn(),getExceptionHistory:vi.fn(),saveException:vi.fn(),removeException:vi.fn()}))
const w=labels.calendar,context={plantToday:'2031-06-11',timeZone:'Asia/Tokyo',activatedOn:'2031-06-10',version:'1'}
const baseline={context,scope:{lineId:null,line:null},exception:null,day:{date:'2031-06-11',state:'Working' as const,hours:null,hoursBasis:'LineDependent' as const,source:{kind:'Weekly' as const,revisionId:'11111111-1111-1111-1111-111111111111',effectiveFrom:'2031-06-10',reason:null},fallback:[],editable:true}}
beforeEach(()=>{vi.clearAllMocks();vi.mocked(getDay).mockResolvedValue(baseline)})
const renderEditor=(onSaved=vi.fn())=>render(<MemoryRouter><NavigationGuardProvider><DateExceptionEditor date="2031-06-11" onSaved={onSaved} onCancel={vi.fn()} onAuth={vi.fn()}/></NavigationGuardProvider></MemoryRouter>)
it('TC-382 clears contradictory hours before closing and sends all nullable members',async()=>{
  vi.mocked(saveException).mockResolvedValue({context:{...context,version:'2'},changed:true,target:{id:'22222222-2222-2222-2222-222222222222',lineId:null,date:'2031-06-11',isWorking:false,workingHours:null,reason:null,isRemoved:false,isCurrent:true,commitRevision:'2',createdAtUtc:'2031-06-11T03:00:00Z'}})
  const saved=vi.fn();renderEditor(saved)
  const state=await screen.findByRole('combobox',{name:w.exception})
  fireEvent.change(state,{target:{value:'yes'}})
  fireEvent.click(screen.getByRole('checkbox',{name:w.inherit}))
  fireEvent.change(screen.getByLabelText(w.hours),{target:{value:'2.125'}})
  fireEvent.change(state,{target:{value:'no'}})
  expect(screen.queryByLabelText(w.hours)).not.toBeInTheDocument()
  fireEvent.click(screen.getByRole('button',{name:w.save}))
  await waitFor(()=>expect(saved).toHaveBeenCalledWith(true))
  expect(saveException).toHaveBeenCalledWith({version:'1',targetRevisionId:null,lineId:null,date:'2031-06-11',isWorking:false,workingHours:null,reason:null})
})
it('TC-385/386 unknown preserves the draft, blocks replay and failed observation cannot enable accept',async()=>{
  vi.mocked(saveException).mockRejectedValue(new CalendarWriteError('Unknown','CALENDAR_WRITE_UNKNOWN'))
  vi.mocked(getExceptionHistory).mockRejectedValue(new Error('network'))
  renderEditor()
  fireEvent.change(await screen.findByRole('combobox',{name:w.exception}),{target:{value:'no'}})
  fireEvent.change(screen.getByLabelText(w.reason),{target:{value:'kept draft'}})
  fireEvent.click(screen.getByRole('button',{name:w.save}))
  await screen.findByText(w.unknown)
  expect(screen.getByLabelText(w.reason)).toHaveValue('kept draft')
  expect(screen.getByRole('button',{name:w.save})).toBeDisabled()
  fireEvent.click(screen.getByRole('button',{name:w.verify}))
  await screen.findByText(w.readError)
  expect(screen.getByRole('button',{name:w.acceptCurrent})).toBeDisabled()
  expect(saveException).toHaveBeenCalledTimes(1)
})
it('TC-382 rejects invalid explicit precision and focuses the linked field without a request',async()=>{
  renderEditor();fireEvent.change(await screen.findByRole('combobox',{name:w.exception}),{target:{value:'yes'}})
  fireEvent.click(screen.getByRole('checkbox',{name:w.inherit}))
  const field=screen.getByLabelText(w.hours);fireEvent.change(field,{target:{value:'1.0000'}})
  fireEvent.click(screen.getByRole('button',{name:w.save}))
  expect(field).toHaveFocus();expect(field).toHaveAttribute('aria-invalid','true')
  expect(saveException).not.toHaveBeenCalled()
})
