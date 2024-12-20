import { useAuth0 } from '@auth0/auth0-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEmployeeModel } from './Employee'
import { useNavigate } from 'react-router-dom'

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

export const useLeavesModel = () => {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()
  const { currentEmployee } = useEmployeeModel()
  const navigate = useNavigate()
  const fetchLeaves = async (
    page: number,
    pageSize: number
  ): Promise<{ leaves: LeaveType[]; totalCount: number }> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/leaves/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
  }
  const fetchPastLeaves = async (
    page: number,
    pageSize: number
  ): Promise<{ leaves: LeaveType[]; totalCount: number }> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/pastleaves/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
  }
  const fetchLeavesAwaitingApproval = async (
    page: number,
    pageSize: number
  ): Promise<{ leaves: LeaveType[]; totalCount: number }> => {
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
  }

  const fetchLeavesRejected = async (
    page: number,
    pageSize: number
  ): Promise<{ leaves: LeaveType[]; totalCount: number }> => {
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
  }

  const usePaginatedLeaves = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['leaves', currentEmployee?.id, page, pageSize],
      queryFn: () => fetchLeaves(page, pageSize),
      placeholderData: (prevData) => prevData,
    })
  const usePaginatedPastLeaves = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['pastLeaves', currentEmployee?.id, page, pageSize],
      queryFn: () => fetchPastLeaves(page, pageSize),
      placeholderData: (prevData) => prevData,
    })
  const usePaginatedLeavesAwaitingApproval = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['leavesAwaitingApproval', currentEmployee?.id, page, pageSize],
      queryFn: () => fetchLeavesAwaitingApproval(page, pageSize),
      placeholderData: (prevData) => prevData,
    })

  const usePaginatedLeavesRejected = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['leavesRejected', currentEmployee?.id, page, pageSize],
      queryFn: () => fetchLeavesRejected(page, pageSize),
      placeholderData: (prevData) => prevData,
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
            owner: currentEmployee?.id,
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
        `${process.env.REACT_APP_API_SERVER_URL}/leaves/validate`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            ...validateData,
            owner: currentEmployee?.id,
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
    usePaginatedLeavesAwaitingApproval,
    usePaginatedLeavesRejected,
    createLeave: createLeaveMutation.mutateAsync,
    updateLeave: updateLeaveMutation.mutateAsync,
    deleteLeave: deleteLeaveMutation.mutateAsync,
    validateLeave: validateLeaveMutation.mutateAsync,
  }
}
