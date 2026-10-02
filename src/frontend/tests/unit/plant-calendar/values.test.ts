import { describe,it,expect } from 'vitest'
import { dateValid,hoursValid,normalizeText,trimText,versionValid,parseCalendarUrl,formatExact,bodyWithinLimit } from '../../../src/features/plant-calendar/values'
describe('TC-366/367/368/377/379/388 exact calendar values',()=>{
  it.each(['0001-01-01','9999-12-31','2000-02-29','2024-02-29'])('accepts Gregorian date %s',date=>expect(dateValid(date)).toBe(true))
  it.each(['0000-01-01','1900-02-29','2026-02-29','2026-4-01','2026-04-31'])('rejects date %s',date=>expect(dateValid(date)).toBe(false))
  it.each(['0.001','24',' 2.125 ','\u00858\u0085'])('accepts exact hours %s',v=>expect(hoursValid(v)).toBe(true))
  it.each(['0','24.001','1.0000','1e1','+1','01','\uFEFF8'])('rejects hours %s',v=>expect(hoursValid(v)).toBe(false))
  it('counts scalars, preserves internal text and rejects malformed Unicode',()=>{
    expect(normalizeText(' 😀'.repeat(500),1000)).toBe('😀'+' 😀'.repeat(499))
    expect(normalizeText('😀'.repeat(500),500)).toBe('😀'.repeat(500))
    expect(normalizeText('😀'.repeat(501),500)).toBeUndefined()
    expect(normalizeText('\uD800',500)).toBeUndefined()
    expect(normalizeText('\uDC00',500)).toBeUndefined()
    expect(normalizeText(' \u0085 ',500)).toBeNull()
    expect(trimText('\uFEFFvalue\uFEFF')).toBe('\uFEFFvalue\uFEFF')
  })
  it('keeps bigint tokens exact and bounded',()=>{
    expect(versionValid('9223372036854775807')).toBe(true)
    expect(versionValid('9223372036854775808')).toBe(false)
    for(const token of ['0','01','1.0','1e1','-1',' 1']) expect(versionValid(token)).toBe(false)
  })
  it('rejects ambiguous URL and derives month solely from supplied date',()=>{
    for(const q of ['?month=2031-06&month=2031-07','?q=x','?month=2031-06&date=2031-07-01','?mode=day','?productId=11111111-1111-1111-1111-111111111111']) expect(parseCalendarUrl(q)).toBeNull()
    expect(parseCalendarUrl('?date=2031-06-11')).toEqual({month:'2031-06',date:'2031-06-11',mode:'month'})
    expect(parseCalendarUrl('')).toEqual({mode:'month'})
  })
  it('formats decimals without binary number conversion',()=>{
    expect(formatExact('1440000')).toBe('1,440,000')
    expect(formatExact('0.001')).toBe('0.001')
    expect(formatExact('9007199254740993.125')).toBe('9,007,199,254,740,993.125')
  })
  it('bounds the final UTF-8 JSON including escapes',()=>{
    expect(bodyWithinLimit({reason:'😀'.repeat(2048)})).toBe(false)
    expect(bodyWithinLimit({reason:'\\'.repeat(4096)})).toBe(false)
    expect(bodyWithinLimit({reason:'😀'.repeat(500)})).toBe(true)
  })
})
