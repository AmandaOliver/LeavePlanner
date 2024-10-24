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

type BaseChannel = Partial<ChannelInterface>

type CalendarStateType = {
  calendarMode: CalendarModeType
  goToPreviousMonth: () => void
  goToPreviousWeek: () => void
  goToNextMonth: () => void
  goToNextWeek: () => void
  goToToday: () => void
  interval: Interval
  setCalendarMode: Dispatch<SetStateAction<CalendarModeType>>
  setInterval: Dispatch<SetStateAction<DateTime>>
  setVisibleDate: Dispatch<SetStateAction<DateTime>>
  visibleDate: DateTime
}
