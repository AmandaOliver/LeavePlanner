import { FormEvent, useState } from 'react'
import { LeaveType, useLeavesModel, ConflictType } from '../models/Leaves'
import { useEmployeeModel } from '../models/Employee'
import LoadingPage from './loading'
import { Leave } from './leave'

export const SetupLeave = ({ leave }: { leave?: LeaveType }) => {
  const { createLeave, updateLeave, validateLeave } = useLeavesModel()
  const { currentEmployee } = useEmployeeModel()
  const [description, setDescription] = useState(leave?.description || '')
  const [dateStart, setDateStart] = useState(leave?.dateStart || '')
  const [dateEnd, setDateEnd] = useState(leave?.dateEnd || '')
  const [isLoading, setIsLoading] = useState(false)
  const [requestedDays, setRequestedDays] = useState<number | undefined>()
  const [conflicts, setConflicts] = useState<ConflictType[]>([])
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

  if (!currentEmployee || isLoading) return <LoadingPage />

  return (
    <form onSubmit={handleSubmit}>
      
  
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
      {requestedDays !== undefined && <p>Requested days: {requestedDays}</p>}
      {conflicts && conflicts.length > 0 && <p>Conflicts:</p>}
      {conflicts?.map((conflict) => (
        <details key={conflict.employeeName}>
          <summary>{conflict.employeeName}</summary>
          {conflict.conflictingLeaves?.map((leave) => (
            <Leave leave={leave} isReadOnly></Leave>
          ))}
        </details>
      ))}
      <button type="submit" disabled={!requestedDays}>
        {leave ? 'Update' : 'Request'} Leave
      </button>
    </form>
  )
}
