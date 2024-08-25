import { useOrganizationModel } from '../models/Organization'
import LoadingPage from './loading'
import { SetupEmployee } from '../components/setupEmployee'
import { OrganizationTree } from '../components/organizationTree'

export const SetupOrganization = () => {
  const { isLoading, currentOrganization } = useOrganizationModel()

  if (isLoading) return <LoadingPage />

  return (
    <>
      {!currentOrganization.tree && <SetupEmployee managerEmail={null} />}

      <OrganizationTree />
    </>
  )
}
