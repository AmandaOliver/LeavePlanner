import { useAuth0 } from '@auth0/auth0-react'
import { useEffect, useState } from 'react'
export type CountryType = {
  code: string
  name: string
}
export type CountriesType = Array<CountryType>
export const useCountriesModel = () => {
  const { getAccessTokenSilently } = useAuth0()
  const [countries, setCountries] = useState<CountriesType>([])
  useEffect(() => {
    const getCountries = async () => {
      const accessToken = await getAccessTokenSilently()

      const response = await fetch(
        `${process.env.REACT_APP_API_SERVER_URL}/countries`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
          },
        }
      )
      if (response.status === 200) {
        setCountries(await response.json())
      } else {
        throw Error('Error fetching countries')
      }
    }
    getCountries()
  }, [getAccessTokenSilently])

  return { countries }
}
