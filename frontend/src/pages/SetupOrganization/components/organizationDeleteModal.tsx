import {
  Modal,
  ModalContent,
  ModalBody,
  ModalFooter,
  Button,
  ModalHeader,
} from '@nextui-org/react'
import { useState } from 'react'
import { useOrganizationModel } from '../../../models/Organization'

export const OrganizationDeleteModal = ({
  isOpen,
  onOpenChange,
}: {
  isOpen: boolean
  onOpenChange: () => void
}) => {
  const { deleteOrganization } = useOrganizationModel()
  const [isLoading, setIsLoading] = useState(false)

  const deleteOrganizationHandler = async (onClose: () => void) => {
    setIsLoading(true)
    await deleteOrganization()
    setIsLoading(false)
    onClose()
  }

  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Delete organization
            </ModalHeader>
            <ModalBody>
              <p>
                Are you sure you want to delete this organization? This action
                is irreversible.
              </p>
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                color="danger"
                onPress={() => deleteOrganizationHandler(onClose)}
                isLoading={isLoading}
              >
                Delete
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
