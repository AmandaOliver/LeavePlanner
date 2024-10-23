import {
  Modal,
  ModalContent,
  ModalBody,
  ModalFooter,
  Button,
  ModalHeader,
} from '@nextui-org/react'
import { useState } from 'react'
import { EmployeeType, useEmployeeModel } from '../models/Employee'

export const DeleteEmployeeModal = ({
  isOpen,
  onOpenChange,
  employee,
}: {
  isOpen: boolean
  onOpenChange: () => void
  employee: EmployeeType
}) => {
  const { deleteEmployee } = useEmployeeModel()
  const [isLoading, setIsLoading] = useState(false)

  const deleteEmployeeHandler = async (onClose: () => void) => {
    setIsLoading(true)
    await deleteEmployee({ email: employee.email })
    setIsLoading(false)
    onClose()
  }

  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Delete employee
            </ModalHeader>
            <ModalBody>
              <p>
                Are you sure you want to delete this employee? This action is
                irreversible.
              </p>
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                color="danger"
                onPress={() => deleteEmployeeHandler(onClose)}
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
