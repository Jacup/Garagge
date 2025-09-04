import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useUserStore } from '../userStore'

const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
})

vi.mock('@/api/generated/users/users', () => ({
  getUsers: vi.fn(() => ({
    getApiUsersMe: vi.fn(),
  })),
}))

describe('useUserStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorageMock.getItem.mockReturnValue(null)
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  it('initializes with empty state when no localStorage data', () => {
    localStorageMock.getItem.mockReturnValue(null)

    const store = useUserStore()

    expect(store.accessToken).toBe('')
    expect(store.user).toBeNull()
  })

  it('initializes with localStorage data when available', () => {
    localStorageMock.getItem.mockImplementation((key: string) => {
      if (key === 'app_token') return 'test-token'
      if (key === 'app_user')
        return JSON.stringify({
          userId: 'user-123',
          email: 'test@example.com',
          firstName: 'Jan',
          lastName: 'Kowalski',
        })
      return null
    })

    const store = useUserStore()

    expect(store.accessToken).toBe('test-token')
    expect(store.user).toEqual({
      userId: 'user-123',
      email: 'test@example.com',
      firstName: 'Jan',
      lastName: 'Kowalski',
    })
  })

  it('sets token and saves to localStorage', () => {
    const store = useUserStore()

    store.setToken('new-token')

    expect(store.accessToken).toBe('new-token')
    expect(localStorageMock.setItem).toHaveBeenCalledWith('app_token', 'new-token')
  })

  it('sets profile and saves to localStorage', () => {
    const store = useUserStore()
    const profileData = {
      id: 'user-456',
      email: 'jane@example.com',
      firstName: 'Jane',
      lastName: 'Doe',
    }

    store.setProfile(profileData)

    expect(store.user).toEqual({
      userId: 'user-456',
      email: 'jane@example.com',
      firstName: 'Jane',
      lastName: 'Doe',
    })

    expect(localStorageMock.setItem).toHaveBeenCalledWith(
      'app_user',
      JSON.stringify({
        userId: 'user-456',
        email: 'jane@example.com',
        firstName: 'Jane',
        lastName: 'Doe',
      }),
    )
  })

  it('clears all data and localStorage', () => {
    const store = useUserStore()

    store.setToken('test-token')
    store.setProfile({
      id: 'user-123',
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
    })

    store.clearToken()

    expect(store.accessToken).toBe('')
    expect(store.user).toBeNull()

    expect(localStorageMock.removeItem).toHaveBeenCalledWith('app_token')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('app_user')
  })

  it('initializeStore returns early when no token in localStorage', async () => {
    localStorageMock.getItem.mockReturnValue(null)
    const store = useUserStore()

    await store.initializeStore()

    expect(store.accessToken).toBe('')
    expect(store.user).toBeNull()
  })

  it('initializeStore clears token on API error', async () => {
    const mockGetApiUsersMe = vi.fn().mockRejectedValue(new Error('Unauthorized'))

    const { getUsers } = await import('@/api/generated/users/users')
    ;(getUsers as ReturnType<typeof vi.fn>).mockReturnValue({
      getApiUsersMe: mockGetApiUsersMe,
    })

    localStorageMock.getItem.mockImplementation((key: string) => {
      if (key === 'app_token') return 'invalid-token'
      if (key === 'app_user') return 'null'
      return null
    })

    const store = useUserStore()
    await store.initializeStore()

    expect(store.accessToken).toBe('')
    expect(store.user).toBeNull()
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('app_token')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('app_user')
  })
})
