import { useAuth0 } from '@auth0/auth0-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'

export type EmployeeType = {
  id: string
  email: string
  name: string
  organization: number
  managedBy?: string
  country: string
  isOrgOwner: boolean
  paidTimeOff: number
  managerName: string
  title: string
  subordinates?: Array<EmployeeType>
  paidTimeOffLeft: number
  paidTimeOffLeftNextYear: number
  pendingRequests: number
}

export type CreateEmployeeParamType = {
  email: string
  name: string
  country: string
  paidTimeOff: number
  managedBy: string | null // null if it's the head
  organization: number
  title: string
  isOrgOwner: boolean
}

export type UpdateEmployeeParamType = {
  id: string
  email: string
  name: string
  country: string
  paidTimeOff: number
  title: string
  isOrgOwner: boolean
}
export type DeleteEmployeeParamType = {
  id: string
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
    enabled: !!user?.email,
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
      isOrgOwner,
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
            isOrgOwner,
          }),
        }
      )
      if (!response.ok) {
        return { error: await response.json() }
      }
      return await response.json()
    },
    onSuccess: (data) => {
      if (data.email === user?.email) {
        queryClient.invalidateQueries({
          queryKey: ['employee'],
        })
      }

      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
    },
  })
  const updateEmployeeMutation = useMutation({
    mutationFn: async (updateData: UpdateEmployeeParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/employee/${updateData.id}`,
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
        return { error: await response.json() }
      }
      return await response.json()
    },
    onSuccess: (data) => {
      if (data.email === user?.email) {
        queryClient.invalidateQueries({
          queryKey: ['employee'],
        })
      }
      queryClient.invalidateQueries({
        queryKey: ['organization'],
      })
    },
  })
  const deleteEmployeeMutation = useMutation({
    mutationFn: async (deleteData: DeleteEmployeeParamType) => {
      const accessToken = await getAccessTokenSilently()
      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/employee/${deleteData.id}`,
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
        }
      )
      if (!response.ok) {
        return { error: await response.json() }
      }
      return await response.json()
    },
    onSuccess: (data) => {
      if (data.email === user?.email) {
        queryClient.invalidateQueries({
          queryKey: ['employee'],
        })
      }
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
