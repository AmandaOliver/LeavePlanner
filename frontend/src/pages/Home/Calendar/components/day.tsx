import { ReactNode, useEffect, useMemo } from 'react'
import { DateTime } from 'luxon'
import { useCalendarContext } from '../CalendarContext'
import { Tooltip } from '@nextui-org/react'
import { LeaveStartDay } from './leaveStartDay'
import { getSpaces } from './getMarginTop'
import { LeaveType } from '../../../../models/Leaves'

interface DayProps {
  dayDate: DateTime
}

export const Day = ({ dayDate }: DayProps) => {
  const { visibleDate, leaves } = useCalendarContext()
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
    const leavesToShow = leaves
      ?.filter(
        (leave) =>
          DateTime.fromISO(leave.dateStart) <= dayDate &&
          DateTime.fromISO(leave.dateEnd) > dayDate
      )
      .sort((a, b) => (a.dateStart < b.dateStart ? -1 : 1))
    const leavesElements = leavesToShow?.map((leave) => {
      const background = leave.approvedBy
        ? leave.type === 'bankHoliday'
          ? 'bg-secondary'
          : 'bg-default-500'
        : 'bg-default-200'
      let processedLeaves: LeaveType[] = []

      const spaces = getSpaces(leave, leaves || [], dayDate, processedLeaves)
      const distanceTop = `${spaces * 32}px`
      const common = `mb-1 h-[28px] min-h-[28px] w-full`
      const TooltipElement = ({ children }: { children: ReactNode }) => (
        <Tooltip
          content={
            leave.type === 'bankHoliday'
              ? 'Public Holiday'
              : leave.approvedBy
                ? 'Paid Time Off'
                : 'Request'
          }
        >
          {children}
        </Tooltip>
      )

      if (DateTime.fromISO(leave.dateStart).hasSame(dayDate, 'day')) {
        return (
          <LeaveStartDay
            leave={leave}
            dayDate={dayDate}
            background={background}
            common={common}
            distanceTop={distanceTop}
            key={`start-${leave.id}`}
          />
        )
      } else if (
        DateTime.fromISO(leave.dateEnd)
          .minus({ days: 1 })
          .hasSame(dayDate, 'day')
      ) {
        return (
          <TooltipElement>
            <div
              key={`leave-${dayDate}`}
              className={`${background} ${common} rounded-r-lg`}
              style={{ marginTop: distanceTop }}
            ></div>
          </TooltipElement>
        )
      } else {
        return (
          <TooltipElement>
            <div
              key={`leave-${dayDate}`}
              className={`${background} ${common}`}
              style={{ marginTop: distanceTop }}
            ></div>
          </TooltipElement>
        )
      }
    })

    return (
      <div
        key={dayDate.day.toString()}
        id={dayDate.day.toString()}
        className={`flex h-[150px] overflow-scroll items-center text-default-600 border-none justify-start w-full flex-col flex-nowrap  border  
          ${isFilling && 'bg-gray-50'} 
          ${isToday && 'bg-primary-100'} 
          ${isPast && 'bg-default-100'}`}
      >
        <div className="mb-1">{dayDate?.day}</div>
        {leavesElements}
      </div>
    )
  }, [dayDate, isFilling, isPast, isToday, leaves])

  return <>{body}</>
}
