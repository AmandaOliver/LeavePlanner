import { useAuth0 } from '@auth0/auth0-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'

export type EmployeeType = {
  email: string
  name: string
  organization: number
  managedBy?: string
  country: string
  isOrgOwner: boolean
  paidTimeOff: number
  title: string
  subordinates?: Array<EmployeeType>
  paidTimeOffLeft: number
}

export type CreateEmployeeParamType = {
  email: string
  name: string
  country: string
  paidTimeOff: number
  managedBy: string | null // null if it's the head
  organization: number
  title: string
}

export type UpdateEmployeeParamType = {
  email: string
  name: string
  country: string
  paidTimeOff: number
  title: string
}
export type DeleteEmployeeParamType = {
  email: string
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
      managedBy,
      organization,
      title,
      name,
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
            title,
            name,
            managedBy: managedBy || null,
            organization: organization,
          }),
        }
      )
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
    },
  })
  const updateEmployeeMutation = useMutation({
    mutationFn: async (updateData: UpdateEmployeeParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/employee/${updateData.email}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
          body: JSON.stringify({ ...updateData, email: undefined }),
        }
      )
      if (!response.ok) {
        throw new Error('Failed to update employee')
      }
      return await response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
    },
  })
  const deleteEmployeeMutation = useMutation({
    mutationFn: async (deleteData: DeleteEmployeeParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/employee/${deleteData.email}`,
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
        }
      )
      if (!response.ok) {
        throw new Error('Failed to delete employee')
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
    currentEmployee: employeeQuery.data,
    isLoading: employeeQuery.isLoading,
    createEmployee: createEmployeeMutation.mutateAsync,
    updateEmployee: updateEmployeeMutation.mutateAsync,
    deleteEmployee: deleteEmployeeMutation.mutateAsync,
  }
}
