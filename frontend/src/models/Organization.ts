import { useAuth0 } from '@auth0/auth0-react'
import {
  useQuery,
  useMutation,
  useQueryClient,
  UseQueryResult,
} from '@tanstack/react-query'
import { EmployeeType, useEmployeeModel } from './Employee'

export type createOrganizationAndEmployeeResponseBodyType = {
  organizationId: number
}
export type WorkingDaysType = number[]
export type OrganizationType = {
  id: number
  name: string
  head: string
  tree: EmployeeType[]
  workingDays: WorkingDaysType
}

export const useOrganizationModel = () => {
  const { currentEmployee } = useEmployeeModel()
  const { user, getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  const fetchOrganization = async (organizationId: number) => {
    const accessToken = await getAccessTokenSilently()
    if (!user) return false
    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/organization/${organizationId}`,
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
      throw new Error('Failed to fetch organization')
    }
  }

  const organizationQuery: UseQueryResult<OrganizationType, Error> = useQuery({
    queryKey: ['organization', currentEmployee?.organization],
    queryFn: () => {
      if (currentEmployee?.organization) {
        return fetchOrganization(currentEmployee.organization)
      }
      return Promise.reject('Organization ID is undefined')
    },
    enabled: !!currentEmployee?.organization,
  })

  const createOrganizationAndEmployeeMutation = useMutation({
    mutationFn: async (name: string) => {
      const accessToken = await getAccessTokenSilently()
      if (!user) return false
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/create-employee-organization`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            email: user.email,
            organizationName: name,
          }),
        }
      )
      if (response.status === 200) {
        return response.json()
      } else {
        throw new Error('Failed to create organization and employee')
      }
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee'],
      })
    },
  })
  const importOrganizationMutation = useMutation({
    mutationFn: async (formData: FormData) => {
      const accessToken = await getAccessTokenSilently()
      if (!user) return false
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/organization/import/${organizationQuery.data?.id}`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
          body: formData,
        }
      )
      if (response.status === 200) {
        return response.text()
      } else {
        const error = await response.json()
        throw new Error(error.detail)
      }
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee'],
      })
    },
  })
  const deleteOrganizationMutation = useMutation({
    mutationFn: async () => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/organization/${organizationQuery.data?.id}`,
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
        }
      )
      if (response.status === 200) {
        return response.json()
      } else {
        throw new Error('Failed to delete organization')
      }
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee'],
      })
    },
  })
  const renameOrganizationMutation = useMutation({
    mutationFn: async (newName: string) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/organization/${organizationQuery.data?.id}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            name: newName,
          }),
        }
      )
      if (response.status === 200) {
        return response.json()
      } else {
        throw new Error('Failed to update organization')
      }
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee'],
      })
    },
  })
  const updateWorkingDaysMutation = useMutation({
    mutationFn: async (workingDays: WorkingDaysType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/organization/${organizationQuery.data?.id}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            workingDays,
          }),
        }
      )
      if (!response.ok) {
        return { error: await response.json() }
      }
      return await response.json()
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee'],
      })
    },
  })
  return {
    createOrganizationAndEmployee:
      createOrganizationAndEmployeeMutation.mutateAsync,
    deleteOrganization: deleteOrganizationMutation.mutateAsync,
    renameOrganization: renameOrganizationMutation.mutateAsync,
    updateWorkingDays: updateWorkingDaysMutation.mutateAsync,
    importOrganization: importOrganizationMutation.mutateAsync,
    currentOrganization: organizationQuery.data,
    isLoading: organizationQuery.isLoading,
  }
}
