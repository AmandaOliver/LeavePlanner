interface MonthInViewAction {
  type: 'MONTH_IN_VIEW'
  payload: DateTime
}
interface MonthToScrollToAction {
  type: 'MONTH_SCROLL_TO'
  payload: DateTime
}

interface EndOfMonthAction {
  type: 'END_OF_MONTH'
  payload: DateTime
}

interface ClickNextAction {
  type: 'NEXT'
  payload: CalendarState
}

interface ClickPreviousAction {
  type: 'PREVIOUS'
  payload: CalendarState
}
type CalendarModeType = CALENDARMODE.WEEK | CALENDARMODE.MONTH

type selectedFilterType = 'myleaves' | 'allleaves' | 'mycircleleaves'
type CalendarStateType = {
  goToPreviousMonth: () => void
  goToNextMonth: () => void
  goToToday: () => void
  interval: Interval
  visibleDate: DateTime
  leaves?: LeaveType[]
  setSelectedFilter: Dispatch<SetStateAction<selectedFilterType>>
}
