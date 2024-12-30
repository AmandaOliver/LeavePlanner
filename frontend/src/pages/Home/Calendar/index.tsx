import { Interval } from 'luxon'
import { useMemo } from 'react'
import Week from './components/week'
import { WEEKDAYS, FIRSTDAYOFWEEK } from './constants'
import Header from './components/header'
import { useCalendarContext } from './CalendarContext'

export const WorkspaceCalendar = () => {
  const { interval } = useCalendarContext()

  const headerElement = useMemo(() => {
    return new Array(7).fill(0).map((_, index) => (
      <div className="flex" key={index}>
        {WEEKDAYS[(index + FIRSTDAYOFWEEK) % 7]}
      </div>
    ))
  }, [])

  const weeksIntervalElement = useMemo(() => {
    const splitIntervals = interval.splitBy({ weeks: 1 })

    return (
      <section className="flex flex-col w-full h-full">
        <Header />
        <section className="flex flex-col h-full w-full">
          <div className="flex justify-around py-2 sticky text-sm text-white font-bold bg-primary">
            {headerElement}
          </div>
          <div className="flex flex-col w-full h-[calc(100vh-164px)] overflow-scroll">
            {splitIntervals.map((weekInterval: Interval) => (
              <Week
                key={`${weekInterval.toISO()}`}
                weekInterval={weekInterval}
              />
            ))}
          </div>
        </section>
      </section>
    )
  }, [headerElement, interval])

  return <>{weeksIntervalElement}</>
}
