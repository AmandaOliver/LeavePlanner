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
  const summary = (
    <p>
      {leave.type} {!isReadOnly && leave.daysRequested + ' days'} from{' '}
      {leave.dateStart} to {leave.dateEnd}: {leave.description}
    </p>
  )
  if (isReadOnly) return summary
  return (
    <details key={leave.id}>
      <summary>{summary}</summary>
      <p>Conflicts:</p>

      {leave.conflicts?.map((conflict) => (
        <details key={conflict.employeeName}>
          <summary>{conflict.employeeName}</summary>
          {conflict.conflictingLeaves?.map((leave) => (
            <Leave leave={leave} isReadOnly></Leave>
          ))}
        </details>
      ))}
      {leave.type !== 'bankHoliday' &&
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
      {isUpdateLeaveFormOpen && (
        <>
          <SetupLeave leave={leave} />
          <button onClick={() => setIsUpdateLeaveFormOpen(false)}>
            Close update form
          </button>
        </>
      )}
      <hr />
    </details>
  )
}
