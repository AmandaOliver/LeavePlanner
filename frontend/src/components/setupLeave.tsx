import {
  ChangeEvent,
  FormEvent,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react'
import { LeaveType, LeaveTypes, useLeavesModel } from '../models/Leaves'
import { useEmployeeModel } from '../models/Employee'
import LoadingPage from '../pages/loading'

export const SetupLeave = ({ leave }: { leave?: LeaveType }) => {
  const { createLeave, updateLeave } = useLeavesModel()
  const { currentEmployee } = useEmployeeModel()
  const [description, setDescription] = useState(leave?.description || '')
  const [dateStart, setDateStart] = useState(leave?.dateStart || '')
  const [dateEnd, setDateEnd] = useState(leave?.dateEnd || '')
  const [type, setType] = useState<LeaveTypes>(leave?.type || 'paidTimeOff')

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
    if (dateStartRef.current && !dateStartRef.current.checkValidity()) {
      setdateStartError('Please enter a valid start date.')
      return false
    }
    setdateStartError(null)
    return true
  }, [])

  const validateDateEnd = useCallback(() => {
    const start = new Date(dateStart)
    const end = new Date(dateEnd)

    if (!currentEmployee) return

    if (dateEndRef.current && !dateEndRef.current.value) {
      setdateEndError('Please enter a valid end date.')
      return false
    } else if (end < start) {
      setdateEndError('The end date cannot be before the start date.')
      return false
    }
    // If no errors, clear the error message
    setdateEndError(null)
    return true
  }, [dateEnd, dateStart, currentEmployee])

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
