import { getJson } from '../../lib/apiClient'
import type { DashboardSnapshot, SystemHealth } from './types'

/** 003_DD-API §1 — one snapshot for every widget. */
export const getDashboard = (signal?: AbortSignal) => getJson<DashboardSnapshot>('/api/dashboard', signal)

/** 003_DD-API §2 — the caller supplies a signal that also enforces the 5-second server timeout (003_BD HS-02). */
export const getHealth = (signal?: AbortSignal) => getJson<SystemHealth>('/api/system/health', signal)
