import { computed } from 'vue'
import { useRouter } from 'vue-router'
import type { NavigationItem } from '@/constants/navigation'

export function useNavigationItem(item: NavigationItem) {
  const router = useRouter()

  const currentRoute = computed(() => router.currentRoute.value.path)
  const isActive = computed(() => currentRoute.value === item.link)

  const currentIcon = computed(() => {
    return isActive.value ? item.activeIcon || item.icon : item.icon
  })

  const navigate = () => {
    router.push(item.link)
  }

  return {
    isActive,
    currentIcon,
    navigate,
  }
}
