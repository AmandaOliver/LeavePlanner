import { RequestType } from '../models/Requests'

export const Request = ({ request }: { request: RequestType }) => {
  return (
    <li key={request.id}>
      <details key={request.id}>
        <summary>
          {request.ownerName} {request.dateStart} - {request.dateEnd}:{' '}
          {request.description}
        </summary>
        <button onClick={() => console.log('approve leave')}>
          Approve Leave
        </button>
        <button onClick={() => console.log('Reject leave')}>
          Reject Leave
        </button>
      </details>
    </li>
  )
}
