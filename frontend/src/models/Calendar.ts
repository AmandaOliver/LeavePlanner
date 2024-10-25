import { useAuth0 } from '@auth0/auth0-react'
import { LeaveType } from './Leaves'
import { useEmployeeModel } from './Employee'
import { useQuery } from '@tanstack/react-query'
import { Interval } from 'luxon'

export const useGetMyLeaves = (interval: Interval) => {
  const { getAccessTokenSilently } = useAuth0()
  const { currentEmployee } = useEmployeeModel()

  const fetchMyLeaves = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()
    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/${currentEmployee?.email}?start=${interval.start?.toString().split('T')[0]}&end=${interval.end?.toString().split('T')[0]}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
      }
    )

    if (response.status === 200) {
      return response.json()
    } else {
      throw new Error('Error fetching my leaves')
    }
  }

  return useQuery({
    queryKey: ['myleaves', interval],
    queryFn: fetchMyLeaves,
  })
}
