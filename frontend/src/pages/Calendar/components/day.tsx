import { useMemo } from 'react'
import { DateTime } from 'luxon'
import { useCalendarContext } from '../CalendarContext'

interface DayProps {
  dayDate: DateTime
}

export const Day = ({ dayDate }: DayProps) => {
  const { visibleDate } = useCalendarContext()
  const isToday = useMemo(
    () => dayDate.hasSame(DateTime.now(), 'day'),
    [dayDate]
  )
  const isPast = useMemo(
    () => dayDate.startOf('day') < DateTime.now().startOf('day'),
    [dayDate]
  )
  const isFilling = useMemo(
    () => dayDate.month !== visibleDate.month,
    [dayDate.month, visibleDate.month]
  )

  const body = useMemo(() => {
    return (
      <div
        className={`flex p-2 items-center text-default-600 border-white justify-start w-full flex-col flex-nowrap h-full border  
          ${isFilling && 'bg-default-100/30'} 
          ${isToday && 'bg-primary-100'} 
          ${isPast && 'bg-primary-100/40'}`}
      >
        {dayDate?.day}
      </div>
    )
  }, [])

  return <>{body}</>
}
