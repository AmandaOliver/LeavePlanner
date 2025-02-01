import { useAuth0 } from '@auth0/auth0-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEmployeeModel } from './Employees'
import { useNavigate } from 'react-router-dom'
import { Interval } from 'luxon'

export type LeaveTypes = 'paidTimeOff' | 'bankHoliday'

export type ConflictType = {
  employeeId: string
  employeeName: string
  conflictingLeaves: LeaveType[]
}
export type LeaveType = {
  id: string
  dateStart: string
  dateEnd: string
  description: string
  type: LeaveTypes
  owner: string
  ownerName?: string
  approvedBy: string
  rejectedBy: string
  daysRequested: number
  daysLeftThisYear?: number
  daysLeftNextYear?: number
  conflicts?: ConflictType[]
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
export type ValidateLeaveParamType = {
  dateStart: string
  dateEnd: string
  id?: string
  type: LeaveTypes
}
export type DeleteLeaveParamType = {
  id: string
}
export const useGetMyLeaves = (interval: Interval) => {
  const { getAccessTokenSilently } = useAuth0()
  const { currentEmployee } = useEmployeeModel()

  const fetchMyLeaves = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()
    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/myleaves/${currentEmployee?.id}?start=${interval.start?.toString().split('T')[0]}&end=${interval.end?.toString().split('T')[0]}`,
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
export const useGetAllLeaves = (interval: Interval) => {
  const { getAccessTokenSilently } = useAuth0()
  const { currentEmployee } = useEmployeeModel()

  const fetchAllLeaves = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()
    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/all/${currentEmployee?.organization}?start=${interval.start?.toString().split('T')[0]}&end=${interval.end?.toString().split('T')[0]}`,
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
    queryKey: ['allleaves', interval],
    queryFn: fetchAllLeaves,
  })
}

