import { useEffect, useRef, type ReactNode } from 'react'
import { labels } from '../production-orders/messages'

export const lineInputClass = 'min-h-12 w-full rounded border border-gray-400 px-3 py-2 text-base focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900'
export const lineButtonClass = 'min-h-12 rounded border border-gray-400 px-4 py-2 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900 disabled:opacity-50'

/** Native modal confirms an explicit intent and restores the invoker after closing. */
export function LineDialog({ open, title, children, confirm, busy, confirmDisabled, onCancel, onConfirm }: {
  open: boolean; title: string; children: ReactNode; confirm: string; busy?: boolean; confirmDisabled?: boolean; onCancel: () => void; onConfirm: () => void
}) {
  const dialog = useRef<HTMLDialogElement>(null)
  const cancel = useRef<HTMLButtonElement>(null)
  const invoker = useRef<HTMLElement | null>(null)
  useEffect(() => {
    if (open && !dialog.current?.open) {
      invoker.current = document.activeElement instanceof HTMLElement ? document.activeElement : null
      dialog.current?.showModal()
      cancel.current?.focus()
    } else if (!open && dialog.current?.open) {
      dialog.current.close()
      if (invoker.current?.isConnected) invoker.current.focus()
    }
  }, [open])
  return <dialog ref={dialog} aria-labelledby="line-dialog-title" aria-describedby="line-dialog-description"
    onCancel={(event) => { event.preventDefault(); if (!busy) onCancel() }}
    onKeyDown={event => {
      if (event.key !== 'Tab') return
      const controls = Array.from(event.currentTarget.querySelectorAll<HTMLButtonElement>('button:not(:disabled)'))
      const first = controls[0]; const last = controls.at(-1)
      if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus() }
      else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first?.focus() }
    }}
    style={{ position: 'fixed', inset: 0, margin: 'auto', width: '480px', maxWidth: 'calc(100vw - 32px)', maxHeight: 'calc(100dvh - 32px)' }}
    className="overflow-auto rounded border border-gray-400 p-5 break-words shadow-xl backdrop:bg-black/40">
    <h2 id="line-dialog-title" className="text-lg font-semibold">{title}</h2>
    <div id="line-dialog-description" className="my-4">{children}</div>
    <div className="flex flex-wrap justify-end gap-2"><button ref={cancel} type="button" className={lineButtonClass} disabled={busy} onClick={onCancel}>{labels.lines.cancel}</button>
      <button type="button" disabled={busy || confirmDisabled} className={lineButtonClass} onClick={onConfirm}>{confirm}</button></div>
  </dialog>
}
