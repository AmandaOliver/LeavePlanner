import { useAuth0 } from '@auth0/auth0-react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useEmployeeModel } from './Employee'
import { useQueryClient } from '@tanstack/react-query'
import { LeaveType } from './Leaves'

type ApproveRequestParams = {
  requestId: string
}
export const useRequestsModel = () => {
  const { getAccessTokenSilently } = useAuth0()
  const { currentEmployee } = useEmployeeModel()
  const queryClient = useQueryClient()

  const fetchRequests = async (): Promise<LeaveType[]> => {
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
      return responseJson.map((request: LeaveType) => ({
        ...request,
        dateStart: request.dateStart.split('T')[0],
        dateEnd: request.dateEnd.split('T')[0],
        conflicts: request.conflicts?.map((conflict) => ({
          ...conflict,
          conflictingLeaves: conflict.conflictingLeaves?.map((leave) => ({
            ...leave,
            dateStart: leave.dateStart.split('T')[0],
            dateEnd: leave.dateEnd.split('T')[0],
          })),
        })),
      }))
    } else {
      throw new Error('Failed to fetch requests')
    }
  }
  const requestsQuery = useQuery({
    queryKey: ['requests', currentEmployee?.email],
    queryFn: fetchRequests,
  })
  const approveRequestMutation = useMutation({
    mutationFn: async ({ requestId }: ApproveRequestParams) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/requests/${currentEmployee?.email}/approve/${requestId}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
        }
      )
      if (!response.ok) {
        throw new Error('Failed to approve leave')
      }
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['requests', currentEmployee?.email],
      })
    },
  })
  const rejectRequestMutation = useMutation({
    mutationFn: async ({ requestId }: ApproveRequestParams) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/requests/${currentEmployee?.email}/reject/${requestId}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
        }
      )
      if (!response.ok) {
        throw new Error('Failed to reject leave')
      }
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['requests', currentEmployee?.email],
      })
    },
  })
  return {
    requests: requestsQuery.data ?? [],
    isLoading: requestsQuery.isLoading,
    approveRequest: approveRequestMutation.mutateAsync,
    rejectRequest: rejectRequestMutation.mutateAsync,
  }
}
