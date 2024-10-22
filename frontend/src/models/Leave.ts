import { useAuth0 } from '@auth0/auth0-react'
import { useQuery } from '@tanstack/react-query'
import { LeaveType } from './Leaves'

export const useLeaveModel = (id: string) => {
  const { getAccessTokenSilently } = useAuth0()
  const leaveQuery = useQuery({
    queryKey: ['leave', id],
    queryFn: async () => {
      const accessToken = await getAccessTokenSilently()

      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/leave/${id}`,
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
        return {
          ...responseJson,
          dateStart: responseJson.dateStart.split('T')[0],
          dateEnd: responseJson.dateEnd.split('T')[0],
        }
      } else {
        throw new Error('Failed to fetch leave')
      }
    },
  })
  return {
    leaveInfo: leaveQuery.data,
    isLoading: leaveQuery.isLoading,
  }
}
