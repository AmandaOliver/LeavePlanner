import { useOrganizationModel } from '../models/Organization'
import { LoadingComponent } from '../components/loading'
import { OrganizationTree } from '../components/organizationTree'
import { TreeIcon } from '../icons/tree'
import { Divider } from '@nextui-org/react'

export const SetupOrganization = () => {
  const { isLoading, currentOrganization } = useOrganizationModel()

  if (isLoading) return <LoadingComponent />

  return (
    <>
      {!currentOrganization.tree.length && (
        <>
          <p>First, setup the head of the organization</p>
          {/* <SetupEmployee isHead={true} /> */}
        </>
      )}
      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <TreeIcon />
          <h1 className=" text-[32px]">Organization hierarchy</h1>

          <Divider />
        </div>
        <OrganizationTree />
      </div>
    </>
  )
}
