import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, it, vi } from 'vitest'
import { OrderLinePicker } from '../../../src/features/production-lines/OrderLinePicker'

const product = { id: '0197e4a0-0000-7000-8000-000000001001', sku: 'P1', name: '製品', unit: 'kg', unitRevision: '0', isActive: true }
const line = { id: '0197e4a0-0000-7000-8000-000000009001', code: 'OLD-LINE', name: '履歴', workingHoursPerDay: '8', isActive: false, updatedAt: '2026-10-01T00:00:00Z', version: '1', minutesPerUnit: '1', unit: 'kg' }
const reply = (items: typeof line[] = []) => new Response(JSON.stringify({ product, items, total: items.length, page: 1, pageSize: 50 }), { headers: { 'Content-Type': 'application/json' } })
afterEach(() => vi.unstubAllGlobals())

it('keeps historical selection independently of empty or filtered candidates and locks all non-Draft controls', async () => {
  const user = userEvent.setup(); const change = vi.fn(); const forbidden = vi.fn()
  vi.stubGlobal('fetch', vi.fn(async () => reply()))
  const view = render(<OrderLinePicker productId={product.id} selected={line} locked={false} pending={false} onChange={change} onForbidden={forbidden} />)
  await screen.findByText('選択できる生産ラインはありません。')
  expect(screen.getByText('OLD-LINE — 履歴 (使用停止)')).toBeInTheDocument()
  await user.type(screen.getByLabelText('コード・名称で検索'), 'absent')
  await user.click(screen.getByRole('button', { name: '検索' }))
  expect(change).not.toHaveBeenCalled()
  view.rerender(<OrderLinePicker productId={product.id} selected={line} locked pending={false} onChange={change} onForbidden={forbidden} />)
  expect(screen.queryByRole('button')).not.toBeInTheDocument()
  expect(screen.getByText('OLD-LINE — 履歴 (使用停止)')).toBeInTheDocument()
})

it('ignores an obsolete old-product response and never restores a cleared selection', async () => {
  let resolveOld: ((value: Response) => void) | undefined
  const change = vi.fn(); const forbidden = vi.fn()
  vi.stubGlobal('fetch', vi.fn((url: string) => url.includes(product.id) ? new Promise<Response>(resolve => { resolveOld = resolve }) : Promise.resolve(reply())))
  const view = render(<OrderLinePicker productId={product.id} selected={null} locked={false} pending={false} onChange={change} onForbidden={forbidden} />)
  await waitFor(() => expect(resolveOld).toBeDefined())
  view.rerender(<OrderLinePicker productId="0197e4a0-0000-7000-8000-000000001002" selected={null} locked={false} pending={false} onChange={change} onForbidden={forbidden} />)
  await screen.findByText('選択できる生産ラインはありません。')
  resolveOld?.(reply([{ ...line, isActive: true }]))
  await waitFor(() => expect(screen.queryByText(/OLD-LINE/)).not.toBeInTheDocument())
  expect(change).not.toHaveBeenCalled()
})

it('suppresses selection and clear while saving and reports a forbidden read without candidates', async () => {
  const change = vi.fn(); const forbidden = vi.fn()
  vi.stubGlobal('fetch', vi.fn(async () => reply([{ ...line, isActive: true }])))
  const view = render(<OrderLinePicker productId={product.id} selected={line} locked={false} pending onChange={change} onForbidden={forbidden} />)
  expect(await screen.findByRole('button', { name: '選択中' })).toBeDisabled()
  expect(screen.getByRole('button', { name: 'クリア', exact: true })).toBeDisabled()
  view.unmount()
  vi.stubGlobal('fetch', vi.fn(async () => new Response(null, { status: 403 })))
  render(<OrderLinePicker productId={product.id} selected={null} locked={false} pending={false} onChange={change} onForbidden={forbidden} />)
  await waitFor(() => expect(forbidden).toHaveBeenCalledOnce())
  expect(screen.queryByRole('button', { name: '選択' })).not.toBeInTheDocument()
})
