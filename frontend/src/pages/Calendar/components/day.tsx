import { useEffect, useMemo } from 'react'
import { DateTime, Interval } from 'luxon'
import { useCalendarContext } from '../CalendarContext'
import { PartyIcon } from '../../../icons/party'
import { RequestIcon } from '../../../icons/request'
import { BussinessWatchIcon } from '../../../icons/bussinesswatch'

interface DayProps {
  dayDate: DateTime
}

export const Day = ({ dayDate }: DayProps) => {
  const { visibleDate, myleaves } = useCalendarContext()
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
  useEffect(() => {
    if (isToday) {
      window.document.getElementById(dayDate.day.toString())?.scrollIntoView()
    }
  }, [dayDate.day, isToday])
  const body = useMemo(() => {
    let leavesElements
    const leaves = myleaves?.filter(
      (leave) =>
        DateTime.fromISO(leave.dateStart) <= dayDate &&
        DateTime.fromISO(leave.dateEnd) > dayDate
    )
    if (leaves) {
      leavesElements = leaves.map((leave) => {
        if (DateTime.fromISO(leave.dateStart).hasSame(dayDate, 'day')) {
          const leaveDuration = Interval.fromDateTimes(
            DateTime.fromISO(leave.dateStart),
            DateTime.fromISO(leave.dateEnd)
          )
            .toDuration('days')
            ?.toObject().days
          if (leaveDuration && leaveDuration < 2) {
            return (
              <div
                key={`leave-${dayDate}`}
                className="border-white pl-2 align-middle border-solid border-l-3 rounded-lg w-[calc(100vw/7)] h-[25px] bg-secondary text-white truncate overflow-hidden"
              >
                {leave.type === 'bankHoliday' ? (
                  <PartyIcon dimension={'12'} fill="white" />
                ) : (
                  <BussinessWatchIcon dimension="12" fill="white" />
                )}
                <p className="pl-2 inline">{leave.owner}</p>
              </div>
            )
          }
          return (
            <div
              key={`leave-${dayDate}`}
              className="border-white pl-2 border-solid border-l-3 rounded-l-lg w-[calc(100vw/7)] h-[25px] bg-secondary text-white whitespace-nowrap overflow-visible z-10"
            >
              {leave.type === 'bankHoliday' ? (
                <PartyIcon dimension={'12'} fill="white" />
              ) : (
                <BussinessWatchIcon dimension="12" fill="white" />
              )}
              <p className="pl-2 inline">{leave.owner}</p>
            </div>
          )
        } else if (
          DateTime.fromISO(leave.dateEnd)
            .minus({ days: 1 })
            .hasSame(dayDate, 'day')
        ) {
          return (
            <div
              key={`leave-${dayDate}`}
              className="w-full bg-secondary h-[25px] rounded-r-lg"
            ></div>
          )
        } else {
          return (
            <div
              key={`leave-${dayDate}`}
              className="w-full bg-secondary h-[25px]"
            ></div>
          )
        }
      })
    }
    return (
      <div
        id={dayDate.day.toString()}
        className={`flex h-[150px] items-center text-default-600 border-none justify-start w-full flex-col flex-nowrap  border  
          ${isFilling && 'bg-default-100/30'} 
          ${isToday && 'bg-primary-100'} 
          ${isPast && 'bg-primary-100/40'}`}
      >
        {dayDate?.day}
        {leavesElements}
      </div>
    )
  }, [dayDate, isFilling, isPast, isToday, myleaves])

  return <>{body}</>
}
