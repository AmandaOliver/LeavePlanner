import {
  Button,
  Card,
  CardBody,
  CardHeader,
  useDisclosure,
} from '@nextui-org/react'
import { OrganizationModal } from '../../components/organizationModal'

export const CreateOrganizationAndEmployee = () => {
  const {
    isOpen: isOpenUpdateModal,
    onOpen: onOpenUpdateModal,
    onOpenChange: onOpenChangeUpdateModal,
  } = useDisclosure()

  return (
    <>
      <OrganizationModal
        isOpen={isOpenUpdateModal}
        onOpenChange={onOpenChangeUpdateModal}
        onCloseCb={() => {}}
      />

      <Card className="bg-default-200 w-[80vw] m-auto mt-8">
        <CardHeader>
          <h1 className="text-[28px] text-bold text-danger-500 m-auto mt-4">
            You are not part of any organization.
          </h1>
        </CardHeader>
        <CardBody>
          {' '}
          <h2 className="text-[22px] m-auto">
            But you can create your own and get started with LeavePlanner
          </h2>
          <div className="p-8 flex flex-wrap flex-col gap-4">
            <Button onPress={onOpenUpdateModal} size="lg" color="primary">
              Get started
            </Button>
          </div>
        </CardBody>
      </Card>
    </>
  )
}
