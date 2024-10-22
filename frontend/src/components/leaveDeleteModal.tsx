import {
  Modal,
  ModalContent,
  ModalBody,
  ModalFooter,
  Button,
  ModalHeader,
} from '@nextui-org/react'
import { LeaveType, useLeavesModel } from '../models/Leaves'
import { useState } from 'react'

export const LeaveDeleteModal = ({
  isOpen,
  onOpenChange,
  leave,
}: {
  isOpen: boolean
  onOpenChange: () => void
  leave: LeaveType
}) => {
  const { deleteLeave } = useLeavesModel()
  const [isLoading, setIsLoading] = useState(false)

  const deleteLeaveHandler = async (onClose: () => void) => {
    setIsLoading(true)
    await deleteLeave({ id: leave.id })
    setIsLoading(false)
    onClose()
  }

  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Delete leave
            </ModalHeader>
            <ModalBody>
              <p>
                Are you sure you want to delete this leave? This action is
                irreversible.
              </p>
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                color="danger"
                onPress={() => deleteLeaveHandler(onClose)}
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
