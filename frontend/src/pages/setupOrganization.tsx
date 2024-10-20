import { useOrganizationModel } from '../models/Organization'
import LoadingPage from '../components/loading'
import { OrganizationTree } from '../components/organizationTree'
import { SetupEmployee } from '../components/organizationTree/components/setupEmployee'

export const SetupOrganization = () => {
  const { isLoading, currentOrganization } = useOrganizationModel()

  if (isLoading) return <LoadingPage />

  return (
    <>
      {!currentOrganization.tree.length && (
        <>
          First, setup the head of the organization
          <SetupEmployee isHead={true} />
        </>
      )}

      <OrganizationTree />
    </>
  )
}
