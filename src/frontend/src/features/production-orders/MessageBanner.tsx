import type { ReactNode } from 'react'
import { labels } from './messages'

export interface Banner {
  kind: 'success' | 'error'
  text: string
  /** Shown on a stale-save conflict (MSG-E009): reloads the order and discards local edits. */
  onReload?: () => void
}

// 001_BD item 5. Success uses role="status", errors role="alert", so assistive tech announces them.
export function MessageBanner({ banner, children }: { banner: Banner | null; children?: ReactNode }) {
  if (!banner) {
    return null
  }

  const isError = banner.kind === 'error'
  return (
    <div
      role={isError ? 'alert' : 'status'}
      className={`flex flex-wrap items-center justify-between gap-3 rounded-md border px-3 py-2.5 text-sm ${
        isError ? 'border-red-200 bg-red-50 text-red-800' : 'border-green-200 bg-green-50 text-green-800'
      }`}
    >
      <span>{banner.text}</span>
      {banner.onReload && (
        <button
          type="button"
          onClick={banner.onReload}
          className="rounded border border-gray-300 bg-white px-2.5 py-1 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
        >
          {labels.common.reload}
        </button>
      )}
      {children}
    </div>
  )
}
