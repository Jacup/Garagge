// src/composables/vehicle/useServiceDetailsState.ts
import { ref } from 'vue';
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas';

export function useServiceDetailsState() {
  // Czy panel szczegółów jest otwarty?
  const isOpen = ref(false);

  // Rekord, który aktualnie wyświetlamy w szczegółach
  const selectedRecord = ref<ServiceRecordDto | null>(null);

  /**
   * Otwiera panel szczegółów dla danego rekordu.
   */
  const open = (record: ServiceRecordDto) => {
    selectedRecord.value = record;
    isOpen.value = true;
  };

  /**
   * Zamyka panel szczegółów.
   */
  const close = () => {
    isOpen.value = false;
    // Opcjonalnie: możemy wyczyścić selectedRecord po zamknięciu (np. z opóźnieniem),
    // ale często lepiej go zostawić, żeby uniknąć "migania" pustego panelu podczas animacji zamykania.
  };

  return {
    isOpen,
    selectedRecord,
    open,
    close
  };
}
