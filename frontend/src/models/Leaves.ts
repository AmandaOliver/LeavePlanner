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
  approvedBy: string
  rejectedBy: string
  daysRequested: number
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

export const useLeavesModel = () => {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()
  const { currentEmployee } = useEmployeeModel()
  const fetchLeaves = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/${currentEmployee?.email}`,
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
      return responseJson?.map((leave: LeaveType) => ({
        ...leave,
        dateStart: leave.dateStart.split('T')[0],
        dateEnd: leave.dateEnd.split('T')[0],
      }))
    } else {
      throw new Error('Failed to fetch approved leaves')
    }
  }
  const fetchLeavesAwaitingApproval = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/pending/${currentEmployee?.email}`,
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
      return responseJson?.map((leave: LeaveType) => ({
        ...leave,
        dateStart: leave.dateStart.split('T')[0],
        dateEnd: leave.dateEnd.split('T')[0],
      }))
    } else {
      throw new Error('Failed to fetch pending leaves')
    }
  }
  const fetchLeavesRejected = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/rejected/${currentEmployee?.email}`,
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
      return responseJson?.map((leave: LeaveType) => ({
        ...leave,
        dateStart: leave.dateStart.split('T')[0],
        dateEnd: leave.dateEnd.split('T')[0],
      }))
    } else {
      throw new Error('Failed to fetch rejected leaves')
    }
  }
  const leavesQuery = useQuery({
    queryKey: ['leaves', currentEmployee?.email],
    queryFn: fetchLeaves,
  })
  const leavesAwaitingApprovalQuery = useQuery({
    queryKey: ['leavesAwaitingApproval', currentEmployee?.email],
    queryFn: fetchLeavesAwaitingApproval,
  })
  const leavesRejectedQuery = useQuery({
    queryKey: ['leavesRejected', currentEmployee?.email],
    queryFn: fetchLeavesRejected,
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
      if (!response.ok) {
        throw new Error('Failed to create leave')
      }
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['leavesAwaitingApproval', currentEmployee?.email],
      })
      queryClient.invalidateQueries({
        queryKey: ['leaves', currentEmployee?.email],
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
          body: JSON.stringify({
            ...updateData,
            owner: currentEmployee?.email,
          }),
        }
      )
      if (!response.ok) {
        throw new Error('Failed to update leave')
      }
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['leaves', currentEmployee?.email],
      })
      queryClient.invalidateQueries({
        queryKey: ['leavesAwaitingApproval', currentEmployee?.email],
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
        queryKey: ['leaves', currentEmployee?.email],
      })
      queryClient.invalidateQueries({
        queryKey: ['leavesAwaitingApproval', currentEmployee?.email],
      })
      queryClient.invalidateQueries({
        queryKey: ['leavesRejected', currentEmployee?.email],
      })
    },
  })
  return {
    leaves: leavesQuery.data ?? [],
    leavesAwaitingApproval: leavesAwaitingApprovalQuery.data ?? [],
    leavesRejected: leavesRejectedQuery.data ?? [],
    createLeave: createLeaveMutation.mutateAsync,
    updateLeave: updateLeaveMutation.mutateAsync,
    deleteLeave: deleteLeaveMutation.mutateAsync,
    isLoading:
      leavesQuery.isLoading ||
      leavesAwaitingApprovalQuery.isLoading ||
      leavesRejectedQuery.isLoading,
  }
}
