/**
 * Error handling utilities for API calls
 * Provides type-safe error parsing without using 'any'
 */
import { isAxiosError } from 'axios'
import type { ProblemDetails, ValidationErrorDetail } from '@/types/api-errors'
import { isProblemDetails, isValidationError } from '@/types/api-errors'

/**
 * Parsed error information from API response
 */
export interface ParsedApiError {
  message: string
  title: string
  status: number
  traceId?: string
  validationErrors?: ValidationErrorDetail[]
  isValidationError: boolean
}

/**
 * Parse an unknown error into a structured ParsedApiError
 * Handles AxiosError, ProblemDetails, and generic errors
 *
 * @param error - The error to parse
 * @returns ParsedApiError with structured error information
 */
export function parseApiError(error: unknown): ParsedApiError {
  // Handle axios errors
  if (isAxiosError(error)) {
    const responseData = error.response?.data

    // Check if response matches ProblemDetails format
    if (isProblemDetails(responseData)) {
      const problemDetails = responseData as ProblemDetails

      return {
        message: problemDetails.detail,
        title: problemDetails.title,
        status: problemDetails.status,
        traceId: problemDetails.traceId,
        validationErrors: isValidationError(problemDetails) ? problemDetails.errors : undefined,
        isValidationError: isValidationError(problemDetails),
      }
    }

    // Fallback for non-standard axios errors
    return {
      message: error.message || 'Request failed',
      title: `HTTP ${error.response?.status || 'Error'}`,
      status: error.response?.status || 0,
      isValidationError: false,
    }
  }

  // Handle standard Error objects
  if (error instanceof Error) {
    return {
      message: error.message,
      title: 'Error',
      status: 0,
      isValidationError: false,
    }
  }

  // Fallback for unknown error types
  return {
    message: 'An unexpected error occurred',
    title: 'Unknown Error',
    status: 0,
    isValidationError: false,
  }
}

/**
 * Get validation error for a specific field
 *
 * @param parsedError - Parsed error from parseApiError()
 * @param fieldName - The field name to get error for (e.g., 'email')
 * @returns Error description or undefined if not found
 */
export function getFieldError(parsedError: ParsedApiError, fieldName: string): string | undefined {
  if (!parsedError.validationErrors) {
    return undefined
  }

  // Try to find error by field name in error code
  const fieldError = parsedError.validationErrors.find(
    (error) => error.code.toLowerCase().includes(fieldName.toLowerCase())
  )

  return fieldError?.description
}

/**
 * Get all validation errors as a map {fieldName: errorMessage}
 *
 * @param parsedError - Parsed error from parseApiError()
 * @returns Object with field-level errors, or empty object if no validation errors
 */
export function getValidationErrorMap(
  parsedError: ParsedApiError
): Record<string, string> {
  if (!parsedError.validationErrors) {
    return {}
  }

  const errorMap: Record<string, string> = {}

  parsedError.validationErrors.forEach((error) => {
    // Try to extract field name from error code
    // e.g., "Auth.InvalidEmail" -> "email", "Email.Required" -> "email"
    const fieldName = error.code.split('.')[1]?.toLowerCase() || error.code.toLowerCase()
    errorMap[fieldName] = error.description
  })

  return errorMap
}
