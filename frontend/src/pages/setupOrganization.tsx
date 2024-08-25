import { useOrganizationModel } from '../models/Organization'

import LoadingPage from './loading'

export const SetupOrganization = () => {
  const { currentOrganization, isLoading } = useOrganizationModel()
  if (isLoading) return <LoadingPage />

  return <p>setup the organization {currentOrganization.name}</p>
}
