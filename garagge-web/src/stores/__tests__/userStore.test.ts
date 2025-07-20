import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useUserStore } from '../userStore'

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock
})

// Mock API
vi.mock('@/api/userApi', () => ({
  getUserProfile: vi.fn()
}))

describe('useUserStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  it('initializes with empty state when no localStorage data', () => {
    localStorageMock.getItem.mockReturnValue(null)

    const store = useUserStore()

    expect(store.accessToken).toBe('')
    expect(store.userId).toBe('')
    expect(store.email).toBe('')
    expect(store.firstName).toBe('')
    expect(store.lastName).toBe('')
  })

  it('initializes with localStorage data when available', () => {
    localStorageMock.getItem.mockImplementation((key: string) => {
      const data = {
        'accessToken': 'test-token',
        'userId': 'user-123',
        'email': 'test@example.com',
        'firstName': 'Jan',
        'lastName': 'Kowalski'
      }
      return data[key as keyof typeof data] || null
    })

    const store = useUserStore()

    expect(store.accessToken).toBe('test-token')
    expect(store.userId).toBe('user-123')
    expect(store.email).toBe('test@example.com')
    expect(store.firstName).toBe('Jan')
    expect(store.lastName).toBe('Kowalski')
  })

  it('sets token and saves to localStorage', () => {
    const store = useUserStore()

    store.setToken('new-token')

    expect(store.accessToken).toBe('new-token')
    expect(localStorageMock.setItem).toHaveBeenCalledWith('accessToken', 'new-token')
  })

  it('sets profile and saves to localStorage', () => {
    const store = useUserStore()
    const profileData = {
      userId: 'user-456',
      email: 'jane@example.com',
      firstName: 'Jane',
      lastName: 'Doe'
    }

    store.setProfile(profileData)

    expect(store.userId).toBe('user-456')
    expect(store.email).toBe('jane@example.com')
    expect(store.firstName).toBe('Jane')
    expect(store.lastName).toBe('Doe')

    expect(localStorageMock.setItem).toHaveBeenCalledWith('userId', 'user-456')
    expect(localStorageMock.setItem).toHaveBeenCalledWith('email', 'jane@example.com')
    expect(localStorageMock.setItem).toHaveBeenCalledWith('firstName', 'Jane')
    expect(localStorageMock.setItem).toHaveBeenCalledWith('lastName', 'Doe')
  })

  it('clears all data and localStorage', () => {
    const store = useUserStore()

    // Set some initial data
    store.setToken('test-token')
    store.setProfile({
      userId: 'user-123',
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User'
    })

    // Clear everything
    store.clearToken()

    expect(store.accessToken).toBe('')
    expect(store.userId).toBe('')
    expect(store.email).toBe('')
    expect(store.firstName).toBe('')
    expect(store.lastName).toBe('')

    expect(localStorageMock.removeItem).toHaveBeenCalledWith('accessToken')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('userId')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('email')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('firstName')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('lastName')
  })
})
