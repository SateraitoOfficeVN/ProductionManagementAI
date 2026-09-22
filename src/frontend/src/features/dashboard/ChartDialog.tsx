import { useEffect, useId, useRef } from 'react'
import { iconProps, RestoreIcon } from '../../components/icons'
import { ChartSvg, ChartTable, type ChartProps } from './BarChart'

// Items 28–29 (DD-003 module 13, DD-003-SPD §11, DEC-018). The native <dialog> + showModal() gives focus containment,
// Escape and an inert page behind it. It renders the snapshot already in memory: no request.
export function ChartDialog({ open, chart, onClose }: { open: boolean; chart: ChartProps; onClose: () => void }) {
  const ref = useRef<HTMLDialogElement>(null)
  const restore = useRef<HTMLButtonElement>(null)
  const titleId = useId()

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (open && !dialog.open) {
      dialog.showModal()
      restore.current?.focus()
    } else if (!open && dialog.open) {
      dialog.close()
    }
  }, [open])

  return (
    <dialog
      ref={ref}
      aria-labelledby={titleId}
      onCancel={(event) => {
        event.preventDefault()
        onClose()
      }}
      className="m-0 h-dvh max-h-none w-screen max-w-none bg-white p-0 backdrop:bg-gray-900/45 sm:m-auto sm:h-[calc(100dvh-2rem)] sm:w-[calc(100vw-2rem)] sm:rounded-lg"
    >
      {open && (
        <div className="grid h-full grid-rows-[auto_1fr]">
          <div className="flex items-center justify-between gap-3 border-b border-gray-200 px-4 py-3">
            <h2 id={titleId} className="flex items-center gap-1.5 text-base font-semibold text-gray-900">
              <chart.Icon {...iconProps} />
              {chart.title}
            </h2>
            <button
              ref={restore}
              type="button"
              onClick={onClose}
              className="inline-flex items-center gap-1.5 rounded border border-gray-300 px-3 py-1.5 text-sm text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
            >
              <RestoreIcon {...iconProps} />
              Restore
            </button>
          </div>
          <div className="grid content-start gap-4 overflow-auto p-4 lg:grid-cols-[2fr_1fr]">
            <div className="grid content-start gap-2">
              <ChartSvg bars={chart.bars} summary={chart.summary} large />
              <p className="text-xs text-gray-500">{chart.caption}</p>
            </div>
            <ChartTable title={chart.title} columns={chart.tableColumns} rows={chart.tableRows} />
          </div>
        </div>
      )}
    </dialog>
  )
}
