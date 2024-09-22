import { useRequestsModel } from '../models/Requests'
import { Request } from '../components/request'
export const Requests = () => {
  const { requests } = useRequestsModel()
  return (
    <ul>
      {requests.map((request) => (
        <Request key={request.id} request={request} />
      ))}
    </ul>
  )
}
