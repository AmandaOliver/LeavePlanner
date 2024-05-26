import { useAuth0 } from '@auth0/auth0-react'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
type EmployeeType = {
  id: string
  picture: string
  email: string
  name: string
  organization: number
  managedBy?: string
  country?: string
  isManager: boolean
  isOrgOwner: boolean
  paidTimeOff: number
}
export const useEmployeeModel = () => {
  const { user, getAccessTokenSilently } = useAuth0()
  const [currentEmployee, setCurrentEmployee] = useState<EmployeeType>()
  const [hasCheckedEmployee, setHasCheckedEmployee] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    const getEmployee = async () => {
      const accessToken = await getAccessTokenSilently()

      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/employee/${user?.sub}`,
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
  }, [])

  useEffect(() => {
    if (hasCheckedEmployee && !currentEmployee) {
      navigate('create-organization')
    }
  }, [hasCheckedEmployee, currentEmployee, navigate])

  return { currentEmployee }
}
