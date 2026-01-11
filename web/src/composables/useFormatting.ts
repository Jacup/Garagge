export function useFormatting() {
  const formatCurrency = (value: number | null) => {
    if (!value) return '-'
    return new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(value)
  }

  const formatDate = (dateString: string) => {
    if (!dateString) return '-'
    return new Date(dateString).toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  const formatMileage = (mileage: number | null) => {
    return mileage ? `${mileage.toLocaleString('pl-PL')} km` : 'N/A'
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

  return { formatCurrency, formatDate, formatMileage, formatDateTime, formatDateOnly }
}
