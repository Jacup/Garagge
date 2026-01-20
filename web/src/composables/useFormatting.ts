export function useFormatting() {
  const formatCurrency = (value: number | undefined | null) => {
    if (!value) return '-'
    return new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(value)
  }

    const formatCurrencyString = (value: string | undefined | null) => {
    if (!value || isNaN(Number(value))) return '-'

    const parsedValue = Number(value)
    return new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(parsedValue)
  }

  const formatDate = (dateString: string | undefined | null) => {
    if (!dateString) return '-'
    return new Date(dateString).toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  const formatMileage = (mileage: number | undefined | null) => {
    return mileage ? `${mileage.toLocaleString('pl-PL')} km` : '-'
  }

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('pl-PL', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  const formatDateOnly = (dateString: string) => {
    return new Date(dateString).toLocaleString('pl-PL', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    })
  }

  return { formatCurrency, formatCurrencyString, formatDate, formatMileage, formatDateTime, formatDateOnly }
}
