import { FormEvent, useState } from 'react'
import { LeaveType, useLeavesModel } from '../models/Leaves'
import { useEmployeeModel } from '../models/Employee'
import LoadingPage from '../pages/loading'

export const SetupLeave = ({ leave }: { leave?: LeaveType }) => {
  const { createLeave, updateLeave } = useLeavesModel()
  const { currentEmployee } = useEmployeeModel()
  const [description, setDescription] = useState(leave?.description || '')
  const [dateStart, setDateStart] = useState(leave?.dateStart || '')
  const [dateEnd, setDateEnd] = useState(leave?.dateEnd || '')

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (leave) {
      await updateLeave({
        id: leave.id,
        description,
        dateStart,
        dateEnd,
        type: 'paidTimeOff',
      })
    } else {
      await createLeave({
        description,
        dateStart,
        dateEnd,
        type: 'paidTimeOff',
      })
    }
  }

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
          onChange={(event) => setDateStart(event.target.value)}
          required
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
      </div>
      <div>
        <label>Enter the end date (you will work this day)*</label>
        <input
          type="date"
          name="dateEnd"
          id="dateEnd"
          value={dateEnd}
          onChange={(event) => setDateEnd(event.target.value)}
          required
          min={dateStart}
          max={
            new Date(new Date().getFullYear() + 2, 0, 2)
              .toISOString()
              .split('T')[0]
          }
        />
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
      <button type="submit">{leave ? 'Update' : 'Request'} Leave</button>
    </form>
  )
}
