import { ref } from 'vue';
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas';

const isOpen = ref(false);
const selectedRecord = ref<ServiceRecordDto | null>(null);

export function useServiceDetailsState() {

  const open = (record: ServiceRecordDto) => {
    selectedRecord.value = record;
    isOpen.value = true;
  };

  const close = () => {
    isOpen.value = false;
    setTimeout(() => {
        selectedRecord.value = null;
    }, 300);
  };

  return {
    isOpen,
    selectedRecord,
    open,
    close
  };
}
