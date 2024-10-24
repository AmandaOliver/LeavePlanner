import { useCallback } from 'react'
import { useCalendarContext } from '../CalendarContext'
import { CALENDARMODE } from '../constants'
import { Button } from '@nextui-org/react'

const Header = () => {
  const {
    visibleDate,
    calendarMode,
    goToPreviousMonth,
    goToPreviousWeek,
    goToNextMonth,
    goToNextWeek,
    goToToday,
  } = useCalendarContext()

  const prevCallback = () =>
    calendarMode === CALENDARMODE.MONTH
      ? goToPreviousMonth()
      : goToPreviousWeek()

  const nextCallback = () =>
    calendarMode === CALENDARMODE.MONTH ? goToNextMonth() : goToNextWeek()

  return (
    <div className="items-center bg-default-300 flex justify-between m-2 pl-4 w-auto">
      <div className="items-center flex">
        <h1 className="text-[16px]">{visibleDate.toFormat('MMM yyyy')}</h1>
        <Button
          data-testid="month-control-prev"
          size={'md'}
          onClick={prevCallback}
        >
          prev
        </Button>
        <Button
          data-testid="month-control-next"
          size={'md'}
          onClick={nextCallback}
        >
          next
        </Button>
        <Button
          data-testid="mode-control-today"
          onClick={() => goToToday()}
          variant="bordered"
        >
          Today
        </Button>
      </div>
    </div>
  )
}

export default Header
