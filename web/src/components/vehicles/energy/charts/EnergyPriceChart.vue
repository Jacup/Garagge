<script setup lang="ts">
import { computed } from 'vue'
import { useTheme } from 'vuetify'
import VueApexCharts from 'vue3-apexcharts'
import type { ApexOptions } from 'apexcharts'
import type { EnergyType, EnergyUnit, StatsPeriod } from '@/api/generated/apiV1.schemas'
import { StatsPeriod as SP } from '@/api/generated/apiV1.schemas'

const props = defineProps<{
  entries: { date: string; type: EnergyType; pricePerUnit: number }[]
  dataPeriod: StatsPeriod
}>()

const theme = useTheme()

const xRange = computed(() => {
  const now = new Date()

  if (props.dataPeriod === SP.Lifetime) {
    const oldest = props.entries.reduce((min, e) => {
      const t = new Date(e.date).getTime()
      return t < min ? t : min
    }, now.getTime())
    return { min: oldest, max: now.getTime() }
  }

  if (props.dataPeriod === SP.Week) {
    const mon = new Date(now)
    const day = now.getDay() === 0 ? 6 : now.getDay() - 1
    mon.setDate(now.getDate() - day)
    mon.setHours(0, 0, 0, 0)
    const sun = new Date(mon)
    sun.setDate(mon.getDate() + 6)
    sun.setHours(23, 59, 59, 999)
    return { min: mon.getTime(), max: sun.getTime() }
  }

  if (props.dataPeriod === SP.Month) {
    const start = new Date(now.getFullYear(), now.getMonth(), 1)
    const end = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999)
    return { min: start.getTime(), max: end.getTime() }
  }

  // Year
  const start = new Date(now.getFullYear(), 0, 1)
  const end = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999)
  return { min: start.getTime(), max: end.getTime() }
})

const xAxisConfig = computed(() => {
  switch (props.dataPeriod) {
    case SP.Week:
      return { tickAmount: 7, format: 'dd MMM' }
    case SP.Month:
      return { tickAmount: 8, format: 'dd MMM' }
    case SP.Year:
      return { tickAmount: 12, format: 'MMM yy' }
    case SP.Lifetime:
    default:
      return { tickAmount: undefined, format: 'MMM yy' }
  }
})

const palette = computed(() => {
  const c = theme.current.value.colors
  return [c.primary, c.tertiary, c.secondary, c.error, c.success, c.warning, c.info]
})

const series = computed(() => {
  const groups = new Map<string, { x: number; y: number }[]>()

  for (const entry of props.entries) {
    const t = new Date(entry.date).getTime()
    if (!groups.has(entry.type)) groups.set(entry.type, [])
    groups.get(entry.type)!.push({ x: t, y: entry.pricePerUnit })
  }

  return Array.from(groups.entries()).map(([name, data]) => ({
    name,
    data: data.sort((a, b) => a.x - b.x),
  }))
})

const chartOptions = computed(
  (): ApexOptions => ({
    chart: {
      type: 'line',
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
    stroke: { curve: 'smooth' },
    markers: {
      size: 0,
      hover: { size: 5, sizeOffset: 3 },
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
          fontFamily: 'inherit',
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
      x: { format: 'dd MMM yyyy' },
      y: { formatter: (val: number) => `${val.toFixed(3)} / L` },
      theme: theme.global.name.value,
      style: { fontFamily: 'inherit' },
    },
    legend: {
      fontFamily: 'inherit',
      fontSize: '14px',
      offsetY: 0,
      labels: { colors: theme.current.value.colors['on-surface'] },
      markers: { size: 5, offsetX: -5, shape: 'circle' as const },
      itemMargin: { horizontal: 12 },
    },
    dataLabels: { enabled: false },
  }),
)
</script>

<template>
  <VueApexCharts height="100%" :options="chartOptions" :series="series" />
</template>
