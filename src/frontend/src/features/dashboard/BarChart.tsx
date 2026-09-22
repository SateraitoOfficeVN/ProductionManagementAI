import { useId, useRef, useState, type ComponentType } from 'react'
import { ExpandIcon, iconProps, TableIcon } from '../../components/icons'
import { ChartDialog } from './ChartDialog'

export interface Bar {
  label: string
  value: number
  /** Overdue and current-week bars: darker fill and bold label, never colour alone (BD-003 D-03). */
  emphasis?: boolean
}

export interface ChartProps {
  title: string
  Icon: ComponentType<typeof iconProps>
  bars: Bar[]
  /** The SVG's accessible name: every bar's value in one sentence. */
  summary: string
  caption: string
  tableColumns: string[]
  tableRows: (string | number)[][]
  /** Shown over an all-zero chart (workload: MSG-I007). */
  emptyMessage?: string
}

const SLOT = 60
const PLOT = 140

/** The SVG itself (DEC-010): one scale, a value printed on every bar, labels below. */
export function ChartSvg({ bars, summary, large }: { bars: Bar[]; summary: string; large?: boolean }) {
  const max = Math.max(0, ...bars.map((b) => b.value))
  const width = bars.length * SLOT
  return (
    <svg
      role="img"
      aria-label={summary}
      viewBox={`0 0 ${width} 184`}
      className={`block h-auto w-full ${large ? '' : 'min-w-[520px]'}`}
    >
      <line x1={0} y1={PLOT + 16} x2={width} y2={PLOT + 16} className="stroke-gray-300" strokeWidth={1} />
      {bars.map((bar, i) => {
        const h = bar.value === 0 || max === 0 ? 1 : Math.max(1, Math.round((PLOT * bar.value) / max))
        const x = i * SLOT + 14
        return (
          <g key={`${bar.label}-${i}`} aria-hidden="true">
            <rect x={x} y={PLOT + 16 - h} width={32} height={h} className={bar.emphasis ? 'fill-gray-900' : 'fill-gray-400'} />
            <text x={x + 16} y={PLOT + 10 - h} textAnchor="middle" className="fill-gray-900 text-[12px] tabular-nums">
              {bar.value}
            </text>
            <text x={x + 16} y={PLOT + 34} textAnchor="middle"
              className={`text-[11px] ${bar.emphasis ? 'fill-gray-900 font-semibold' : 'fill-gray-600'}`}>
              {bar.label}
            </text>
          </g>
        )
      })}
    </svg>
  )
}

export function ChartTable({ title, columns, rows }: { title: string; columns: string[]; rows: (string | number)[][] }) {
  return (
    <div className="overflow-x-auto rounded-md border border-gray-200">
      <table className="w-full text-sm">
        <caption className="sr-only">{title}</caption>
        <thead className="bg-gray-50 text-left text-gray-700">
          <tr>
            {columns.map((c, i) => (
              <th key={c} scope="col" className={`px-3 py-2 font-medium ${i >= 2 ? 'text-right' : ''}`}>{c}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, r) => (
            <tr key={r} className="border-t border-gray-100">
              {row.map((cell, i) => (
                <td key={i} className={`px-3 py-1.5 tabular-nums ${i >= 2 ? 'text-right' : ''}`}>{cell}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

/** Items 20–21 / 23–24 and Expand (27): card, "View as table" disclosure, maximized dialog (DD-003 modules 5, 13). */
export function BarChart(props: ChartProps) {
  const { title, Icon, bars, summary, caption, tableColumns, tableRows, emptyMessage } = props
  const [showTable, setShowTable] = useState(false)
  const [maximized, setMaximized] = useState(false)
  const expandButton = useRef<HTMLButtonElement>(null)
  const titleId = useId()
  const tableId = useId()
  const allZero = bars.every((b) => b.value === 0)

  return (
    <section aria-labelledby={titleId} className="grid min-w-0 content-start gap-2.5 rounded-lg border border-gray-200 bg-white p-3.5">
      <div className="flex items-center justify-between gap-2">
        <h2 id={titleId} className="flex items-center gap-1.5 text-sm font-semibold text-gray-900">
          <Icon {...iconProps} />
          {title}
        </h2>
        <button
          ref={expandButton}
          type="button"
          aria-label={`Expand ${title}`}
          onClick={() => setMaximized(true)}
          className="grid h-7 w-7 place-items-center rounded border border-gray-300 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
        >
          <ExpandIcon {...iconProps} />
        </button>
      </div>
      {/* On SP the chart keeps a readable width and scrolls inside its own card, never the page. */}
      <div className="relative overflow-x-auto" tabIndex={0} aria-label={`${title}, scrollable`}>
        <ChartSvg bars={bars} summary={summary} />
        {allZero && emptyMessage && (
          <p className="absolute inset-x-0 top-0 bottom-10 grid place-items-center text-sm text-gray-600">{emptyMessage}</p>
        )}
      </div>
      <p className="text-xs text-gray-500">{caption}</p>
      <button
        type="button"
        aria-expanded={showTable}
        aria-controls={tableId}
        onClick={() => setShowTable((v) => !v)}
        className="inline-flex items-center gap-1.5 justify-self-start rounded text-sm text-gray-900 underline underline-offset-4 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
      >
        <TableIcon {...iconProps} />
        {showTable ? 'Hide table' : 'View as table'}
      </button>
      <div id={tableId} hidden={!showTable}>
        {showTable && <ChartTable title={title} columns={tableColumns} rows={tableRows} />}
      </div>
      <ChartDialog
        open={maximized}
        chart={props}
        onClose={() => {
          setMaximized(false)
          expandButton.current?.focus()
        }}
      />
    </section>
  )
}
