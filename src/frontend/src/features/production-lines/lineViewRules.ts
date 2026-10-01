import { useAuth } from '../auth/useAuth'

export function useLineRole() { return useAuth().user?.roles.some(role => role === 'Admin' || role === 'Operator') ?? false }
export function parseLineUrl(params: URLSearchParams) {
  const q = (params.get('q') ?? '').trim()
  const state = params.get('state') ?? 'active'
  const rawPage = params.get('page') ?? '1'
  const page = Number(rawPage)
  const valid = [...params.keys()].every(key => ['q', 'state', 'page'].includes(key) && params.getAll(key).length === 1) &&
    [...q].length <= 100 && ['active', 'retired', 'all'].includes(state) && /^[0-9]+$/.test(rawPage) && Number.isInteger(page) && page >= 1 && page <= 10000
  return { q, state, page, valid }
}
