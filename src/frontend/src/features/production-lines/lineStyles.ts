// WI-015 (005_DD-SPD-REDESIGN §3): SCR-005 reuses the 製品マスタ visual vocabulary. Filled buttons are 40 px and
// outlined buttons and fields 42 px, matching the order screens (DEC-002). Order-assignment controls keep their own
// styles in LineDialog.tsx.

const focusRing = 'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900'
const buttonBase = `inline-flex items-center justify-center rounded px-4 py-2 text-center text-base font-bold whitespace-nowrap disabled:opacity-50 ${focusRing}`

export const primaryButton = `${buttonBase} bg-[#1f5fa8] text-white`
export const secondaryButton = `${buttonBase} border border-[#1f5fa8] bg-white text-[#1f5fa8]`
export const dangerButton = `${buttonBase} bg-[#a33232] text-white`
export const pagerButton = `rounded border border-[#1f5fa8] bg-white px-3 py-1.5 text-sm font-bold text-[#1f5fa8] disabled:border-gray-200 disabled:text-gray-400 ${focusRing}`
export const inputClass = `w-full min-w-0 rounded border border-[#8ea0b5] bg-white px-3 py-2 text-base text-[#172334] aria-[invalid=true]:border-[#b73838] ${focusRing}`
export const readOnlyClass = 'flex w-full min-w-0 items-center gap-3 rounded border border-[#8ea0b5] bg-[#eef2f6] px-3 text-[#536475] focus-within:outline-2 focus-within:outline-offset-2 focus-within:outline-gray-900'
export const readOnlyInput = 'min-w-0 flex-1 bg-transparent py-2 text-base outline-none'
export const labelClass = 'block text-sm font-bold'
export const hintClass = 'mt-1 text-sm text-[#536475]'
export const fieldError = 'mt-1 text-sm font-bold text-[#b73838]'
export const errorBox = 'rounded-lg border border-[#b73838] bg-[#fff0f0] p-3.5 [overflow-wrap:anywhere]'
export const successBox = 'rounded-lg border border-[#407a54] bg-[#eef8f0] p-3.5'
export const infoBox = 'rounded-lg border border-[#c3ccd7] bg-[#f6f8fb] p-3.5'
export const warnBox = 'rounded-lg border border-[#b7791f] bg-[#fff8e6] p-3.5 [overflow-wrap:anywhere]'
export const sectionClass = 'grid min-w-0 gap-4 rounded-lg border border-[#d5dde6] p-4 sm:p-5'
export const headerCell = 'sticky top-0 z-[1] whitespace-nowrap border-b border-[#e2e8ef] bg-[#f6f8fb] px-2.5 py-3 text-left font-bold text-[#485c74]'
export const cell = 'border-b border-[#e2e8ef] px-2.5 py-3 align-middle'
// DEC-009: every list scrolls inside a bounded box; count, page size and pager stay outside it.
export const scrollBox = 'min-w-0 overflow-y-auto rounded border border-[#d5dde6] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900'
export const lineListHeight = 'max-h-[min(60vh,640px)]'
export const pairListHeight = 'max-h-[min(50vh,560px)]'
export const choiceListHeight = 'max-h-[min(45vh,400px)]'

export const badgeClass = {
  active: 'bg-[#e6f4ec] text-[#155833]',
  retired: 'bg-[#edf0f4] text-[#4d5a6a]',
  added: 'bg-[#e8f0fb] text-[#1f5fa8]',
  pending: 'bg-[#fff6dc] text-[#7a4d00]',
} as const

export const badge = (kind: keyof typeof badgeClass) => `inline-block rounded-full px-2.5 py-0.5 text-sm font-bold whitespace-nowrap ${badgeClass[kind]}`
