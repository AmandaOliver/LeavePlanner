import {
  Modal,
  ModalContent,
  ModalBody,
  ModalFooter,
  Button,
  ModalHeader,
} from '@nextui-org/react'
import { useState } from 'react'

export const ImportModal = ({
  isOpen,
  onOpenChange,
  onCloseCb,
}: {
  isOpen: boolean
  onOpenChange: () => void
  onCloseCb: () => void
}) => {
  const [isLoading, setIsLoading] = useState(false)
  const importHandler = async (onClose: () => void) => {
    setIsLoading(true)
    setIsLoading(false)
    onCloseCb()
    onClose()
  }
  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange} onClose={onCloseCb}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Import Organization Tree
            </ModalHeader>
            <ModalBody>
              <p>Import CSV</p>
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                color="primary"
                onPress={() => importHandler(onClose)}
                isLoading={isLoading}
              >
                Import
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
