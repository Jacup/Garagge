<script setup lang="ts">
import { computed } from 'vue'
import { useTheme } from 'vuetify'
import VueApexCharts from 'vue3-apexcharts'
import type { ApexOptions } from 'apexcharts'
import type { EnergyChartEntryDto } from './EnergyChartsSection.vue'
import type { EnergyUnit } from '@/api/generated/apiV1.schemas'

const props = defineProps<{
  entries: Pick<EnergyChartEntryDto, 'date' | 'type' | 'energyUnit' | 'consumption'>[]
  dataPeriod: 0 | 1 | 2 | 3
}>()

const theme = useTheme()

const xRange = computed(() => {
  const now = new Date()

  if (props.dataPeriod === 0) {
    const oldest = props.entries.reduce((min, e) => {
      const t = new Date(e.date).getTime()
      return t < min ? t : min
    }, now.getTime())
    return { min: oldest, max: now.getTime() }
  }

  if (props.dataPeriod === 3) {
    const mon = new Date(now)
    const day = now.getDay() === 0 ? 6 : now.getDay() - 1
    mon.setDate(now.getDate() - day)
    mon.setHours(0, 0, 0, 0)
    const sun = new Date(mon)
    sun.setDate(mon.getDate() + 6)
    sun.setHours(23, 59, 59, 999)
    return { min: mon.getTime(), max: sun.getTime() }
  }

  if (props.dataPeriod === 2) {
    const start = new Date(now.getFullYear(), now.getMonth(), 1)
    const end = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999)
    return { min: start.getTime(), max: end.getTime() }
  }

  const start = new Date(now.getFullYear(), 0, 1)
  const end = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999)
  return { min: start.getTime(), max: end.getTime() }
})

const xAxisConfig = computed(() => {
  switch (props.dataPeriod) {
    case 3:
      return { tickAmount: 7, format: 'dd MMM' }
    case 2:
      return { tickAmount: 8, format: 'dd MMM' }
    case 1:
      return { tickAmount: 12, format: 'MMM yy' }
    case 0:
      return { tickAmount: undefined, format: 'MMM yy' }
  }
})

const palette = computed(() => {
  const c = theme.current.value.colors
  return [c.primary, c.tertiary, c.secondary, c.error, c.success, c.warning, c.info]
})

const unitLabel = (unit: EnergyUnit | undefined): string => {
  if (!unit) return ''
  return (
    {
      Liter: 'L/100km',
      Gallon: 'gal/100mi',
      CubicMeter: 'm³/100km',
      kWh: 'kWh/100km',
    }[unit] ?? ''
  )
}

const axes = computed(() => {
  const seen = new Set<string>()
  const result: { type: string; unit: EnergyUnit }[] = []
  for (const e of props.entries) {
    if (!seen.has(e.type)) {
      seen.add(e.type)
      result.push({ type: e.type, unit: e.energyUnit })
    }
  }
  return result
})

const series = computed(() =>
  axes.value.map(({ type }) => {
    const data = props.entries
      .filter((e) => e.type === type && e.consumption !== null)
      .map((e) => ({ x: new Date(e.date).getTime(), y: e.consumption! }))
      .sort((a, b) => a.x - b.x)
    return { name: type, data }
  }),
)

const yaxis = computed(
  (): NonNullable<ApexOptions['yaxis']> =>
    axes.value.map(({ type, unit }, index) => ({
      seriesName: type,
      opposite: index > 0,
      labels: {
        formatter: (val: number) => `${val.toFixed(1)} ${unitLabel(unit)}`,
        style: {
          colors: palette.value[index],
          fontSize: '11px',
          fontFamily: 'inherit',
        },
      },
      axisBorder: {
        show: axes.value.length > 1,
        color: palette.value[index],
      },
    })),
)

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
    stroke: {
      curve: 'smooth',
    },
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
    yaxis: yaxis.value,
    grid: {
      borderColor: theme.current.value.colors['surface-variant'],
      strokeDashArray: 0,
      xaxis: { lines: { show: false } },
      yaxis: { lines: { show: true } },
      padding: { left: 8, right: axes.value.length > 1 ? 24 : 16, bottom: 24 },
    },
    tooltip: {
      x: { format: 'dd MMM yyyy' },
      y: {
        formatter: (val: number, { seriesIndex }: { seriesIndex: number }) => {
          const unit = axes.value[seriesIndex]?.unit
          const label = unitLabel(unit)
          return label ? `${val.toFixed(2)} ${label}` : val.toFixed(2)
        },
      },
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
