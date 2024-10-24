import { OrganizationType, useOrganizationModel } from '../models/Organization'
import { LoadingComponent } from '../components/loading'
import { OrganizationTree } from '../components/organizationTree'
import { TreeIcon } from '../icons/tree'
import {
  Button,
  Card,
  CardBody,
  CardFooter,
  Divider,
  useDisclosure,
} from '@nextui-org/react'
import { EmployeeModal } from '../components/employeeModal'
import { OrganizationDeleteModal } from '../components/organizationDeleteModal'
import { OrganizationModal } from '../components/organizationModal'
import { useState } from 'react'

export const SetupOrganization = () => {
  const { isLoading, currentOrganization } = useOrganizationModel()
  const {
    isOpen: isOpenUpdateModal,
    onOpen: onOpenUpdateModal,
    onOpenChange: onOpenChangeUpdateModal,
  } = useDisclosure()
  const {
    isOpen: isOpenDeleteModal,
    onOpen: onOpenDeleteModal,
    onOpenChange: onOpenChangeDeleteModal,
  } = useDisclosure()

  const [updateOrganization, setUpdateOrganization] = useState(
    {} as OrganizationType
  )
  if (isLoading) return <LoadingComponent />

  return (
    <>
      <OrganizationDeleteModal
        isOpen={isOpenDeleteModal}
        onOpenChange={onOpenChangeDeleteModal}
      />
      {updateOrganization?.id && (
        <OrganizationModal
          isOpen={isOpenUpdateModal}
          onOpenChange={onOpenChangeUpdateModal}
          organization={updateOrganization}
          onCloseCb={() => setUpdateOrganization({} as OrganizationType)}
        />
      )}
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
          <h1 className=" text-[32px]">Organization info</h1>

          <Divider />
        </div>
        <Card className="bg-default-200 p-4 m-4">
          <CardBody>
            <h1 className=" text-[24px]">Name: {currentOrganization.name}</h1>
          </CardBody>
          <CardFooter>
            <div className="flex flex-wrap flex-row gap-4 ">
              <Button
                color="primary"
                onPress={() => {
                  setUpdateOrganization(currentOrganization)
                  onOpenUpdateModal()
                }}
              >
                Rename
              </Button>
              <Button color="danger" onPress={onOpenDeleteModal}>
                Delete
              </Button>
            </div>
          </CardFooter>
        </Card>
      </div>
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
