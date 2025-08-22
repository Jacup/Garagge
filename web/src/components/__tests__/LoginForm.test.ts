import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import LoginForm from '../LoginForm.vue'

// Mock router
const mockPush = vi.fn()
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

// Mock API
vi.mock('@/api/generated/users/users', () => ({
  getUsers: vi.fn(() => ({
    getUsersUserId: vi.fn(),
    getUsersMe: vi.fn(),
    postUsersLogin: vi.fn(),
    postUsersRegister: vi.fn(),
  })),
}))

describe('LoginForm', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('renders login form with all fields', () => {
    const wrapper = mount(LoginForm)

    expect(wrapper.find('h2').text()).toBe('Logowanie')
    expect(wrapper.find('#email').exists()).toBe(true)
    expect(wrapper.find('#password').exists()).toBe(true)
    expect(wrapper.find('button[type="submit"]').text()).toBe('Zaloguj')
  })

  it('updates input values when user types', async () => {
    const wrapper = mount(LoginForm)

    const emailInput = wrapper.find('#email')
    const passwordInput = wrapper.find('#password')

    await emailInput.setValue('test@example.com')
    await passwordInput.setValue('password123')

    expect((emailInput.element as HTMLInputElement).value).toBe('test@example.com')
    expect((passwordInput.element as HTMLInputElement).value).toBe('password123')
  })

  it('has required attributes on form fields', () => {
    const wrapper = mount(LoginForm)

    const emailInput = wrapper.find('#email')
    const passwordInput = wrapper.find('#password')

    expect(emailInput.attributes('type')).toBe('email')
    expect(emailInput.attributes('required')).toBeDefined()
    expect(passwordInput.attributes('type')).toBe('password')
    expect(passwordInput.attributes('required')).toBeDefined()
  })

  it('displays error message when login fails', async () => {
    const { getUsers } = await import('@/api/generated/users/users')
    const mockGetUsers = vi.mocked(getUsers)
    const mockPostUsersLogin = vi.fn().mockRejectedValue(new Error('Invalid credentials'))
    mockGetUsers.mockReturnValue({
      getUsersUserId: vi.fn(),
      getUsersMe: vi.fn(),
      postUsersLogin: mockPostUsersLogin,
      postUsersRegister: vi.fn(),
    })

    const wrapper = mount(LoginForm)

    // Fill form
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('wrongpassword')

    // Submit form
    await wrapper.find('form').trigger('submit.prevent')

    // Wait for async operations
    await wrapper.vm.$nextTick()

    expect(wrapper.find('.error').text()).toBe('Invalid credentials')
  })
})
