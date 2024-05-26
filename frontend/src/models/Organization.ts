import { useAuth0 } from '@auth0/auth0-react'

export type createOrganizationAndEmployeeResponseBodyType = {
  employeeId: string
  organizationId: number
}

export const useOrganizationModel = () => {
  const { user, getAccessTokenSilently } = useAuth0()
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
          employeeId: user.sub,
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

  return { createOrganizationAndEmployee }
}
