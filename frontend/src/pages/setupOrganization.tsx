import { useOrganizationModel } from '../models/Organization'
import { LoadingComponent } from '../components/loading'
import { OrganizationTree } from '../components/organizationTree'
import { TreeIcon } from '../icons/tree'
import { Divider, useDisclosure } from '@nextui-org/react'
import { EmployeeModal } from '../components/employeeModal'
import { FirstIcon } from '../icons/first'

export const SetupOrganization = () => {
  const { isLoading, currentOrganization } = useOrganizationModel()

  if (isLoading) return <LoadingComponent />

  return (
    <>
      {!currentOrganization.tree.length && (
        <EmployeeModal
          isOpen={true}
          onOpenChange={() => {}}
          onCloseCb={() => {}}
          label={'Set the head employee'}
        />
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
