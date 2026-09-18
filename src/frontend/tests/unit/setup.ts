import '@testing-library/jest-dom/vitest'
import 'vitest-axe/extend-expect'
import * as axeMatchers from 'vitest-axe/matchers'
import { expect } from 'vitest'

expect.extend(axeMatchers)

// jsdom has HTMLDialogElement but no showModal/close; a minimal stand-in so DiscardChangesDialog can be tested.
// Real browsers provide the native focus trap and Escape handling this component relies on.
if (typeof HTMLDialogElement !== 'undefined' && !HTMLDialogElement.prototype.showModal) {
  HTMLDialogElement.prototype.showModal = function showModal(this: HTMLDialogElement) {
    this.setAttribute('open', '')
  }
  HTMLDialogElement.prototype.close = function close(this: HTMLDialogElement) {
    this.removeAttribute('open')
    this.dispatchEvent(new Event('close'))
  }
}
