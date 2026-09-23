/** 003_BD H-2: the navbar entry for the current route. The edit route belongs to "Production orders". */
export function currentEntry(pathname: string): string | null {
  if (pathname === '/') return '/'
  if (pathname === '/production-orders/new') return '/production-orders/new'
  if (pathname === '/production-orders' || pathname.startsWith('/production-orders/')) return '/production-orders'
  return null
}
