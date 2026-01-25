/**
 * API Error Response Types based on RFC 9110 Problem Details
 * Matches the structure returned by the backend
 */

/**
 * Single validation error from the backend
 */
export interface ValidationErrorDetail {
  code: string
  description: string
  type: string
}

/**
 * Generic problem details response from API
 * Used for 400, 401, 404, 409, 500 errors
 */
export interface ProblemDetails {
  type: string
  title: string
  status: number
  detail: string
  traceId: string
  /**
   * Only present for validation errors (400)
   */
  errors?: ValidationErrorDetail[]
}

/**
 * Type guard to check if error is AxiosError with ProblemDetails
 */
export function isProblemDetails(data: unknown): data is ProblemDetails {
  if (typeof data !== 'object' || data === null) {
    return false
  }
  const obj = data as Record<string, unknown>
  return typeof obj.title === 'string' && typeof obj.status === 'number'
}

/**
 * Type guard to check if it's a validation error response
 */
export function isValidationError(error: ProblemDetails): error is ProblemDetails & { errors: ValidationErrorDetail[] } {
  return Array.isArray(error.errors) && error.errors.length > 0
}
