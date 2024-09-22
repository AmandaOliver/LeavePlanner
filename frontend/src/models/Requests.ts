import { useAuth0 } from '@auth0/auth0-react'
import { useQuery } from '@tanstack/react-query'
import { useEmployeeModel } from './Employee'
import { LeaveTypes } from './Leaves'
export type RequestType = {
  id: number
  type: LeaveTypes
  dateStart: string
  dateEnd: string
  description: string
  ownerName: string
}
export const useRequestsModel = () => {
  const { getAccessTokenSilently } = useAuth0()
  const { currentEmployee } = useEmployeeModel()
  const fetchRequests = async (): Promise<RequestType[]> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/requests/${currentEmployee?.email}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
      }
    )

    if (response.ok) {
      const responseJson = await response.json()
      return responseJson.map((request: RequestType) => ({
        ...request,
        dateStart: request.dateStart.split('T')[0],
        dateEnd: request.dateEnd.split('T')[0],
      }))
    } else {
      return []
    }
  }
  const requestsQuery = useQuery({
    queryKey: ['requests', currentEmployee?.email],
    queryFn: fetchRequests,
  })

  return {
    requests: requestsQuery.data ?? [],
    isLoading: requestsQuery.isLoading,
  }
}
