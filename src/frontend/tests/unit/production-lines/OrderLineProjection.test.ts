import { afterEach, expect, it, vi } from 'vitest'
import { getOrder, listOrders } from '../../../src/features/production-orders/api'
import { defaultViewState } from '../../../src/features/production-orders/listViewState'

afterEach(() => vi.unstubAllGlobals())
it.each([{}, { line: undefined }, { line: {} }, { line: { id: 'id', code: 'L', name: 'Line', isActive: 'true' } }])('rejects missing or malformed assignment metadata instead of showing unassigned history: %j', async value => {
  vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(value), { headers: { 'Content-Type': 'application/json' } })))
  await expect(getOrder('id')).rejects.toThrow('INVALID_ORDER_LINE_PROJECTION')
})
it.each([null, { id: 'id', code: 'L', name: 'Line', isActive: false }])('preserves actual null or retired assignment metadata: %j', async line => {
  vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({ line }), { headers: { 'Content-Type': 'application/json' } })))
  expect((await getOrder('id')).line).toEqual(line)
})
it('rejects a list row missing line metadata without fabricating cards', async () => {
  vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({ items: [{}] }), { headers: { 'Content-Type': 'application/json' } })))
  await expect(listOrders(defaultViewState)).rejects.toThrow('INVALID_ORDER_LINE_PROJECTION')
})
