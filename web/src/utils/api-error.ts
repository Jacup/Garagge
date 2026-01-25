/**
 * Custom error class to preserve API error structure through throws
 */
import type { ParsedApiError } from './error-handler'

export class ApiError extends Error {
  constructor(
    public parsedError: ParsedApiError,
  ) {
    super(parsedError.message)
    this.name = 'ApiError'
  }
}
