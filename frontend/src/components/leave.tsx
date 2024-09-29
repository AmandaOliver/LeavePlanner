import { useState } from 'react'
import { LeaveType, useLeavesModel } from '../models/Leaves'
import { SetupLeave } from './setupLeave'

export const Leave = ({
  leave,
  isReadOnly = false,
}: {
  leave: LeaveType
  isReadOnly?: boolean
}) => {
  const [isUpdateLeaveFormOpen, setIsUpdateLeaveFormOpen] = useState(false)
  const { deleteLeave } = useLeavesModel()
  return (
    <li key={leave.id}>
      <details key={leave.id}>
        <summary>
          {leave.type} {!isReadOnly && leave.daysRequested + ' days'} from{' '}
          {leave.dateStart} to {leave.dateEnd}: {leave.description}
        </summary>
        {!isReadOnly &&
          leave.type !== 'bankHoliday' &&
          (leave.approvedBy == null ||
            (new Date(leave.dateStart) > new Date() &&
              new Date(leave.dateEnd) > new Date())) && (
            <>
              {leave.rejectedBy == null && (
                <button onClick={() => setIsUpdateLeaveFormOpen(true)}>
                  Update Leave
                </button>
              )}
              <button onClick={() => deleteLeave({ id: leave.id })}>
                Delete Leave
              </button>
            </>
          )}
        {!isReadOnly && isUpdateLeaveFormOpen && (
          <>
            <SetupLeave leave={leave} />
            <button onClick={() => setIsUpdateLeaveFormOpen(false)}>
              Close update form
            </button>
          </>
        )}
      </details>
    </li>
  )
}
