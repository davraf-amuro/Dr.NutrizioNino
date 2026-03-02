export interface ApiErrorPayload {
  status?: number
  title?: string
  detail?: string
  message?: string
  traceId?: string
}

export class ApiError extends Error {
  status?: number
  title?: string
  detail?: string
  traceId?: string

  constructor(message: string, payload?: ApiErrorPayload) {
    super(message)
    this.name = 'ApiError'
    this.status = payload?.status
    this.title = payload?.title
    this.detail = payload?.detail
    this.traceId = payload?.traceId
  }
}
