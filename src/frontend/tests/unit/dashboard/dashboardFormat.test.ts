import { describe, expect, it } from 'vitest'
import {
  bucketLabel,
  formatInZone,
  formatLeadTime,
  formatOrders,
  formatRate,
  formatUnits,
  shownNote,
} from '../../../src/features/dashboard/dashboardFormat'

describe('dashboardFormat (BD-003 M-13, M-14, M-16–M-19)', () => {
  it('rounds the on-time rate half up to a whole percent, and shows — with nothing completed (TC-210)', () => {
    expect(formatRate(23, 30)).toBe('77%')
    expect(formatRate(1, 3)).toBe('33%')
    expect(formatRate(2, 3)).toBe('67%')
    expect(formatRate(1, 8)).toBe('13%') // 12.5 → 13
    expect(formatRate(0, 0)).toBe('—')
  })

  it('prints the lead time with one decimal, singular at exactly 1, — when absent (TC-212)', () => {
    expect(formatLeadTime(1.8)).toBe('1.8 days')
    expect(formatLeadTime(13)).toBe('13.0 days')
    expect(formatLeadTime(1)).toBe('1.0 day')
    expect(formatLeadTime(null)).toBe('—')
  })

  it('pluralises counts and uses thousands separators (M-18)', () => {
    expect(formatOrders(1)).toBe('1 order')
    expect(formatOrders(1234)).toBe('1,234 orders')
    expect(formatUnits(17922)).toBe('17,922 units')
  })

  it('labels workload buckets (M-16)', () => {
    expect(bucketLabel('overdue', null, 0)).toBe('Overdue')
    expect(bucketLabel('week', '2026-09-22', 1)).toBe('This week')
    expect(bucketLabel('week', '2026-09-28', 2)).toBe('09-28')
    expect(bucketLabel('later', null, 9)).toBe('Later')
  })

  it('shows instants in the plant zone, not the browser zone (M-17, M-20)', () => {
    expect(formatInZone('2026-09-22T05:05:12Z', 'Asia/Tokyo', 'dateTime')).toBe('2026-09-22 14:05')
    expect(formatInZone('2026-09-21T15:30:00Z', 'Asia/Tokyo', 'dateTime')).toBe('2026-09-22 00:30')
    expect(formatInZone('2026-09-22T05:05:12Z', 'Asia/Tokyo', 'time')).toBe('14:05:12')
  })

  it('notes hidden rows only when there are some (M-19)', () => {
    expect(shownNote(10, 15)).toBe('Showing 10 of 15')
    expect(shownNote(8, 8)).toBeNull()
  })
})
