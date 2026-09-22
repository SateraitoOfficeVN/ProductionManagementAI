// The only module that imports lucide-react (WI-004 DEC-023, BD-003 M-21). Icons are re-exported under role names,
// so changing a glyph is a one-line edit here. Every use is decorative: `aria-hidden` beside visible text, or inside a
// control that has its own accessible name.
export {
  Package as TopProductsIcon,
  Server as ServerIcon,
  Database as DatabaseIcon,
  LayoutDashboard as NavDashboardIcon,
  ClipboardList as NavOrdersIcon,
  Plus as NavNewOrderIcon,
  LogOut as SignOutIcon,
  Menu as MenuIcon,
  X as CloseIcon,
  Maximize2 as ExpandIcon,
  Minimize2 as RestoreIcon,
  RotateCw as RetryIcon,
  Table2 as TableIcon,
  Layers as StatusTotalIcon,
  FilePenLine as StatusDraftIcon,
  Clock as StatusInProgressIcon,
  CircleCheck as StatusCompletedIcon,
  CircleX as StatusCancelledIcon,
  CalendarCheck as CompletedWeekIcon,
  CalendarDays as CompletedMonthIcon,
  Target as OnTimeIcon,
  Timer as LeadTimeIcon,
  TriangleAlert as AttentionIcon,
  AlarmClock as OverdueIcon,
  CalendarClock as DueSoonIcon,
  ChartColumn as WorkloadIcon,
  TrendingUp as TrendIcon,
  Inbox as EmptyIcon,
  CircleAlert as ErrorIcon,
  ShieldX as ForbiddenIcon,
} from 'lucide-react'

/** Props every decorative icon gets: 16 px, currentColor, hidden from assistive technology. */
export const iconProps = { size: 16, 'aria-hidden': true, focusable: false } as const
