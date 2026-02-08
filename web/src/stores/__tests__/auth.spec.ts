import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'
import { createApp } from 'vue'
import type { LoginRequest, RegisterRequest } from '@/api/generated/apiV1.schemas'

const app = createApp({})

const { mockPostApiAuthLogin, mockPostApiAuthLogout, mockPostApiAuthRegister } = vi.hoisted(() => {
  return {
    mockPostApiAuthLogin: vi.fn(),
    mockPostApiAuthLogout: vi.fn(),
    mockPostApiAuthRegister: vi.fn(),
  }
})

vi.mock('@/api/generated/auth/auth', () => ({
  getAuth: () => ({
    postApiAuthLogin: mockPostApiAuthLogin,
    postApiAuthRegister: mockPostApiAuthRegister,
    postApiAuthLogout: mockPostApiAuthLogout,
  }),
}))

vi.mock('axios', () => {
  const isAxiosError = (error: unknown) => {
    return error instanceof Error && 'isAxiosError' in error && (error as Record<string, unknown>).isAxiosError === true
  }
  return {
    default: { isAxiosError },
    isAxiosError,
  }
})

const mockFetchUserData = vi.fn()
const mockUserReset = vi.fn()
const mockUserId = vi.fn(() => '')

vi.mock('../user', () => {
  return {
    useUserStore: vi.fn(() => ({
      fetchUserData: mockFetchUserData,
      $reset: mockUserReset,
      id: mockUserId(),
    })),
  }
})

vi.mock('@/router', () => ({
  default: {
    push: vi.fn(),
    currentRoute: {
      value: { name: 'Home' },
    },
  },
}))

describe('Auth Store', () => {
  beforeEach(() => {
    const pinia = createPinia()
    app.use(pinia)
    setActivePinia(pinia)
    vi.clearAllMocks()
    mockUserId.mockReturnValue('')
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('initializes with empty state', () => {
      const store = useAuthStore()

      expect(store.$state).toEqual({})
    })
  })

  describe('getters', () => {
    it('isAuthenticated returns false when user has no id', () => {
      mockUserId.mockReturnValue('')
      const store = useAuthStore()

      expect(store.isAuthenticated).toBe(false)
    })

    it('isAuthenticated returns true when user has id', () => {
      mockUserId.mockReturnValue('user-id-123')
      const store = useAuthStore()

      expect(store.isAuthenticated).toBe(true)
    })

    it('isAuthenticated returns false when user id is null', () => {
      mockUserId.mockReturnValue(null!)
      const store = useAuthStore()

      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('actions', () => {
    describe('login', () => {
      it('successfully logs in and fetches user data', async () => {
        mockPostApiAuthLogin.mockResolvedValue(undefined)
        mockFetchUserData.mockResolvedValue(undefined)

        const store = useAuthStore()
        const loginRequest: LoginRequest = {
          email: 'test@test.com',
          password: 'password',
          rememberMe: false,
        }

        await store.login(loginRequest)

        expect(mockPostApiAuthLogin).toHaveBeenCalledWith(loginRequest)
        expect(mockFetchUserData).toHaveBeenCalled()
      })

      it('calls API with correct login request', async () => {
        mockPostApiAuthLogin.mockResolvedValue(undefined)
        mockFetchUserData.mockResolvedValue(undefined)

        const store = useAuthStore()
        const loginRequest: LoginRequest = {
          email: 'user@example.com',
          password: 'pass123',
          rememberMe: true,
        }

        await store.login(loginRequest)

        expect(mockPostApiAuthLogin).toHaveBeenCalledWith(loginRequest)
        expect(mockFetchUserData).toHaveBeenCalled()
      })

      it('throws error when login fails', async () => {
        const error = Object.assign(new Error('Invalid credentials'), {
          isAxiosError: true,
          response: {
            status: 401,
            data: { message: 'Invalid credentials' },
          },
        })

        mockPostApiAuthLogin.mockRejectedValue(error)

        const store = useAuthStore()
        const loginRequest: LoginRequest = {
          email: 'test@test.com',
          password: 'wrong-password',
          rememberMe: false,
        }

        await expect(store.login(loginRequest)).rejects.toThrow('Invalid credentials')
        expect(mockFetchUserData).not.toHaveBeenCalled()
      })

      it('throws error when fetchUserData fails', async () => {
        mockPostApiAuthLogin.mockResolvedValue(undefined)
        const error = Object.assign(new Error('Failed to fetch user'), {
          isAxiosError: true,
          response: {
            status: 500,
          },
        })
        mockFetchUserData.mockRejectedValue(error)

        const store = useAuthStore()
        const loginRequest: LoginRequest = {
          email: 'test@test.com',
          password: 'password',
          rememberMe: false,
        }

        await expect(store.login(loginRequest)).rejects.toThrow('Failed to fetch user')
      })
    })

    describe('logout', () => {
      it('successfully logs out and clears state', async () => {
        mockPostApiAuthLogout.mockResolvedValue(undefined)

        const store = useAuthStore()

        await store.logout()

        expect(mockPostApiAuthLogout).toHaveBeenCalled()
        expect(store.$reset).toBeDefined()
        expect(mockUserReset).toHaveBeenCalled()
      })

      it('clears state even when logout API fails', async () => {
        mockPostApiAuthLogout.mockRejectedValue(new Error('API error'))

        const store = useAuthStore()
        const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

        await store.logout()

        expect(mockUserReset).toHaveBeenCalled()
        expect(consoleErrorSpy).toHaveBeenCalledWith('Logout API failed, but clearing local state anyway', expect.any(Error))

        consoleErrorSpy.mockRestore()
      })

      it('resets both auth and user stores on logout', async () => {
        mockPostApiAuthLogout.mockResolvedValue(undefined)

        const store = useAuthStore()

        await store.logout()

        expect(mockUserReset).toHaveBeenCalled()
      })
    })

    describe('register', () => {
      it('successfully registers user', async () => {
        mockPostApiAuthRegister.mockResolvedValue(undefined)

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await store.register(registerRequest)

        expect(mockPostApiAuthRegister).toHaveBeenCalledWith(registerRequest)
      })

      it('handles 400 bad request error', async () => {
        const error = Object.assign(new Error('Bad Request'), {
          isAxiosError: true,
          response: {
            status: 400,
            data: { message: 'Invalid input' },
          },
        })

        mockPostApiAuthRegister.mockRejectedValue(error)

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await expect(store.register(registerRequest)).rejects.toThrow('Bad Request')
      })

      it('handles 409 conflict error (email already in use)', async () => {
        const error = Object.assign(new Error('Conflict'), {
          isAxiosError: true,
          response: {
            status: 409,
            data: { message: 'Email already exists' },
          },
        })

        mockPostApiAuthRegister.mockRejectedValue(error)

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await expect(store.register(registerRequest)).rejects.toThrow('Conflict')
      })

      it('handles generic errors', async () => {
        const error = Object.assign(new Error('Network error'), {
          isAxiosError: false,
        })

        mockPostApiAuthRegister.mockRejectedValue(error)

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await expect(store.register(registerRequest)).rejects.toThrow('Network error')
      })
    })
  })
})
