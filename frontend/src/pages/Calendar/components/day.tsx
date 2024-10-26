import { useEffect, useMemo } from 'react'
import { DateTime, Interval } from 'luxon'
import { useCalendarContext } from '../CalendarContext'
import { PartyIcon } from '../../../icons/party'
import { BussinessWatchIcon } from '../../../icons/bussinesswatch'
import { Tooltip } from '@nextui-org/react'

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
      leavesElements = leaves
        .sort((a, b) => (a.dateStart < b.dateStart ? -1 : 1))
        .map((leave) => {
          if (DateTime.fromISO(leave.dateStart).hasSame(dayDate, 'day')) {
            const leaveDuration = Interval.fromDateTimes(
              DateTime.fromISO(leave.dateStart),
              DateTime.fromISO(leave.dateEnd)
            )
              .toDuration('days')
              ?.toObject().days
            if (leaveDuration && leaveDuration < 2) {
              return (
                <Tooltip
                  content={
                    leave.type === 'bankHoliday'
                      ? 'Bank Holiday'
                      : leave.approvedBy
                        ? 'Paid Time Off'
                        : 'Request'
                  }
                >
                  <div
                    key={`leave-${dayDate}`}
                    className={`${
                      isFilling
                        ? 'border-gray-50'
                        : isToday
                          ? 'border-primary-100'
                          : isPast
                            ? 'border-default-100'
                            : 'border-white'
                    } ${leave.type === 'bankHoliday' ? 'bg-secondary' : leave.approvedBy ? 'bg-default-500' : 'bg-default-200'} pl-2 align-middle border-solid border-l-3 rounded-lg w-[calc(100vw/7)] h-[25px]  text-white truncate overflow-hidden`}
                  >
                    {leave.type === 'bankHoliday' ? (
                      <PartyIcon dimension={'12'} fill="white" />
                    ) : (
                      <BussinessWatchIcon dimension="12" fill="white" />
                    )}
                    <p className="pl-2 inline">{leave.ownerName}</p>
                  </div>
                </Tooltip>
              )
            }
            return (
              <Tooltip
                content={
                  leave.type === 'bankHoliday'
                    ? 'Bank Holiday'
                    : leave.approvedBy
                      ? 'Paid Time Off'
                      : 'Request'
                }
              >
                <div
                  key={`leave-${dayDate}`}
                  className={`${
                    isFilling
                      ? 'border-gray-50'
                      : isToday
                        ? 'border-primary-100'
                        : isPast
                          ? 'border-default-100'
                          : 'border-white'
                  } ${leave.type === 'bankHoliday' ? 'bg-secondary' : leave.approvedBy ? 'bg-default-500' : 'bg-default-200'} pl-2 border-solid border-l-3 rounded-l-lg w-[calc(100vw/7)] h-[25px] text-white whitespace-nowrap overflow-visible z-10`}
                >
                  {leave.type === 'bankHoliday' ? (
                    <PartyIcon dimension={'12'} fill="white" />
                  ) : (
                    <BussinessWatchIcon dimension="12" fill="white" />
                  )}
                  <p className="pl-2 inline">{leave.ownerName}</p>
                </div>
              </Tooltip>
            )
          } else if (
            DateTime.fromISO(leave.dateEnd)
              .minus({ days: 1 })
              .hasSame(dayDate, 'day')
          ) {
            return (
              <Tooltip
                content={
                  leave.type === 'bankHoliday'
                    ? 'Bank Holiday'
                    : leave.approvedBy
                      ? 'Paid Time Off'
                      : 'Request'
                }
              >
                <div
                  key={`leave-${dayDate}`}
                  className={`${leave.type === 'bankHoliday' ? 'bg-secondary' : leave.approvedBy ? 'bg-default-500' : 'bg-default-200'} w-full h-[25px] rounded-r-lg`}
                ></div>
              </Tooltip>
            )
          } else {
            return (
              <Tooltip
                content={
                  leave.type === 'bankHoliday'
                    ? 'Bank Holiday'
                    : leave.approvedBy
                      ? 'Paid Time Off'
                      : 'Request'
                }
              >
                <div
                  key={`leave-${dayDate}`}
                  className={`${leave.type === 'bankHoliday' ? 'bg-secondary' : leave.approvedBy ? 'bg-default-500' : 'bg-default-200'} w-full h-[25px]`}
                ></div>
              </Tooltip>
            )
          }
        })
    }
    return (
      <div
        id={dayDate.day.toString()}
        className={`flex h-[150px] items-center text-default-600 border-none justify-start w-full flex-col flex-nowrap  border  
          ${isFilling && 'bg-gray-50'} 
          ${isToday && 'bg-primary-100'} 
          ${isPast && 'bg-default-100'}`}
      >
        {dayDate?.day}
        {leavesElements}
      </div>
    )
  }, [dayDate, isFilling, isPast, isToday, myleaves])

  return <>{body}</>
}
