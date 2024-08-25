import { useAuth0 } from '@auth0/auth0-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'

export type EmployeeType = {
  id?: string
  picture?: string
  email: string
  name?: string
  organization: number
  managedBy?: string
  country?: string
  isManager: boolean
  isOrgOwner: boolean
  paidTimeOff: number
}

export type CreateEmployeeParamType = {
  email: string
  country: string
  paidTimeOff: number
  isManager: boolean
  managedBy?: string
  organization: number
}

export const useEmployeeModel = () => {
  const { user, getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  const fetchEmployee = async (): Promise<EmployeeType | null> => {
    const accessToken = await getAccessTokenSilently()
    const response = await fetch(
      `${process.env.REACT_APP_API_SERVER_URL}/employee/${user?.email}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
      }
    )
    if (response.status === 200) {
      return await response.json()
    } else if (response.status === 404) {
      return null
    } else {
      throw new Error('error fetching employee')
    }
  }

  const employeeQuery = useQuery({
    queryKey: ['employee', user?.email],
    queryFn: fetchEmployee,
    enabled: !!user?.email, // Only run the query if the email is available
  })

  const createEmployeeMutation = useMutation({
    mutationFn: async ({
      email,
      country,
      paidTimeOff,
      isManager,
      managedBy,
      organization,
    }: CreateEmployeeParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/employee`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({
            email,
            country,
            paidTimeOff,
            isManager,
            managedBy: managedBy || null,
            organization: organization,
          }),
        }
      )
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['employee', user?.email],
      })
    },
  })

  return {
    currentEmployee: employeeQuery.data,
    isLoading: employeeQuery.isLoading,
    createEmployee: createEmployeeMutation.mutateAsync,
  }
}
