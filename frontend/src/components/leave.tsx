import { useState } from 'react'
import { LeaveType, useLeavesModel } from '../models/Leaves'
import { RequestLeave } from './requestLeave'

export const Leave = ({
  leave,
  employeeEmail,
}: {
  leave: LeaveType
  employeeEmail: string
}) => {
  const [isUpdateLeaveFormOpen, setIsUpdateLeaveFormOpen] = useState(false)
  const { deleteLeave } = useLeavesModel(employeeEmail)
  return (
    <li key={leave.id}>
      <details key={leave.id}>
        <summary>
          {leave.type} {leave.dateStart} - {leave.dateEnd}: {leave.description}
        </summary>
        <button onClick={() => setIsUpdateLeaveFormOpen(true)}>
          Update Leave
        </button>
        {isUpdateLeaveFormOpen && (
          <>
            <RequestLeave leave={leave} employeeEmail={employeeEmail} />
            <button onClick={() => setIsUpdateLeaveFormOpen(false)}>
              Close update form
            </button>
          </>
        )}

        <button onClick={() => deleteLeave({ id: leave.id })}>
          Delete Leave
        </button>
      </details>
    </li>
  )
}
