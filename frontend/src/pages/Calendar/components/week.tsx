import { FC, useMemo } from 'react'
import { DateTime, Interval } from 'luxon'
import { Day } from './day'

type WeekProps = {
  weekInterval: Interval
}

const Week: FC<WeekProps> = ({ weekInterval }) => {
  const isoWeekDate = useMemo(
    () => weekInterval.start?.toISOWeekDate(),
    [weekInterval.start]
  )
  const weekIntervals = useMemo(
    () => weekInterval.splitBy({ days: 1 }),
    [weekInterval]
  )

  const weeksElement = useMemo(() => {
    return (
      <div className="flex flex-row w-full h-[150px]" key={isoWeekDate}>
        {weekIntervals.map((dayDate, index) => (
          <Day key={index} dayDate={dayDate.start || DateTime.utc()} />
        ))}
      </div>
    )
  }, [weekIntervals, isoWeekDate])

  return <>{weeksElement}</>
}

export default Week
