/* eslint-disable no-nested-ternary */
import { DateTime, Interval } from 'luxon'
import { createContext, useContext, useState } from 'react'
import { Calendar } from '.'
import {
  useGetAllLeaves,
  useGetMyCircleLeaves,
  useGetMyLeaves,
} from '../../../models/Leaves'

const CalendarContext = createContext<CalendarStateType>(undefined!)

const InitialInterval = Interval.fromDateTimes(
  DateTime.utc().startOf('month').startOf('week'),
  DateTime.utc().endOf('month').endOf('week')
)

const CalendarContextProvider = () => {
  const [interval, setInterval] = useState<Interval>(InitialInterval)
  const [visibleDate, setInternalVisibleDate] = useState(DateTime.now())
  const [selectedFilter, setSelectedFilter] =
    useState<selectedFilterType>('myleaves')
  const myleaves = useGetMyLeaves(interval)
  const allleaves = useGetAllLeaves(interval)
  const mycircleleaves = useGetMyCircleLeaves(interval)

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

  const goToNextMonth = () => {
    setInternalVisibleDate((currentVisibleDate) =>
      currentVisibleDate.plus({ months: 1 }).startOf('month').plus({ days: 14 })
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
        interval,
        goToPreviousMonth,
        goToNextMonth,
        goToToday,
        visibleDate,
        setSelectedFilter,
        leaves:
          selectedFilter === 'myleaves'
            ? myleaves.data
            : selectedFilter === 'allleaves'
              ? allleaves.data
              : mycircleleaves.data,
      }}
    >
      <Calendar />
    </CalendarContext.Provider>
  )
}

const useCalendarContext = () => {
  const context = useContext(CalendarContext)

  if (!context) {
    throw new Error('getContext must be used within CalendarContext')
  }

  return context
}

export { CalendarContextProvider, useCalendarContext }
