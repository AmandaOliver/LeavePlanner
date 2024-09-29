import { RequestType, useRequestsModel } from '../models/Requests'
import { Leave } from './leave'

export const Request = ({ request }: { request: RequestType }) => {
  const { approveRequest, rejectRequest } = useRequestsModel()
  return (
    <li key={request.id}>
      <details key={request.id}>
        <summary>
          {request.ownerName} {request.daysRequested} days from{' '}
          {request.dateStart} to {request.dateEnd}: {request.description}
        </summary>
        <p>Conflicts:</p>
        <ul>
          {request.conflicts?.map((conflict) => (
            <li>
              <details key={conflict.employeeName}>
                <summary>{conflict.employeeName}</summary>
                {conflict.conflictingLeaves?.map((leave) => (
                  <Leave leave={leave} isReadOnly></Leave>
                ))}
              </details>
            </li>
          ))}
        </ul>
        <button onClick={() => approveRequest({ requestId: request.id })}>
          Approve Leave
        </button>
        <button onClick={() => rejectRequest({ requestId: request.id })}>
          Reject Leave
        </button>
      </details>
    </li>
  )
}
