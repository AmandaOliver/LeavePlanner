import {
  ChangeEvent,
  FormEvent,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import { LeaveType, LeaveTypes, useLeavesModel } from '../models/Leaves'
import { useEmployeeModel } from '../models/Employee'
import LoadingPage from '../pages/loading'

export const SetupLeave = ({ leave }: { leave?: LeaveType }) => {
  const { createLeave, updateLeave, leaves, leavesAwaitingApproval } =
    useLeavesModel()
  const { currentEmployee } = useEmployeeModel()
  const [description, setDescription] = useState(leave?.description || '')
  const [dateStart, setDateStart] = useState(leave?.dateStart || '')
  const [dateEnd, setDateEnd] = useState(leave?.dateEnd || '')
  const [type, setType] = useState<LeaveTypes>(leave?.type || 'paidTimeOff')
  const totalLeaves = useMemo(
    () => [...leaves, ...leavesAwaitingApproval],
    [leaves, leavesAwaitingApproval]
  )
  const [dateStartError, setdateStartError] = useState<string | null>(null)
  const [dateEndError, setdateEndError] = useState<string | null>(null)

  const dateStartRef = useRef<HTMLInputElement>(null)
  const dateEndRef = useRef<HTMLInputElement>(null)

  const handleDateStartChange = (event: ChangeEvent<HTMLInputElement>) => {
    setDateStart(event.target.value)
  }

  const handleDateEndChange = (event: ChangeEvent<HTMLInputElement>) => {
    setDateEnd(event.target.value)
  }

  const validateDateStart = useCallback(() => {
    const start = new Date(dateStart)

    if (dateStartRef.current && !dateStartRef.current.checkValidity()) {
      setdateStartError('Please enter a valid start date.')
      return false
    } else if (
      totalLeaves.find((l) => {
        const isCurrentLeave = l.id === leave?.id
        const leaveEndDate = new Date(l.dateEnd)
        leaveEndDate.setDate(leaveEndDate.getDate() - 1)
        return (
          !isCurrentLeave &&
          start >= new Date(l.dateStart) &&
          start <= leaveEndDate
        )
      })
    ) {
      setdateStartError(
        'The requested leave start date conflicts with another leave'
      )
      return false
    }
    setdateStartError(null)
    return true
  }, [dateStart, leave, totalLeaves])

  const validateDateEnd = useCallback(() => {
    const start = new Date(dateStart)
    const end = new Date(dateEnd)
    const startYear = start.getFullYear()
    const endYear = end.getFullYear()
    if (!currentEmployee) return
    // Helper function to calculate weekdays between two dates
    const getWeekdaysBetween = (start: Date, end: Date) => {
      let totalDays = 0
      let currentDate = new Date(start)

      while (currentDate <= end) {
        const dayOfWeek = currentDate.getDay()
        if (dayOfWeek !== 0 && dayOfWeek !== 6) {
          // 0 = Sunday, 6 = Saturday
          totalDays++
        }
        currentDate.setDate(currentDate.getDate() + 1)
      }

      return totalDays
    }

    if (dateEndRef.current && !dateEndRef.current.value) {
      setdateEndError('Please enter a valid end date.')
      return false
    } else if (end < start) {
      setdateEndError('The end date cannot be before the start date.')
      return false
    } else if (
      totalLeaves.find((l) => {
        const isCurrentLeave = l.id === leave?.id
        return (
          !isCurrentLeave &&
          end > new Date(l.dateStart) &&
          end <= new Date(l.dateEnd)
        )
      })
    ) {
      setdateEndError(
        'The requested leave end date conflicts with another leave'
      )
      return false
    } else if (startYear !== endYear) {
      // Leave spans multiple years, split the calculation

      // Calculate weekdays for current year
      const endOfYear = new Date(startYear, 11, 31) // December 31 of the start year
      const daysInCurrentYear = getWeekdaysBetween(start, endOfYear)

      // Calculate weekdays for next year
      const startOfNextYear = new Date(endYear, 0, 1) // January 1 of the end year
      const daysInNextYear = getWeekdaysBetween(startOfNextYear, end)

      // Check if employee has enough paid time off for both years
      if (daysInCurrentYear > currentEmployee.paidTimeOffLeft) {
        setdateEndError(
          `You cannot request more days than you have left for the year ${startYear}.`
        )
        return false
      } else if (daysInNextYear > currentEmployee.paidTimeOffLeftNextYear) {
        setdateEndError(
          `You cannot request more days than you have left for the year ${endYear}.`
        )
        return false
      }
    } else if (startYear === endYear) {
      // Same year calculation
      const totalWeekdays = getWeekdaysBetween(start, end)

      if (startYear === new Date().getFullYear()) {
        if (currentEmployee.paidTimeOffLeft < totalWeekdays) {
          setdateEndError('You cannot request more days than you have left.')
          return false
        }
      } else {
        if (currentEmployee.paidTimeOffLeftNextYear < totalWeekdays) {
          setdateEndError('You cannot request more days than you have left.')
          return false
        }
      }
    }

    // If no errors, clear the error message
    setdateEndError(null)
    return true
  }, [dateEnd, dateStart, leave, totalLeaves, currentEmployee])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (leave) {
      await updateLeave({
        id: leave.id,
        description,
        dateStart,
        dateEnd,
        type,
      })
    } else {
      await createLeave({
        description,
        dateStart,
        dateEnd,
        type,
      })
    }
  }

  useEffect(() => {
    if (dateEnd) {
      validateDateEnd()
    }
  }, [dateEnd, validateDateEnd])

  useEffect(() => {
    if (dateStart) {
      validateDateStart()
    }
  }, [dateStart, validateDateStart])

  if (!currentEmployee) return <LoadingPage />

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Enter the start date*</label>
        <input
          type="date"
          name="dateStart"
          id="dateStart"
          value={dateStart}
          onChange={handleDateStartChange}
          onBlur={validateDateStart}
          required
          ref={dateStartRef}
          min={
            new Date(new Date().setDate(new Date().getDate() + 1))
              .toISOString()
              .split('T')[0]
          } // Tomorrow's date
          max={
            new Date(new Date().getFullYear() + 2, 0, 1)
              .toISOString()
              .split('T')[0]
          }
        />
        {dateStartError && <p style={{ color: 'red' }}>{dateStartError}</p>}
      </div>
      <div>
        <label>Enter the end date (you will work this day)*</label>
        <input
          type="date"
          name="dateEnd"
          id="dateEnd"
          value={dateEnd}
          onChange={handleDateEndChange}
          onBlur={validateDateEnd}
          required
          ref={dateEndRef}
          min={dateStart}
          max={
            new Date(new Date().getFullYear() + 2, 0, 2)
              .toISOString()
              .split('T')[0]
          }
        />
        {dateEndError && <p style={{ color: 'red' }}>{dateEndError}</p>}
      </div>
      <div>
        <label>Enter the description *</label>
        <textarea
          id="description"
          name="description"
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          placeholder="I am going to have the trip of my life..."
          required
        />
      </div>
      <div>
        <label htmlFor="leaveType">Select Leave Type:</label>
        <select
          id="leaveType"
          name="leaveType"
          onChange={(event) => setType(event.target.value as LeaveTypes)}
        >
          <option value="paidTimeOff">Paid Time Off</option>
          <option value="unpaidTimeOff">Unpaid Time Off</option>
          <option value="sickLeave">Sick Leave</option>
        </select>
      </div>
      <button type="submit" disabled={!!dateStartError || !!dateEndError}>
        {leave ? 'Update' : 'Create'} Leave
      </button>
    </form>
  )
}
