import { useAuth0 } from '@auth0/auth0-react'
import { useQuery } from '@tanstack/react-query'

export type LeaveType = {
  id: number
  dateStart: string
  dateEnd: string
  description: string
  type: 'sickLeave' | 'paidTimeOff' | 'unpaidTimeOff' | 'bankHoliday'
}

export const useLeavesModel = (employeeEmail: string) => {
  const { getAccessTokenSilently } = useAuth0()

  const fetchLeaves = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/${employeeEmail}`,
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
      throw new Error('Error fetching leaves')
    }
  }

  const leavesQuery = useQuery({
    queryKey: ['leaves', employeeEmail],
    queryFn: fetchLeaves,
  })

  return {
    leaves: leavesQuery.data ?? [],
    isLoading: leavesQuery.isLoading,
    isError: leavesQuery.isError,
    error: leavesQuery.error,
  }
}
