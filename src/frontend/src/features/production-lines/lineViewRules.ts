import { useAuth } from '../auth/useAuth'

/** 「表示件数」 choices for the line list (WI-015 DEC-006), the same as 製品マスタ. */
export const linePageSizes = [10, 20, 50, 100] as const
export type LinePageSize = (typeof linePageSizes)[number]
export const defaultLinePageSize: LinePageSize = 20
/** Register/edit pages read product pairs and product choices 20 at a time (WI-015 DEC-008). */
export const formPageSize = 20

export function useLineRole() { return useAuth().user?.roles.some(role => role === 'Admin' || role === 'Operator') ?? false }
export function parseLineUrl(params: URLSearchParams) {
  const q = (params.get('q') ?? '').trim()
  const state = params.get('state') ?? 'active'
  const rawPage = params.get('page') ?? '1'
  const page = Number(rawPage)
  // An unknown or missing page size is replaced by the default rather than rejected (design §4 R4).
  const rawSize = params.get('pageSize')
  const pageSize = (linePageSizes as readonly number[]).includes(Number(rawSize)) && /^[0-9]+$/.test(rawSize ?? '') ? Number(rawSize) as LinePageSize : defaultLinePageSize
  const valid = [...params.keys()].every(key => ['q', 'state', 'page', 'pageSize'].includes(key) && params.getAll(key).length === 1) &&
    [...q].length <= 100 && ['active', 'retired', 'all'].includes(state) && /^[0-9]+$/.test(rawPage) && Number.isInteger(page) && page >= 1 && page <= 10000
  return { q, state, page, pageSize, valid, canonicalSize: rawSize === String(pageSize) }
}
