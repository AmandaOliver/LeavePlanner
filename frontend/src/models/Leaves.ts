import { useAuth0 } from '@auth0/auth0-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEmployeeModel } from './Employee'
export type LeaveTypes =
  | 'sickLeave'
  | 'paidTimeOff'
  | 'unpaidTimeOff'
  | 'bankHoliday'

export type LeaveType = {
  id: string
  dateStart: string
  dateEnd: string
  description: string
  type: LeaveTypes
  owner: string
}
export type CreateLeaveParamType = {
  description: string
  dateStart: string
  dateEnd: string
  type: LeaveTypes
}
export type UpdateLeaveParamType = {
  id: string
  description: string
  dateStart: string
  dateEnd: string
  type: LeaveTypes
}
export type DeleteLeaveParamType = {
  id: string
}
export const useLeavesModel = (employeeEmail: string) => {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()
  const { currentEmployee } = useEmployeeModel()
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
  const createLeaveMutation = useMutation({
    mutationFn: async (createData: CreateLeaveParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/leaves`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            ...createData,
            owner: currentEmployee?.email,
          }),
        }
      )
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['leaves', employeeEmail],
      })
    },
  })
  const updateLeaveMutation = useMutation({
    mutationFn: async (updateData: UpdateLeaveParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/leaves/${updateData.id}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({ ...updateData }),
        }
      )
      if (!response.ok) {
        throw new Error('Failed to update leave')
      }
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['leaves', employeeEmail],
      })
    },
  })
  const deleteLeaveMutation = useMutation({
    mutationFn: async (deleteData: DeleteLeaveParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/leaves/${deleteData.id}`,
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
        }
      )
      if (!response.ok) {
        throw new Error('Failed to delete leave')
      }
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
    },
  })
  return {
    leaves: leavesQuery.data ?? [],
    createLeave: createLeaveMutation.mutateAsync,
    updateLeave: updateLeaveMutation.mutateAsync,
    deleteLeave: deleteLeaveMutation.mutateAsync,
    isLoading: leavesQuery.isLoading,
    isError: leavesQuery.isError,
    error: leavesQuery.error,
  }
}
