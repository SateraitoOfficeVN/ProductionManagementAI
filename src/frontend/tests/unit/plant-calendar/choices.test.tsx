import { render,screen,fireEvent } from '@testing-library/react'
import { describe,it,expect,vi } from 'vitest'
import { ChoiceSearch } from '../../../src/features/plant-calendar/ChoiceSearch'
import { labels } from '../../../src/features/production-orders/messages'
const w=labels.calendar
const props={title:w.productSearch,value:'',onChange:vi.fn(),onSearch:vi.fn(),onClear:vi.fn(),page:1,totalPages:1,totalCount:30,onPage:vi.fn()}
describe('TC-411 bounded choice disclosure',()=>{
 it('omits pagination for a single page and retains explicit search/clear actions',()=>{
  const {container}=render(<ChoiceSearch {...props}/>)
  expect(container.querySelector('details')).not.toHaveAttribute('open')
  expect(screen.queryByRole('button',{name:w.previousPage,hidden:true})).not.toBeInTheDocument()
  expect(screen.queryByRole('button',{name:w.nextPage,hidden:true})).not.toBeInTheDocument()
  expect(screen.getByText(w.productSearch)).toBeInTheDocument()
 })
 it('exposes bounded paging only inside its matching expanded group',()=>{
  const onPage=vi.fn();const {container}=render(<ChoiceSearch {...props} totalPages={2} totalCount={51} onPage={onPage}/>)
  const details=container.querySelector('details');details?.setAttribute('open','')
  expect(screen.getByRole('button',{name:w.previousPage})).toBeDisabled()
  fireEvent.click(screen.getByRole('button',{name:w.nextPage}))
  expect(onPage).toHaveBeenCalledWith(2)
  fireEvent.click(screen.getByRole('button',{name:w.clearSearch}))
  expect(props.onClear).toHaveBeenCalled()
 })
})