export const useGetMyCircleLeaves = (interval: Interval) => {
  const { getAccessTokenSilently } = useAuth0()
  const { currentEmployee } = useEmployeeModel()

  const fetchAllLeaves = async (): Promise<LeaveType[]> => {
    const accessToken = await getAccessTokenSilently()
    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/circle/${currentEmployee?.id}?start=${interval.start?.toString().split('T')[0]}&end=${interval.end?.toString().split('T')[0]}`,
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
    queryKey: ['mycircleleaves', interval],
    queryFn: fetchAllLeaves,
  })
}
export const useLeavesModel = () => {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()
  const { currentEmployee } = useEmployeeModel()
  const navigate = useNavigate()
  const usePaginatedLeaves = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['leaves', currentEmployee?.id, page, pageSize],
      queryFn: async (): Promise<{
        leaves: LeaveType[]
        totalCount: number
      }> => {
        const accessToken = await getAccessTokenSilently()

        const response = await fetch(
          `${process.env.REACT_APP_API_SERVER_URL}/leaves/approved/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
            leaves: responseJson.leaves.map((leave: LeaveType) => ({
              ...leave,
              dateStart: leave.dateStart.split('T')[0],
              dateEnd: leave.dateEnd.split('T')[0],
            })),
            totalCount: responseJson.totalCount,
          }
        } else {
          throw new Error('Failed to fetch leaves')
        }
      },
      placeholderData: (prevData) => prevData,
    })
  const usePaginatedPastLeaves = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['pastLeaves', currentEmployee?.id, page, pageSize],
      queryFn: async (): Promise<{
        leaves: LeaveType[]
        totalCount: number
      }> => {
        const accessToken = await getAccessTokenSilently()

        const response = await fetch(
          `${process.env.REACT_APP_API_SERVER_URL}/leaves/past/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
            leaves: responseJson.leaves.map((leave: LeaveType) => ({
              ...leave,
              dateStart: leave.dateStart.split('T')[0],
              dateEnd: leave.dateEnd.split('T')[0],
            })),
            totalCount: responseJson.totalCount,
          }
        } else {
          throw new Error('Failed to fetch past leaves')
        }
      },
      placeholderData: (prevData) => prevData,
    })
  const usePaginatedLeavesPending = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['leavesAwaitingApproval', currentEmployee?.id, page, pageSize],
      queryFn: async (): Promise<{
        leaves: LeaveType[]
        totalCount: number
      }> => {
        const accessToken = await getAccessTokenSilently()

        const response = await fetch(
          `${process.env.REACT_APP_API_SERVER_URL}/leaves/pending/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
            leaves: responseJson.leaves.map((leave: LeaveType) => ({
              ...leave,
              dateStart: leave.dateStart.split('T')[0],
              dateEnd: leave.dateEnd.split('T')[0],
            })),
            totalCount: responseJson.totalCount,
          }
        } else {
          throw new Error('Failed to fetch pending leaves')
        }
      },
      placeholderData: (prevData) => prevData,
    })
  const usePaginatedLeavesRejected = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['leavesRejected', currentEmployee?.id, page, pageSize],
      queryFn: async (): Promise<{
        leaves: LeaveType[]
        totalCount: number
      }> => {
        const accessToken = await getAccessTokenSilently()

        const response = await fetch(
          `${process.env.REACT_APP_API_SERVER_URL}/leaves/rejected/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
            leaves: responseJson.leaves.map((leave: LeaveType) => ({
              ...leave,
              dateStart: leave.dateStart.split('T')[0],
              dateEnd: leave.dateEnd.split('T')[0],
            })),
            totalCount: responseJson.totalCount,
          }
        } else {
          throw new Error('Failed to fetch rejected leaves')
        }
      },
      placeholderData: (prevData) => prevData,
    })
  const createLeaveMutation = useMutation({
    mutationFn: async (createData: CreateLeaveParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/leaves/${currentEmployee?.id}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            ...createData,
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
        queryKey: ['leavesAwaitingApproval', currentEmployee?.id],
      })
      queryClient.invalidateQueries({
        queryKey: ['leaves', currentEmployee?.id],
      })
      navigate('/requests')
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
            owner: currentEmployee?.id,
          }),
        }
      )
      if (!response.ok) {
        throw new Error('Failed to update leave')
      }
      return await response.json()
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['leaves', currentEmployee?.id],
      })
      await queryClient.invalidateQueries({
        queryKey: ['leavesAwaitingApproval', currentEmployee?.id],
      })
      await queryClient.invalidateQueries({
        queryKey: ['employee', currentEmployee?.id],
      })
      await queryClient.invalidateQueries({
        queryKey: ['leavesRejected', currentEmployee?.id],
      })
      navigate('/requests')
    },
  })
  const validateLeaveMutation = useMutation({
    mutationFn: async (validateData: ValidateLeaveParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/leaves/validate/${currentEmployee?.id}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            ...validateData,
          }),
        }
      )
      if (response.ok) {
        const responseJson = await response.json()
        return {
          ...responseJson,
          dateStart: responseJson.dateStart.split('T')[0],
          dateEnd: responseJson.dateEnd.split('T')[0],
        }
      }

      return { error: await response.json() }
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
        queryKey: ['leaves', currentEmployee?.id],
      })
      queryClient.invalidateQueries({
        queryKey: ['leavesAwaitingApproval', currentEmployee?.id],
      })
      queryClient.invalidateQueries({
        queryKey: ['leavesRejected', currentEmployee?.id],
      })
    },
  })

  return {
    usePaginatedLeaves,
    usePaginatedPastLeaves,
    usePaginatedLeavesPending,
    usePaginatedLeavesRejected,
    createLeave: createLeaveMutation.mutateAsync,
    updateLeave: updateLeaveMutation.mutateAsync,
    deleteLeave: deleteLeaveMutation.mutateAsync,
    validateLeave: validateLeaveMutation.mutateAsync,
  }
}

export const useLeaveModel = (id: string) => {
  const { getAccessTokenSilently } = useAuth0()
  const leaveQuery = useQuery({
    queryKey: ['leave', id],
    queryFn: async () => {
      const accessToken = await getAccessTokenSilently()

      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/leaves/${id}`,
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
