import { afterEach, expect, it, vi } from 'vitest'
import { createOrder } from '../../../src/features/production-orders/api'

afterEach(() => vi.unstubAllGlobals())

it('sends the validated decimal text as an unquoted JSON number token', async () => {
  let sent = ''
  vi.stubGlobal('fetch', vi.fn(async (_url: string, init: RequestInit) => {
    sent = init.body as string
    return new Response('{"line":null}', { status: 200, headers: { 'Content-Type': 'application/json' } })
  }))
  await createOrder({ productId: 'p1', quantity: '0.001', dueDate: '2099-01-01', notes: null })
  expect(sent).toContain('"quantity":0.001')
  expect(sent).not.toContain('"quantity":"0.001"')
  expect(() => JSON.parse(sent)).not.toThrow()
})

it('rejects unsupported quantity spelling before any network request', async () => {
  const fetch = vi.fn()
  vi.stubGlobal('fetch', fetch)
  expect(() => createOrder({ productId: 'p1', quantity: '1.2340', dueDate: '2099-01-01', notes: null }))
    .toThrow('QUANTITY_UNIT_INVALID')
  expect(fetch).not.toHaveBeenCalled()
})

it('preserves large nested quantity subtotals and escaped text without rounding', async () => {
  const { parseQuantityJson } = await import('../../../src/lib/apiClient')
  const { formatNumber } = await import('../../../src/lib/format')
  const parsed = parseQuantityJson<{ unitQuantities: { quantity: string }[]; openQuantity: string; count: number; name: string }>(
    '{"unitQuantities":[{"quantity":9007199254740.991}],"openQuantity":999999999.001,"count":2,"name":"quantity: 1.234"}',
  )
  expect(parsed.unitQuantities[0].quantity).toBe('9007199254740.991')
  expect(formatNumber(parsed.unitQuantities[0].quantity)).toBe('9,007,199,254,740.991')
  expect(parsed.openQuantity).toBe('999999999.001')
  expect(parsed.count).toBe(2)
  expect(parsed.name).toBe('quantity: 1.234')
  expect(formatNumber('1.230')).toBe('1.23')
})
