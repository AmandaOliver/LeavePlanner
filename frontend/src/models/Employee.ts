import { useAuth0 } from '@auth0/auth0-react'
import { useEffect, useState } from 'react'
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
  const [currentEmployee, setCurrentEmployee] = useState<EmployeeType>()
  const [hasCheckedEmployee, setHasCheckedEmployee] = useState(false)
  const navigate = useNavigate()
  const createEmployee = async ({
    email,
    country,
    paidTimeOff,
    isManager,
    managedBy,
    organization,
  }: CreateEmployeeParamType): Promise<EmployeeType> => {
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
  }
  useEffect(() => {
    const getEmployee = async () => {
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
        setCurrentEmployee(await response.json())
      }
      setHasCheckedEmployee(true)
    }
    if (!hasCheckedEmployee) getEmployee()
  }, [getAccessTokenSilently, hasCheckedEmployee, user?.email])

  useEffect(() => {
    if (hasCheckedEmployee && !currentEmployee) {
      navigate('create-organization')
    }
  }, [hasCheckedEmployee, currentEmployee, navigate])

  return { currentEmployee, createEmployee }
}
