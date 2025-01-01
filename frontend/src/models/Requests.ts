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

  const fetchRequests = async (
    page: number,
    pageSize: number
  ): Promise<{ requests: LeaveType[]; totalCount: number }> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/requests/review/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
        requests: responseJson.requests.map((request: LeaveType) => ({
          ...request,
          dateStart: request.dateStart.split('T')[0],
          dateEnd: request.dateEnd.split('T')[0],
        })),
        totalCount: responseJson.totalCount,
      }
    } else {
      throw new Error('Failed to fetch requests')
    }
  }
  const fetchReviewedRequests = async (
    page: number,
    pageSize: number
  ): Promise<{ requests: LeaveType[]; totalCount: number }> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/requests/reviewed/${currentEmployee?.id}?page=${page}&pageSize=${pageSize}`,
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
        requests: responseJson.requests.map((request: LeaveType) => ({
          ...request,
          dateStart: request.dateStart.split('T')[0],
          dateEnd: request.dateEnd.split('T')[0],
        })),
        totalCount: responseJson.totalCount,
      }
    } else {
      throw new Error('Failed to fetch reviewed requests')
    }
  }
  const fetchRequestInfo = async (requestId: string): Promise<LeaveType> => {
    const accessToken = await getAccessTokenSilently()

    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/requests/${requestId}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
      }
    )

    if (response.ok) {
      return await response.json()
    } else {
      throw new Error('Failed to fetch request info')
    }
  }
  const useRequestInfo = (requestId: string) =>
    useQuery({
      queryKey: ['requestInfo', requestId],
      queryFn: () => fetchRequestInfo(requestId),
    })
  const usePaginatedRequests = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['requests', currentEmployee?.id, page, pageSize],
      queryFn: () => fetchRequests(page, pageSize),
      placeholderData: (prevData) => prevData,
    })
  const usePaginatedReviewedRequests = (page: number, pageSize: number) =>
    useQuery({
      queryKey: ['reviewedRequests', currentEmployee?.id, page, pageSize],
      queryFn: () => fetchReviewedRequests(page, pageSize),
      placeholderData: (prevData) => prevData,
    })
  const approveRequestMutation = useMutation({
    mutationFn: async ({ requestId }: ApproveRequestParams) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/requests/${currentEmployee?.id}/approve/${requestId}`,
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
        queryKey: ['requests', currentEmployee?.id],
      })
      queryClient.invalidateQueries({
        queryKey: ['reviewedRequests', currentEmployee?.id],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee', currentEmployee?.id],
      })
    },
  })
  const rejectRequestMutation = useMutation({
    mutationFn: async ({ requestId }: ApproveRequestParams) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/requests/${currentEmployee?.id}/reject/${requestId}`,
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
        queryKey: ['requests', currentEmployee?.id],
      })
      queryClient.invalidateQueries({
        queryKey: ['reviewedRequests', currentEmployee?.id],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee', currentEmployee?.id],
      })
    },
  })
  return {
    usePaginatedRequests,
    usePaginatedReviewedRequests,
    useRequestInfo,
    approveRequest: approveRequestMutation.mutateAsync,
    rejectRequest: rejectRequestMutation.mutateAsync,
  }
}
