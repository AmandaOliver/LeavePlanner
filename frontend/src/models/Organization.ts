import { useAuth0 } from '@auth0/auth0-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { EmployeeType, useEmployeeModel } from './Employee'

export type createOrganizationAndEmployeeResponseBodyType = {
  organizationId: number
}

export type OrganizationType = {
  id: number
  name: string
  head: string
  tree: EmployeeType
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

  const organizationQuery = useQuery({
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
        queryKey: ['organization', data.organizationId],
      })
      queryClient.invalidateQueries({
        queryKey: ['employee', user?.email],
      })
    },
  })

  return {
    createOrganizationAndEmployee:
      createOrganizationAndEmployeeMutation.mutateAsync,
    currentOrganization: organizationQuery.data,
    isLoading: organizationQuery.isLoading,
  }
}
