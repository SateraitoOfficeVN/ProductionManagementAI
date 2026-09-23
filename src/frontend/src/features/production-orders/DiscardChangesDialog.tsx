import { useEffect, useRef } from 'react'
import { labels } from './messages'

interface Props {
  open: boolean
  onDiscard: () => void
  onKeepEditing: () => void
}

// 001_BD items 17–19 (REQ-019, DEC-018). The native <dialog> + showModal() gives the focus trap, Escape and
// backdrop for free; Escape fires "cancel", which is treated as {labels.order.keepEditing}. {labels.order.keepEditing} gets initial focus.
export function DiscardChangesDialog({ open, onDiscard, onKeepEditing }: Props) {
  const ref = useRef<HTMLDialogElement>(null)
  const keepEditing = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) {
      return
    }
    if (open && !dialog.open) {
      dialog.showModal()
      // Explicit: React doesn't emit the autofocus attribute (it focuses on mount, when the dialog is still closed).
      keepEditing.current?.focus()
    } else if (!open && dialog.open) {
      dialog.close()
    }
  }, [open])

  return (
    <dialog
      ref={ref}
      aria-labelledby="discard-title"
      aria-describedby="discard-body"
      onCancel={(event) => {
        event.preventDefault()
        onKeepEditing()
      }}
      className="m-auto w-[26rem] max-w-[calc(100%-2rem)] rounded-lg p-5 shadow-xl backdrop:bg-gray-900/45"
    >
      <h2 id="discard-title" className="text-base font-semibold text-gray-900">
        {labels.order.discardTitle}
      </h2>
      <p id="discard-body" className="mt-2 text-sm text-gray-600">
        {labels.order.discardBody}
      </p>
      <div className="mt-5 flex justify-end gap-2.5">
        <button
          ref={keepEditing}
          type="button"
          onClick={onKeepEditing}
          className="rounded border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
        >
          {labels.order.keepEditing}
        </button>
        <button
          type="button"
          onClick={onDiscard}
          className="rounded bg-red-700 px-4 py-2 text-sm text-white focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
        >
          {labels.order.discard}
        </button>
      </div>
    </dialog>
  )
}
