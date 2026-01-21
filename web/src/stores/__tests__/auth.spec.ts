import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'
import { useUserStore } from '../user'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import { createApp } from 'vue'
import type { LoginRequest, RegisterRequest, LoginResponse } from '@/api/generated/apiV1.schemas'

const app = createApp({})

// Define mocks at module level
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
    default: {
      isAxiosError,
    },
  }
})

vi.mock('../user', () => {
  const mockFetchUserData = vi.fn()
  const mockReset = vi.fn()
  return {
    useUserStore: vi.fn(() => ({
      fetchUserData: mockFetchUserData,
      $reset: mockReset,
    })),
  }
})

describe('Auth Store', () => {
  beforeEach(() => {
    const pinia = createPinia().use(piniaPluginPersistedstate)
    app.use(pinia)
    setActivePinia(pinia)
    localStorage.clear()
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('initializes with null access token', () => {
      const store = useAuthStore()

      expect(store.accessToken).toBeNull()
    })
  })

  describe('getters', () => {
    it('isAuthenticated returns false when accessToken is null', () => {
      const store = useAuthStore()

      expect(store.isAuthenticated).toBe(false)
    })

    it('isAuthenticated returns true when accessToken is set', () => {
      const store = useAuthStore()
      store.setToken('test-token')

      expect(store.isAuthenticated).toBe(true)
    })

    it('isAuthenticated returns false when accessToken is empty string', () => {
      const store = useAuthStore()
      store.setToken('')

      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('actions', () => {
    describe('setToken', () => {
      it('sets access token correctly', () => {
        const store = useAuthStore()

        store.setToken('my-token')

        expect(store.accessToken).toBe('my-token')
        expect(store.isAuthenticated).toBe(true)
      })

      it('clears access token when set to null', () => {
        const store = useAuthStore()
        store.setToken('my-token')

        store.setToken(null)

        expect(store.accessToken).toBeNull()
        expect(store.isAuthenticated).toBe(false)
      })
    })

    describe('login', () => {
      it('successfully logs in and sets token', async () => {
        mockPostApiAuthLogin.mockResolvedValue({ accessToken: 'new-token' })
        const mockUserStore = useUserStore()

        const store = useAuthStore()
        const loginRequest: LoginRequest = {
          email: 'test@test.com',
          password: 'password',
          rememberMe: false,
        }

        await store.login(loginRequest)

        expect(store.accessToken).toBe('new-token')
        expect(store.isAuthenticated).toBe(true)
        expect(mockUserStore.fetchUserData).toHaveBeenCalled()
        expect(mockPostApiAuthLogin).toHaveBeenCalledWith(loginRequest)
      })

      it('throws error when login response has no access token', async () => {
        mockPostApiAuthLogin.mockResolvedValue({} as LoginResponse)
        useUserStore()

        const store = useAuthStore()
        const loginRequest: LoginRequest = {
          email: 'test@test.com',
          password: 'password',
          rememberMe: false,
        }

        await expect(store.login(loginRequest)).rejects.toThrow('Login failed: No access token received')
      })

      it('calls API with correct login request', async () => {
        mockPostApiAuthLogin.mockResolvedValue({ accessToken: 'token' })
        useUserStore()

        const store = useAuthStore()
        const loginRequest: LoginRequest = {
          email: 'user@example.com',
          password: 'pass123',
          rememberMe: true,
        }

        await store.login(loginRequest)

        expect(mockPostApiAuthLogin).toHaveBeenCalledWith(loginRequest)
      })
    })

    describe('logout', () => {
      it('successfully logs out and clears state', async () => {
        mockPostApiAuthLogout.mockResolvedValue(undefined)
        const mockUserStore = useUserStore()

        const store = useAuthStore()
        store.setToken('test-token')
        localStorage.setItem('test-key', 'test-value')

        await store.logout()

        expect(store.accessToken).toBeNull()
        expect(store.isAuthenticated).toBe(false)
        expect(mockUserStore.$reset).toHaveBeenCalled()
        expect(localStorage.length).toBe(0)
        expect(mockPostApiAuthLogout).toHaveBeenCalled()
      })

      it('clears state even when logout API fails', async () => {
        mockPostApiAuthLogout.mockRejectedValue(new Error('API error'))
        const mockUserStore = useUserStore()

        const store = useAuthStore()
        store.setToken('test-token')
        localStorage.setItem('test-key', 'test-value')
        const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

        await store.logout()

        expect(store.accessToken).toBeNull()
        expect(store.isAuthenticated).toBe(false)
        expect(mockUserStore.$reset).toHaveBeenCalled()
        expect(localStorage.length).toBe(0)
        expect(consoleErrorSpy).toHaveBeenCalledWith('Logout API failed, but clearing local state anyway', expect.any(Error))

        consoleErrorSpy.mockRestore()
      })

      it('resets store state on logout', async () => {
        mockPostApiAuthLogout.mockResolvedValue(undefined)
        useUserStore()

        const store = useAuthStore()
        store.setToken('token123')

        await store.logout()

        expect(store.accessToken).toBeNull()
      })
    })

    describe('register', () => {
      it('successfully registers user', async () => {
        mockPostApiAuthRegister.mockResolvedValue('success')
        const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await store.register(registerRequest)

        expect(mockPostApiAuthRegister).toHaveBeenCalledWith(registerRequest)
        expect(consoleSpy).toHaveBeenCalledWith('Registration successful:')

        consoleSpy.mockRestore()
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
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await store.register(registerRequest)

        expect(consoleSpy).toHaveBeenCalledWith('Registration failed: Bad Request -', { message: 'Invalid input' })

        consoleSpy.mockRestore()
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
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await store.register(registerRequest)

        expect(consoleSpy).toHaveBeenCalledWith('Registration failed: Conflict - Email already in use.')

        consoleSpy.mockRestore()
      })

      it('handles generic errors', async () => {
        const error = Object.assign(new Error('Network error'), {
          isAxiosError: false,
        })

        mockPostApiAuthRegister.mockRejectedValue(error)
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        await store.register(registerRequest)

        expect(consoleSpy).toHaveBeenCalledWith('Registration failed:', error)

        consoleSpy.mockRestore()
      })

      it('does not throw exception on any error type', async () => {
        mockPostApiAuthRegister.mockRejectedValue(new Error('Some error'))
        vi.spyOn(console, 'error').mockImplementation(() => {})

        const store = useAuthStore()
        const registerRequest: RegisterRequest = {
          email: 'test@test.com',
          password: 'password123',
          firstName: 'Test',
          lastName: 'User',
        }

        // Should not throw
        await expect(store.register(registerRequest)).resolves.not.toThrow()
      })
    })
  })
})
