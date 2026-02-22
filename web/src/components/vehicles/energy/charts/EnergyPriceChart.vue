<script setup lang="ts">
import { computed } from 'vue'
import { useTheme } from 'vuetify'
import VueApexCharts from 'vue3-apexcharts'
import type { ApexOptions } from 'apexcharts'

export interface FuelPriceEntry {
  datetime: string // ISO 8601, np. "2024-06-15T10:30:00"
  pricePerUnit: number
  type: string // np. "Diesel", "Petrol 95"
}

const props = defineProps<{
  entries: FuelPriceEntry[]
  dataPeriod: 0 | 1 | 2 | 3 // 0=Lifetime, 1=Year, 2=Month, 3=Week
}>()

const theme = useTheme()

// ── Zakres osi X na podstawie dataPeriod ──────────────────────────────────────
const xRange = computed(() => {
  const now = new Date()

  if (props.dataPeriod === 0) {
    const oldest = props.entries.reduce((min, e) => {
      const t = new Date(e.datetime).getTime()
      return t < min ? t : min
    }, now.getTime())
    return { min: oldest, max: now.getTime() }
  }

  if (props.dataPeriod === 3) {
    // Ten tydzień: poniedziałek 00:00 — niedziela 23:59
    const mon = new Date(now)
    const day = now.getDay() === 0 ? 6 : now.getDay() - 1 // getDay: 0=Sun
    mon.setDate(now.getDate() - day)
    mon.setHours(0, 0, 0, 0)

    const sun = new Date(mon)
    sun.setDate(mon.getDate() + 6)
    sun.setHours(23, 59, 59, 999)

    return { min: mon.getTime(), max: sun.getTime() }
  }

  if (props.dataPeriod === 2) {
    // Ten miesiąc: 1. dzień — ostatni dzień
    const start = new Date(now.getFullYear(), now.getMonth(), 1)
    const end = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999)
    return { min: start.getTime(), max: end.getTime() }
  }

  if (props.dataPeriod === 1) {
    // Ten rok: 1 Jan — 31 Dec
    const start = new Date(now.getFullYear(), 0, 1)
    const end = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999)
    return { min: start.getTime(), max: end.getTime() }
  }

  return { min: now.getTime(), max: now.getTime() }
})

// ── Tick / format osi X ───────────────────────────────────────────────────────
const xAxisConfig = computed(() => {
  switch (props.dataPeriod) {
    case 3: // Week — podziałki dzienne
      return {
        tickAmount: 7,
        format: 'dd MMM',
      }
    case 2: // Month — podziałki co ~4 dni
      return {
        tickAmount: 8,
        format: 'dd MMM',
      }
    case 1: // Year — podziałki miesięczne
      return {
        tickAmount: 12,
        format: 'MMM yy',
      }
    case 0: // Lifetime — ApexCharts dobierze sam
    default:
      return {
        tickAmount: undefined,
        format: 'MMM yy',
      }
  }
})

// ── Kolory z Vuetify theme ────────────────────────────────────────────────────
const palette = computed(() => {
  const c = theme.current.value.colors
  return [c.primary, c.secondary, c.tertiary ?? c.error, c.success, c.warning, c.info]
})

// ── Serie — grupowanie po type ────────────────────────────────────────────────
const series = computed(() => {
  const groups = new Map<string, { x: number; y: number }[]>()

  for (const entry of props.entries) {
    const t = new Date(entry.datetime).getTime()
    if (!groups.has(entry.type)) groups.set(entry.type, [])
    groups.get(entry.type)!.push({ x: t, y: entry.pricePerUnit })
  }

  // Sortujemy punkty chronologicznie w każdej serii
  return Array.from(groups.entries()).map(([name, data]) => ({
    name,
    data: data.sort((a, b) => a.x - b.x),
  }))
})

// ── Opcje ApexCharts ──────────────────────────────────────────────────────────
const chartOptions = computed(
  (): ApexOptions => ({
    chart: {
      type: 'area',
      toolbar: { show: false },
      zoom: { enabled: false },
      fontFamily: 'inherit',
      background: 'transparent',
      animations: {
        enabled: true,
        speed: 400,
      },
    },
    colors: palette.value,
    stroke: {
      curve: 'smooth',
    },
    fill: {
      type: 'gradient',
      gradient: {
        shadeIntensity: 1,
        opacityFrom: 0.3,
        opacityTo: 0.02,
        stops: [0, 100],
      },
    },
    markers: {
      size: 4,
      strokeWidth: 0,
      hover: { size: 6 },
    },
    xaxis: {
      type: 'datetime',
      min: xRange.value.min,
      max: xRange.value.max,
      tickAmount: xAxisConfig.value.tickAmount,
      labels: {
        datetimeUTC: false,
        format: xAxisConfig.value.format,
        style: {
          colors: theme.current.value.colors['on-surface-variant'],
          fontSize: '11px',
        },
      },
      axisBorder: { show: false },
      axisTicks: { show: false },
    },
    yaxis: {
      labels: {
        formatter: (val: number) => val.toFixed(2),
        style: {
          colors: theme.current.value.colors['on-surface-variant'],
          fontSize: '11px',
        },
      },
    },
    grid: {
      borderColor: theme.current.value.colors['surface-variant'],
      strokeDashArray: 4,
      xaxis: { lines: { show: false } },
    },
    tooltip: {
      x: { format: 'dd MMM yyyy HH:mm' },
      y: { formatter: (val: number) => `${val.toFixed(3)} / L` },
      theme: theme.global.name.value, // 'light' | 'dark'
    },
    legend: {
      position: 'top',
      horizontalAlign: 'left',
      labels: {
        colors: theme.current.value.colors['on-surface'],
      },
    },
    dataLabels: { enabled: false },
  }),
)
</script>

<template>
  <VueApexCharts height="260" :options="chartOptions" :series="series" />
</template>
