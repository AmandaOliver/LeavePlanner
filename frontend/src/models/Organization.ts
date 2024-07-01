import { useAuth0 } from '@auth0/auth0-react'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'

export type createOrganizationAndEmployeeResponseBodyType = {
  employeeId: string
  organizationId: number
}

export type OrganizationType = {
  id: string
  name: string
  head: string
}

export const useOrganizationModel = () => {
  const { id } = useParams()
  const { user, getAccessTokenSilently } = useAuth0()
  const [isLoadingOrganization, setIsLoadingOrganization] = useState(false)
  const [currentOrganization, setCurrentOrganization] =
    useState<OrganizationType>()

  useEffect(() => {
    if (id) {
      const getOrganization = async (id: string) => {
        const accessToken = await getAccessTokenSilently()
        if (!user) return false
        const response = await fetch(
          `${process.env.REACT_APP_API_SERVER_URL}/organization/${id}`,
          {
            method: 'GET',
            headers: {
              'Content-Type': 'application/json',
              Authorization: `Bearer ${accessToken}`,
            },
          }
        )
        if (response.status === 200) {
          const organization: OrganizationType = await response.json()
          setCurrentOrganization(organization)
          setIsLoadingOrganization(false)
        }
      }
      setIsLoadingOrganization(true)
      getOrganization(id)
    }
  }, [id, user, getAccessTokenSilently])

  const createOrganizationAndEmployee = async (name: string) => {
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
          picture: user.picture,
          email: user.email,
          name: user.name,
          organizationName: name,
        }),
      }
    )
    if (response.status === 200) {
      return response.json()
    }
  }

  return {
    createOrganizationAndEmployee,
    currentOrganization,
    isLoadingOrganization,
  }
}
