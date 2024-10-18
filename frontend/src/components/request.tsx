import { LeaveType } from '../models/Leaves'
import { useRequestsModel } from '../models/Requests'
import { Leave } from './leave'

export const Request = ({ request }: { request: LeaveType }) => {
  const { approveRequest, rejectRequest } = useRequestsModel()
  return (
    <details key={request.id}>
      <summary>
        {request.ownerName} {request.daysRequested} days from{' '}
        {request.dateStart} to {request.dateEnd}: {request.description}
      </summary>
      <p>Conflicts:</p>

      {request.conflicts?.map((conflict) => (
        <details key={conflict.employeeName}>
          <summary>{conflict.employeeName}</summary>
          {conflict.conflictingLeaves?.map((leave) => (
            <Leave leave={leave} isReadOnly></Leave>
          ))}
        </details>
      ))}

      <button onClick={() => approveRequest({ requestId: request.id })}>
        Approve Leave
      </button>
      <button onClick={() => rejectRequest({ requestId: request.id })}>
        Reject Leave
      </button>
      <hr />
    </details>
  )
}
