/* eslint-disable no-nested-ternary */
import { DateTime, Interval } from 'luxon'
import { createContext, useContext, useState } from 'react'
import { CALENDARMODE } from './constants'
import { WorkspaceCalendar } from '.'
import { useGetAllLeaves, useGetMyLeaves } from '../../models/Calendar'

export const statusFiltersOptions = ['draft', 'scheduled', 'failed', 'sent']

const CalendarContext = createContext<CalendarStateType>(undefined!)

const InitialInterval = Interval.fromDateTimes(
  DateTime.utc().startOf('month').startOf('week'),
  DateTime.utc().endOf('month').endOf('week')
)

const CalendarContextProvider = () => {
  const [interval, setInterval] = useState<Interval>(InitialInterval)
  const [visibleDate, setInternalVisibleDate] = useState(DateTime.now())
  const [calendarMode, setCalendarMode] = useState(CALENDARMODE.MONTH)
  const [selectedFilter, setSelectedFilter] =
    useState<selectedFilterType>('myleaves')
  const myleaves = useGetMyLeaves(interval)
  const allleaves = useGetAllLeaves(interval)

  const loadPreviousInterval = () => {
    setInterval((previousInterval) => {
      if (previousInterval && previousInterval.start && previousInterval.end) {
        return Interval.fromDateTimes(
          previousInterval.start
            .plus({ days: 14 })
            .minus({ months: 1 })
            .startOf('month')
            .startOf('week')
            .toJSDate(),
          previousInterval.start
            .plus({ days: 14 })
            .minus({ months: 1 })
            .endOf('month')
            .endOf('week')
            .toJSDate()
        )
      }

      return InitialInterval
    })
  }

  const loadNextInterval = () => {
    setInterval((previousInterval) => {
      if (previousInterval && previousInterval.start && previousInterval.end) {
        return Interval.fromDateTimes(
          previousInterval.start
            .plus({ months: 1, days: 14 })
            .startOf('month')
            .startOf('week')
            .toJSDate(),
          previousInterval.start
            .plus({ months: 1, days: 14 })
            .endOf('month')
            .endOf('week')
            .toJSDate()
        )
      }

      return InitialInterval
    })
  }

  const goToPreviousMonth = () => {
    setInternalVisibleDate((currentVisibleDate) =>
      currentVisibleDate
        .startOf('month')
        .minus({ months: 1 })
        .plus({ days: 14 })
    )
    loadPreviousInterval()
  }

  const goToPreviousWeek = () => {
    setInternalVisibleDate((currentVisibleDate) =>
      currentVisibleDate.startOf('week').minus({ week: 1 }).plus({ days: 3 })
    )
    loadPreviousInterval()
  }

  const goToNextMonth = () => {
    setInternalVisibleDate((currentVisibleDate) =>
      currentVisibleDate.plus({ months: 1 }).startOf('month').plus({ days: 14 })
    )
    loadNextInterval()
  }

  const goToNextWeek = () => {
    setInternalVisibleDate((currentVisibleDate) =>
      currentVisibleDate.plus({ week: 1 }).startOf('week').plus({ days: 3 })
    )
    loadNextInterval()
  }

  const goToToday = () => {
    const today = DateTime.local()

    if (!interval.contains(today)) {
      setInterval(InitialInterval)
    }
    setInternalVisibleDate(today)
  }

  return (
    <CalendarContext.Provider
      value={{
        calendarMode,
        interval,
        goToPreviousMonth,
        goToPreviousWeek,
        goToNextMonth,
        goToNextWeek,
        goToToday,
        setCalendarMode,
        setInterval,
        setVisibleDate: setInternalVisibleDate,
        visibleDate,
        setSelectedFilter,
        leaves: selectedFilter === 'myleaves' ? myleaves.data : allleaves.data,
      }}
    >
      <WorkspaceCalendar />
    </CalendarContext.Provider>
  )
}

const useCalendarContext = () => {
  const context = useContext(CalendarContext)

  if (!context) {
    throw new Error('getContext must be used within AuthContext')
  }

  return context
}

export { CalendarContextProvider, useCalendarContext }
