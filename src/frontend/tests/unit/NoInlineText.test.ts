// TC-301 (WI-005 REQ-043, DEC-007): every word the user sees comes from the catalog in messages.ts, so no component
// or formatter carries inline English text. A source scan, so a new inline string fails here before it reaches a screen.

const sources = import.meta.glob(['../../src/**/*.{ts,tsx}', '!../../src/**/messages.ts'], {
  query: '?raw',
  import: 'default',
  eager: true,
}) as Record<string, string>

// Comments may be English; only code is scanned.
const stripComments = (code: string) => code.replace(/\/\*[\s\S]*?\*\//g, '').replace(/^\s*\/\/.*$/gm, '')

const patterns: [string, RegExp][] = [
  // Not `=> Promise<` (a return type): JSX text follows a tag, never an arrow.
  ['JSX text', /(?<![=-])>\s*[A-Za-z][A-Za-z']*(?: [A-Za-z']+)*[.…?!]?\s*</],
  ['text attribute', /\b(?:aria-label|title|placeholder|label|alt|hint|caption|summary)="[^"]*[A-Za-z][a-z]+[^"]*"/], // a word, not a code like PO-2026-… (DEC-005)
  ['sentence in quotes', /['`"][A-Z][a-z]+(?: [a-z]+)+[.…?!]?['`"]/],
]

describe('No inline UI text outside the catalog (TC-301)', () => {
  it('scans the frontend sources', () => {
    expect(Object.keys(sources).length).toBeGreaterThan(20)
  })

  it.each(Object.entries(sources))('%s has no inline English UI text', (_, code) => {
    const findings = patterns.flatMap(([kind, pattern]) => {
      const match = stripComments(code).match(pattern)
      return match ? [`${kind}: ${match[0]}`] : []
    })
    expect(findings).toEqual([])
  })
})
