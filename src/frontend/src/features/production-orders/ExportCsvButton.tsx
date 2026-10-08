import { useEffect, useRef, useState } from 'react'
import { ErrorIcon, ExportIcon, iconProps } from '../../components/icons'
import { ApiError } from '../../lib/apiClient'
import { saveFile } from '../../lib/download'
import { formatNumber } from '../../lib/format'
import { exportOrdersCsv } from './api'
import { exportHint, labels, message } from './messages'
import type { ListViewState } from './types'

// 002_DD-CSV module 1 / 002_DD-SPD-CSV P-16: items 28 (button), 30 (hint) and 29 (message). Rendered as two flex items
// of the list's result-header row: the button group sits before 「表示件数」 on PC and takes its own full-width row on
// SP; the message always takes a full row below. The parent re-mounts this component (key = view) whenever the applied
// view changes, which aborts an export in flight and clears the message (P-17).

/** Row limit per export (WI-016 DEC-005); the server enforces it too. */
const MAX_EXPORT_ROWS = 10_000

type Notice = { kind: 'success'; count: number } | { kind: 'error'; ids: string[] } | null

interface Props {
  /** The applied view, from the URL — the same one the list query used. */
  view: ListViewState
  /** The total of the list response on screen. */
  total: number
  /** A list query is in flight. */
  disabled?: boolean
}

export function ExportCsvButton({ view, total, disabled = false }: Props) {
  const [exporting, setExporting] = useState(false)
  const [notice, setNotice] = useState<Notice>(null)
  // Set synchronously, so a fast double activation cannot start a second request before the state re-renders.
  const busyRef = useRef(false)
  const controllerRef = useRef<AbortController | null>(null)

  useEffect(() => () => controllerRef.current?.abort(), [])

  const start = async () => {
    if (busyRef.current || disabled) {
      return
    }
    if (total > MAX_EXPORT_ROWS) {
      setNotice({ kind: 'error', ids: ['MSG-E024'] })
      return
    }

    busyRef.current = true
    setExporting(true)
    setNotice(null)
    const controller = new AbortController()
    controllerRef.current = controller
    try {
      const file = await exportOrdersCsv(view, controller.signal)
      saveFile(file.blob, file.fileName)
      setNotice({ kind: 'success', count: file.count })
    } catch (error: unknown) {
      if (controller.signal.aborted) {
        return
      }
      const ids = toMessageIds(error)
      if (ids) {
        setNotice({ kind: 'error', ids })
      }
    } finally {
      if (!controller.signal.aborted) {
        busyRef.current = false
        setExporting(false)
      }
    }
  }

  return (
    <>
      <div className="order-3 grid basis-full gap-1 sm:order-2 sm:ml-auto sm:basis-auto sm:justify-items-end">
        {/* Busy is aria-disabled, not disabled: a focused control that becomes disabled can drop keyboard focus to
            the page (WI-016 DEC-017). The real disabled attribute is only for a running list query. */}
        <button
          type="button"
          onClick={start}
          disabled={disabled}
          aria-disabled={exporting || undefined}
          aria-busy={exporting || undefined}
          aria-describedby="export-hint"
          className="inline-flex items-center justify-center gap-1.5 rounded border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none disabled:border-gray-200 disabled:text-gray-400 aria-disabled:border-gray-200 aria-disabled:bg-gray-50 aria-disabled:text-gray-400 max-sm:min-h-11 max-sm:w-full"
        >
          <ExportIcon {...iconProps} />
          {exporting ? labels.list.exporting : labels.list.exportCsv}
        </button>
        <p id="export-hint" className="text-xs text-gray-500">
          {exportHint(view, total)}
        </p>
      </div>
      <div className="order-4 basis-full">
        {/* Always mounted, so the first success text is announced (002_DD-CSV item 29). */}
        <p role="status" className="text-sm text-gray-700">
          {notice?.kind === 'success' ? message('MSG-I009', { count: formatNumber(notice.count) }) : null}
        </p>
        {notice?.kind === 'error' && (
          <p role="alert" className="flex items-center gap-1.5 text-sm text-red-600">
            <ErrorIcon {...iconProps} />
            {notice.ids.map((id) => message(id)).join(' ')}
          </p>
        )}
      </div>
    </>
  )
}

/** 002_DD-SPD-CSV P-16 step 7. Null for 401, which apiClient already handled by ending the session. */
function toMessageIds(error: unknown): string[] | null {
  if (error instanceof ApiError) {
    if (error.status === 401) {
      return null
    }
    if (error.status === 403) {
      return ['MSG-E020']
    }
    if (error.status === 422 && error.problem?.code === 'MSG-E024') {
      return ['MSG-E024']
    }
    if (error.status === 400 && error.problem?.errors) {
      return [...new Set(Object.values(error.problem.errors).flat())]
    }
  }
  return ['MSG-E025']
}
