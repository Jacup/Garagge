import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import App from '../App.vue'

// Mock components
vi.mock('../components/layout/MainContent.vue', () => ({
  default: {
    template: '<div data-testid="main-content">Main Content</div>',
  },
}))

vi.mock('../components/layout/NavBar.vue', () => ({
  default: {
    template: '<div data-testid="navbar">NavBar</div>',
  },
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [{ path: '/', component: { template: '<div>Home</div>' } }],
})

describe('App', () => {
  it('renders main layout components', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [createPinia(), router],
      },
    })

    expect(wrapper.find('[data-testid="navbar"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="main-content"]').exists()).toBe(true)
  })

  it('has correct CSS classes for layout', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [createPinia(), router],
      },
    })

    expect(wrapper.find('.app-layout').exists()).toBe(true)
  })
})
