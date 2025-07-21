import { describe, it, expect, vi, beforeEach } from 'vitest'
import { login, register, getUserProfile } from '../userApi'
import { createPinia, setActivePinia } from 'pinia'

// Mock fetch
const mockFetch = vi.fn()
global.fetch = mockFetch

// Mock userStore
vi.mock('@/stores/userStore', () => ({
  useUserStore: () => ({
    accessToken: 'test-token'
  })
}))

describe('userApi', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('login', () => {
    it('sends POST request with credentials and returns response', async () => {
      const mockResponse = { accessToken: 'new-token', userId: 'user-123' }
      mockFetch.mockResolvedValue({
        ok: true,
        json: () => Promise.resolve(mockResponse)
      })

      const result = await login('test@example.com', 'password123')

      expect(mockFetch).toHaveBeenCalledWith('/users/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: 'test@example.com',
          password: 'password123'
        }),
      })
      expect(result).toEqual(mockResponse)
    })

    it('throws error when login fails', async () => {
      mockFetch.mockResolvedValue({
        ok: false,
        status: 401
      })

      await expect(login('test@example.com', 'wrongpassword'))
        .rejects.toThrow('Login failed')
    })
  })

  describe('register', () => {
    it('sends POST request with user data and returns response', async () => {
      const mockResponse = {
        accessToken: 'new-token',
        userId: 'user-456',
        email: 'new@example.com'
      }
      mockFetch.mockResolvedValue({
        ok: true,
        json: () => Promise.resolve(mockResponse)
      })

      const result = await register('new@example.com', 'Jane', 'Doe', 'password123')

      expect(mockFetch).toHaveBeenCalledWith('/users/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: 'new@example.com',
          firstName: 'Jane',
          lastName: 'Doe',
          password: 'password123'
        }),
      })
      expect(result).toEqual(mockResponse)
    })

    it('throws error when registration fails', async () => {
      mockFetch.mockResolvedValue({
        ok: false,
        status: 400
      })

      await expect(register('invalid@email', 'Jane', 'Doe', 'weak'))
        .rejects.toThrow('Registration failed')
    })
  })

  describe('getUserProfile', () => {
    it('sends GET request with authorization header', async () => {
      const mockProfile = {
        userId: 'user-123',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User'
      }
      mockFetch.mockResolvedValue({
        ok: true,
        json: () => Promise.resolve(mockProfile)
      })

      const result = await getUserProfile()

      expect(mockFetch).toHaveBeenCalledWith('/users/me', {
        headers: {
          Authorization: 'Bearer test-token',
        },
      })
      expect(result).toEqual(mockProfile)
    })

    it('throws error when profile fetch fails', async () => {
      mockFetch.mockResolvedValue({
        ok: false,
        status: 401
      })

      await expect(getUserProfile())
        .rejects.toThrow('Fetch profile failed')
    })
  })
})
