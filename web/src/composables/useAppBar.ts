import { ref, readonly, watch } from 'vue'
import { useRoute } from 'vue-router'

export interface AppBarAction {
  icon: string
  label?: string
  action: () => void
}

export interface AppBarConfig {
  type: 'search' | 'context'
  title: string
  actions: AppBarAction[]
}

const globalState = ref<AppBarConfig>({
  type: 'search',
  title: '',
  actions: [],
})

export function useAppBar() {
  const route = useRoute()

  watch(
    () => route.path,
    () => {
      const appBarMeta = route.meta?.appBar as { type: 'search' | 'context' } | undefined
      if (appBarMeta?.type !== 'context') {
        resetToSearch()
      }
    },
  )

  function setContextBar(title: string, actions: AppBarAction[] = []) {
    globalState.value = {
      type: 'context',
      title,
      actions,
    }
  }

  function resetToSearch() {
    globalState.value = {
      type: 'search',
      title: '',
      actions: [],
    }
  }

  return {
    state: readonly(globalState),
    setContextBar,
    resetToSearch,
  }
}
