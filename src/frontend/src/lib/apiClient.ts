// Shared JSON fetch wrapper (DD-001 module 10). Same-origin cookies, JSON bodies only (DEC-020), RFC 9457 errors.

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  /** "VALIDATION" for 400, otherwise a DD-001 message ID (e.g. MSG-E009). */
  code?: string
  /** Field → message IDs (400 only). */
  errors?: Record<string, string[]>
  traceId?: string
}

export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails | null

  constructor(status: number, problem: ProblemDetails | null) {
    super(`API request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

let unauthorizedHandler: (() => void) | null = null

/** Registered by AuthProvider: on any 401 the session is cleared, and ProtectedRoute then redirects to /login. */
export function setUnauthorizedHandler(handler: (() => void) | null) {
  unauthorizedHandler = handler
}

async function request<T>(method: string, url: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }

  const response = await fetch(url, {
    method,
    credentials: 'same-origin',
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
    signal,
  })

  if (!response.ok) {
    if (response.status === 401) {
      unauthorizedHandler?.()
    }
    throw new ApiError(response.status, await readProblem(response))
  }

  return (await response.json()) as T
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  const contentType = response.headers.get('Content-Type') ?? ''
  if (!contentType.includes('json')) {
    return null
  }
  try {
    return (await response.json()) as ProblemDetails
  } catch {
    return null
  }
}

export function getJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  return request<T>('GET', url, undefined, signal)
}

export function sendJson<T>(method: 'POST' | 'PUT', url: string, body: unknown): Promise<T> {
  return request<T>(method, url, body)
}
