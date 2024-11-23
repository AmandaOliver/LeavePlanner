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
import { ImportModal } from '../components/importModal'
import { BussinessWatchIcon } from '../icons/bussinesswatch'

export const SetupOrganization = () => {
  const { isLoading, currentOrganization } = useOrganizationModel()
  const {
    isOpen: isOpenHeadModal,
    onOpen: onOpenHeadModal,
    onOpenChange: onOpenChangeHeadModal,
  } = useDisclosure()
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
  const {
    isOpen: isOpenImportModal,
    onOpen: onOpenImportModal,
    onOpenChange: onOpenChangeImportModal,
  } = useDisclosure()

  const [updateOrganization, setUpdateOrganization] = useState(
    {} as OrganizationType
  )
  const [headOrganization, setHeadOrganization] = useState(false)
  const [importOrganization, setImportOrganization] = useState(false)

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
      {headOrganization && (
        <EmployeeModal
          isOpen={isOpenHeadModal}
          onOpenChange={onOpenChangeHeadModal}
          label={'Set the head employee'}
          onCloseCb={() => setHeadOrganization(false)}
        />
      )}
      {importOrganization && (
        <ImportModal
          isOpen={isOpenImportModal}
          onOpenChange={onOpenChangeImportModal}
          onCloseCb={() => setImportOrganization(false)}
        />
      )}
      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <BussinessWatchIcon />
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
          <h1 className=" text-[32px]">Organization tree</h1>

          <Divider />
        </div>
        {!currentOrganization.tree.length && (
          <Card className="bg-default-200 p-4 m-4">
            <p className="p-4 m-auto">No employees registered.</p>
            <div className="flex flex-wrap flex-row justify-center gap-4">
              <Button
                color="primary"
                onPress={() => {
                  setImportOrganization(true)
                  onOpenImportModal()
                }}
              >
                Import Full Tree From CSV
              </Button>
              <p className="self-center">or</p>
              <Button
                color="primary"
                variant="bordered"
                onPress={() => {
                  setHeadOrganization(true)
                  onOpenHeadModal()
                }}
              >
                Create Employees Manually
              </Button>
            </div>
          </Card>
        )}
        <OrganizationTree />
      </div>
    </>
  )
}
