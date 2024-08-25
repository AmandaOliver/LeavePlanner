import { useAuth0 } from '@auth0/auth0-react'
import { useQuery } from '@tanstack/react-query'

export type CountryType = {
  code: string
  name: string
}

export type CountriesType = Array<CountryType>

export const useCountriesModel = () => {
  const { getAccessTokenSilently } = useAuth0()

  const fetchCountries = async (): Promise<CountriesType> => {
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
      return response.json()
    } else {
      throw new Error('Error fetching countries')
    }
  }

  const countriesQuery = useQuery({
    queryKey: ['countries'],
    queryFn: fetchCountries,
  })

  return {
    countries: countriesQuery.data ?? [],
    isLoading: countriesQuery.isLoading,
    isError: countriesQuery.isError,
    error: countriesQuery.error,
  }
}
