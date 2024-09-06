import { ChangeEvent, FormEvent, useRef, useState } from 'react'
import { LeaveType, LeaveTypes, useLeavesModel } from '../models/Leaves'

export const RequestLeave = ({
  leave,
  employeeEmail,
}: {
  leave?: LeaveType
  employeeEmail: string
}) => {
  const { createLeave, updateLeave } = useLeavesModel(employeeEmail)
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
    setdateStartError(null)
  }
  const handleDateEndChange = (event: ChangeEvent<HTMLInputElement>) => {
    setDateEnd(event.target.value)
    setdateEndError(null)
  }

  const handleDateStartBlur = () => {
    if (dateStartRef.current && !dateStartRef.current.checkValidity()) {
      setdateStartError('Please enter a valid start date.')
    } else {
      setdateStartError(null)
    }
  }
  const handleDateEndBlur = () => {
    if (dateEndRef.current && !dateEndRef.current.value) {
      setdateEndError('Please enter a valid end date.')
    } else {
      setdateEndError(null)
    }
  }

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
  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Enter the start date *</label>
        <input
          type="date"
          name="dateStart"
          id="dateStart"
          value={dateStart}
          onChange={handleDateStartChange}
          onBlur={handleDateStartBlur}
          required
          ref={dateStartRef}
        />
        {dateStartError && <p style={{ color: 'red' }}>{dateStartError}</p>}
      </div>
      <div>
        <label>Enter the end date *</label>
        <input
          type="date"
          name="dateEnd"
          id="dateEnd"
          value={dateEnd}
          onChange={handleDateEndChange}
          onBlur={handleDateEndBlur}
          required
          ref={dateEndRef}
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
