<script setup lang="ts">
import { computed } from 'vue'
import { useTheme } from 'vuetify'
import VueApexCharts from 'vue3-apexcharts'
import type { ApexOptions } from 'apexcharts'
import type { EnergyChartEntryDto } from './EnergyChartsSection.vue'

const props = defineProps<{
  entries: Pick<EnergyChartEntryDto, 'date' | 'type' | 'cost'>[]
  dataPeriod: 0 | 1 | 2 | 3
}>()

const theme = useTheme()

const palette = computed(() => {
  const c = theme.current.value.colors
  return [c.primary, c.tertiary, c.secondary, c.error, c.success, c.warning, c.info]
})

const startOfWeek = (date: Date): Date => {
  const d = new Date(date)
  const day = d.getDay() === 0 ? 6 : d.getDay() - 1
  d.setDate(d.getDate() - day)
  d.setHours(0, 0, 0, 0)
  return d
}

const localDateKey = (date: Date): string => {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

const monthKey = (date: Date): string => {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  return `${y}-${m}`
}

const categories = computed((): string[] => {
  const now = new Date()

  if (props.dataPeriod === 3) {
    // 7 dni bieżącego tygodnia
    const mon = startOfWeek(now)
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(mon)
      d.setDate(mon.getDate() + i)
      return localDateKey(d)
    })
  }

  if (props.dataPeriod === 2) {
    // tygodnie bieżącego miesiąca (poniedziałki)
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1)
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0)
    const weeks: string[] = []
    const cursor = startOfWeek(firstDay)
    while (cursor <= lastDay) {
      weeks.push(localDateKey(new Date(cursor)))
      cursor.setDate(cursor.getDate() + 7)
    }
    return weeks
  }

  if (props.dataPeriod === 1) {
    return Array.from({ length: 12 }, (_, i) => {
      const m = String(i + 1).padStart(2, '0')
      return `${now.getFullYear()}-${m}`
    })
  }

  const dates = props.entries.map((e) => new Date(e.date))
  if (dates.length === 0) return []
  const oldest = new Date(Math.min(...dates.map((d) => d.getTime())))
  const newest = new Date(Math.max(...dates.map((d) => d.getTime())))
  const keys: string[] = []
  const cursor = new Date(oldest.getFullYear(), oldest.getMonth(), 1)
  const end = new Date(newest.getFullYear(), newest.getMonth(), 1)
  while (cursor <= end) {
    keys.push(monthKey(new Date(cursor)))
    cursor.setMonth(cursor.getMonth() + 1)
  }
  return keys
})

const entryBucketKey = (date: Date): string => {
  if (props.dataPeriod === 3) return localDateKey(date)
  if (props.dataPeriod === 2) return localDateKey(startOfWeek(date))
  return monthKey(date)
}

const types = computed((): string[] => {
  const set = new Set(props.entries.map((e) => e.type))
  return Array.from(set).sort()
})

const series = computed(() =>
  types.value.map((type) => {
    const buckets = new Map<string, number>()
    for (const e of props.entries) {
      if (e.type !== type || e.cost === null) continue
      const key = entryBucketKey(new Date(e.date))
      buckets.set(key, (buckets.get(key) ?? 0) + e.cost)
    }
    return {
      name: type,
      data: categories.value.map((key) => parseFloat((buckets.get(key) ?? 0).toFixed(2))),
    }
  }),
)

const xAxisFormat = computed(() => {
  switch (props.dataPeriod) {
    case 3:
      return 'dd MMM'
    case 2:
      return 'dd MMM'
    case 1:
      return 'MMM'
    case 0:
      return 'MMM yy'
  }
})

const chartOptions = computed(
  (): ApexOptions => ({
    chart: {
      type: 'bar',
      stacked: true,
      toolbar: { show: false },
      zoom: { enabled: false },
      fontFamily: 'inherit',
      background: 'transparent',
      animations: {
        enabled: true,
        speed: 600,
        animateGradually: { enabled: true, delay: 100 },
        dynamicAnimation: { enabled: true, speed: 400 },
      },
    },
    colors: palette.value,
    plotOptions: {
      bar: {
        columnWidth: '50%',
        borderRadius: 4,
        borderRadiusApplication: 'end' as const,
        borderRadiusWhenStacked: 'last' as const,
      },
    },
    xaxis: {
      type: 'datetime',
      categories: categories.value,
      labels: {
        datetimeUTC: false,
        format: xAxisFormat.value,
        style: {
          colors: theme.current.value.colors['on-surface-variant'],
          fontSize: '11px',
          fontFamily: 'inherit',
        },
      },
      axisBorder: { show: false },
      axisTicks: { show: false },
    },
    yaxis: {
      labels: {
        formatter: (val: number) => `${val.toFixed(0)} €`,
        style: {
          colors: theme.current.value.colors['on-surface-variant'],
          fontSize: '11px',
          fontFamily: 'inherit',
        },
      },
    },
    grid: {
      borderColor: theme.current.value.colors['surface-variant'],
      strokeDashArray: 0,
      xaxis: { lines: { show: false } },
      yaxis: { lines: { show: true } },
      padding: { left: 8, right: 16, bottom: 24 },
    },
    tooltip: {
      x: { format: props.dataPeriod <= 1 ? 'MMM yyyy' : 'dd MMM yyyy' },
      y: { formatter: (val: number) => `${val.toFixed(2)} €` },
      theme: theme.global.name.value,
      style: { fontFamily: 'inherit' },
    },
    legend: {
      fontFamily: 'inherit',
      fontSize: '14px',
      offsetY: 0,
      labels: {
        colors: theme.current.value.colors['on-surface'],
      },
      markers: {
        size: 5,
        offsetX: -5,
        shape: 'circle' as const,
      },
      itemMargin: { horizontal: 12 },
    },
    dataLabels: { enabled: false },
  }),
)
</script>

<template>
  <VueApexCharts height="100%" :options="chartOptions" :series="series" />
</template>
