import { RequestType, useRequestsModel } from '../models/Requests'

export const Request = ({ request }: { request: RequestType }) => {
  const { approveRequest, rejectRequest } = useRequestsModel()
  return (
    <li key={request.id}>
      <details key={request.id}>
        <summary>
          {request.ownerName} {request.daysRequested} days from{' '}
          {request.dateStart} to {request.dateEnd}: {request.description}
        </summary>
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
