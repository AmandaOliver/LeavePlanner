import { useState } from 'react'
import { LeaveType, useLeavesModel } from '../models/Leaves'
import { SetupLeave } from './setupLeave'
import { EmployeeType } from '../models/Employee'

export const Leave = ({
  leave,
  employee,
}: {
  leave: LeaveType
  employee: EmployeeType
}) => {
  const [isUpdateLeaveFormOpen, setIsUpdateLeaveFormOpen] = useState(false)
  const { deleteLeave } = useLeavesModel(employee.email)
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
            <SetupLeave leave={leave} employee={employee} />
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