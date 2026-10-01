import { describe, expect, it } from 'vitest'
import { decimalValid, mutationWithinLimits } from '../../../src/features/production-lines/api'
import { parseLineUrl } from '../../../src/features/production-lines/lineViewRules'

describe('WI-009 exact decimal and bounded URL rules', () => {
  it.each(['1e1', '+1', '-1', '01', ' 1', '1 ', '1.2340', '0', '0.000', 'NaN', 'Infinity', '１', '24.001'])('rejects hours %s', value => expect(decimalValid(value, '24')).toBe(false))
  it.each(['0.001', '1', '7.500', '24.000'])('accepts exact hours %s', value => expect(decimalValid(value, '24')).toBe(true))
  it('preserves the large timing ceiling without floating-point comparison', () => {
    expect(decimalValid('999999999.999', '999999999.999')).toBe(true)
    expect(decimalValid('1000000000', '999999999.999')).toBe(false)
  })
  it('defaults to active page one and counts Unicode code points', () => {
    expect(parseLineUrl(new URLSearchParams())).toEqual({ q: '', state: 'active', page: 1, valid: true })
    expect(parseLineUrl(new URLSearchParams({ q: '😀'.repeat(100) })).valid).toBe(true)
    expect(parseLineUrl(new URLSearchParams({ q: '😀'.repeat(101) })).valid).toBe(false)
  })
  it.each(['page=0', 'page=10001', 'page=1&page=2', 'state=Active', 'pageSize=10', 'page=1.5', 'page=１'])('rejects malformed filters %s', query => expect(parseLineUrl(new URLSearchParams(query)).valid).toBe(false))
})

it('enforces aggregate count and exact UTF-8 body boundaries before sending', () => {
  expect(mutationWithinLimits({}, 1000)).toBe(true)
  expect(mutationWithinLimits({}, 1001)).toBe(false)
  expect(mutationWithinLimits('x'.repeat(256 * 1024 - 2), 1)).toBe(true)
  expect(mutationWithinLimits('x'.repeat(256 * 1024 - 1), 1)).toBe(false)
  expect(mutationWithinLimits('😀'.repeat(65536), 1)).toBe(false)
})
