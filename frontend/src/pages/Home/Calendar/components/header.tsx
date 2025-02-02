import { useCalendarContext } from '../CalendarContext'
import { Button, Select, SelectItem } from '@nextui-org/react'
import { ChevronLeftIcon } from '../../../../icons/chevron_left'
import { ChevronRightIcon } from '../../../../icons/chevron_right'

const Header = () => {
  const {
    visibleDate,
    goToPreviousMonth,
    goToNextMonth,
    goToToday,
    setSelectedFilter,
  } = useCalendarContext()
  const prevCallback = () => goToPreviousMonth()

  const nextCallback = () => goToNextMonth()

  return (
    <div className="flex m-2 gap-2">
      <div className="flex justify-between gap-8 w-full">
        <Button
          data-testid="month-control-prev"
          size={'lg'}
          isIconOnly
          variant="ghost"
          onClick={prevCallback}
          aria-label="previous month"
        >
          <ChevronLeftIcon />
        </Button>
        <h1 className="text-[22px] self-center">
          {visibleDate.toFormat('MMMM yyyy')}
        </h1>

        <Button
          data-testid="month-control-next"
          size={'lg'}
          isIconOnly
          variant="ghost"
          onClick={nextCallback}
          aria-label="next month"
        >
          <ChevronRightIcon />
        </Button>
      </div>
      <Select
        className="max-w-xs"
        defaultSelectedKeys={['myleaves']}
        label="Filter options"
        size="sm"
        onChange={(event) => {
          setSelectedFilter(event.target.value)
        }}
      >
        <SelectItem key={'myleaves'}>{'My Leaves'}</SelectItem>
        <SelectItem key={'allleaves'}>{'All Leaves'}</SelectItem>
        <SelectItem key={'mycircleleaves'}>{"My Circle's Leaves"}</SelectItem>
      </Select>
      <Button
        data-testid="mode-control-today"
        onClick={() => goToToday()}
        color="secondary"
        size="lg"
        className="hidden sm:flex text-black"
      >
        Go to Today
      </Button>
    </div>
  )
}

export default Header
