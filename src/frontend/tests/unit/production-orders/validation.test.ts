import { describe, expect, it } from 'vitest'
import {
  codePointLength,
  localToday,
  validateDueDate,
  validateNotes,
  validateProduct,
  validateQuantity,
} from '../../../src/features/production-orders/validation'

describe('production order validation (BD-001 V-01–V-05)', () => {
  it('requires a product', () => {
    expect(validateProduct('')).toBe('MSG-E001')
    expect(validateProduct('some-id')).toBeNull()
  })

  it.each([
    ['1', null],
    ['250', null],
    [' 42 ', null],
    ['999999999', null],
    ['', 'MSG-E003'],
    ['0', 'MSG-E003'],
    ['-5', 'MSG-E003'],
    ['2.5', 'MSG-E003'],
    ['abc', 'MSG-E003'],
    ['1000000000', 'MSG-E010'],
  ])('quantity %j → %s', (input, expected) => {
    expect(validateQuantity(input)).toBe(expected)
  })

  it('requires a due date of today or later on create', () => {
    expect(validateDueDate('', null, '2026-09-18')).toBe('MSG-E004')
    expect(validateDueDate('2026-09-17', null, '2026-09-18')).toBe('MSG-E005')
    expect(validateDueDate('2026-09-18', null, '2026-09-18')).toBeNull()
  })

  it('lets an overdue order keep its past due date, but not move to another past date (DEC-009)', () => {
    expect(validateDueDate('2026-09-10', '2026-09-10', '2026-09-18')).toBeNull()
    expect(validateDueDate('2026-09-11', '2026-09-10', '2026-09-18')).toBe('MSG-E005')
  })

  it('counts notes in code points, so an emoji counts once', () => {
    const note = 'a'.repeat(499) + '😀'
    expect(note.length).toBe(501)
    expect(codePointLength(note)).toBe(500)
    expect(validateNotes(note)).toBeNull()
    expect(validateNotes('a'.repeat(501))).toBe('MSG-E006')
    expect(validateNotes(`  ${'a'.repeat(500)}  `)).toBeNull() // trimmed like the server
  })

  it('formats the browser-local date', () => {
    expect(localToday(new Date(2026, 0, 5, 23, 59))).toBe('2026-01-05')
  })
})
