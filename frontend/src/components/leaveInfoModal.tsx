import { Modal, ModalContent, ModalBody } from '@nextui-org/react'

export const LeaveInfoModal = ({
  isOpen,
  onOpenChange,
}: {
  isOpen: boolean
  onOpenChange: () => void
}) => (
  <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
    <ModalContent>
      {() => (
        <ModalBody>
          <p>See info</p>
        </ModalBody>
      )}
    </ModalContent>
  </Modal>
)
