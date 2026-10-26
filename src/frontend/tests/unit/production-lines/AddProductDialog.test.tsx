import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, it, vi } from 'vitest'
import { AddProductDialog } from '../../../src/features/production-lines/AddProductDialog'

// WI-018 BUG-011: a reopened dialog must not reload its previous search, and an older load must never replace a newer search.
const p1001 = { id: '0197e4a0-0000-7000-8000-000000001001', sku: 'P-1001', name: 'ブレーキキャリパー', unit: '個', unitRevision: '0', isActive: true }
const p1002 = { id: '0197e4a0-0000-7000-8000-000000001002', sku: 'P-1002', name: 'ブレーキディスクローター', unit: '枚', unitRevision: '0', isActive: true }

function stubChoices(delayFor: (q: string) => Promise<void> = async () => {}) {
  const queries: string[] = []
  vi.stubGlobal('fetch', vi.fn(async (url: string) => {
    const q = new URLSearchParams(url.split('?')[1]).get('q') ?? ''
    queries.push(q)
    await delayFor(q)
    const items = q === '' ? [p1001, p1002] : [p1001, p1002].filter(p => p.sku.includes(q))
    return new Response(JSON.stringify({ items, total: items.length, page: 1, pageSize: 20 }),
      { status: 200, headers: { 'Content-Type': 'application/json' } })
  }))
  return queries
}

const props = { onAdd: vi.fn(), onClose: vi.fn(), onForbidden: vi.fn() }

afterEach(() => vi.unstubAllGlobals())

it('reopens with an empty search and loads only the unfiltered list', async () => {
  const user = userEvent.setup()
  const queries = stubChoices()
  const { rerender } = render(<AddProductDialog open excluded={new Set()} {...props} />)
  await screen.findByText(/P-1002/)
  await user.type(screen.getByLabelText('製品を検索'), 'P-1001')
  await user.click(screen.getByRole('button', { name: '検索', hidden: true }))
  await waitFor(() => expect(queries.at(-1)).toBe('P-1001'))

  rerender(<AddProductDialog open={false} excluded={new Set([p1001.id])} {...props} />)
  const before = queries.length
  rerender(<AddProductDialog open excluded={new Set([p1001.id])} {...props} />)
  await screen.findByText(/P-1002/)
  expect(screen.getByLabelText('製品を検索')).toHaveValue('')
  expect(queries.slice(before)).toEqual([''])
})

it('keeps a newer search when an older load answers last', async () => {
  const user = userEvent.setup()
  let release!: () => void
  const slowAll = new Promise<void>(resolve => { release = resolve })
  let first = true
  stubChoices(async q => { if (q === '' && !first) await slowAll; if (q === '') first = false })
  const { rerender } = render(<AddProductDialog open excluded={new Set()} {...props} />)
  await screen.findByText(/P-1002/)
  rerender(<AddProductDialog open={false} excluded={new Set([p1001.id])} {...props} />)
  rerender(<AddProductDialog open excluded={new Set([p1001.id])} {...props} />)
  await user.type(screen.getByLabelText('製品を検索'), 'P-1001')
  await user.click(screen.getByRole('button', { name: '検索', hidden: true }))
  expect(await screen.findByText('選択できる製品はありません。')).toBeInTheDocument()
  release()
  await new Promise(resolve => setTimeout(resolve, 20))
  expect(screen.getByText('選択できる製品はありません。')).toBeInTheDocument()
  expect(screen.queryByText(/P-1002/)).not.toBeInTheDocument()
})
