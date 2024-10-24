import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  ModalFooter,
  Button,
  Input,
} from '@nextui-org/react'
import { useRef, useState } from 'react'

import { OrganizationType, useOrganizationModel } from '../models/Organization'

export const OrganizationModal = ({
  isOpen,
  onOpenChange,
  organization,
  onCloseCb,
}: {
  isOpen: boolean
  onOpenChange: () => void
  organization?: OrganizationType
  onCloseCb: () => void
}) => {
  const { renameOrganization, createOrganizationAndEmployee } =
    useOrganizationModel()
  const [orgName, setOrgName] = useState('')
  const nameRef = useRef<HTMLInputElement>(null)
  const [nameError, setNameError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const organizationHandler = async (onClose: () => void) => {
    setIsLoading(true)
    if (organization) {
      await renameOrganization(orgName)
    } else {
      await createOrganizationAndEmployee(orgName)
    }
    setIsLoading(false)
    onCloseCb()
    onClose()
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onCloseCb}
      size="lg"
      onOpenChange={onOpenChange}
    >
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              {organization ? 'Update organization' : 'Create organization'}
            </ModalHeader>
            <ModalBody>
              <Input
                type="text"
                name="orgName"
                label="Your organization name"
                id="orgName"
                value={orgName}
                onChange={(e) => {
                  setOrgName(e.target.value)
                  setNameError(null)
                }}
                onBlur={() => {
                  if (nameRef.current && !nameRef.current.value) {
                    setNameError('Enter a name for your organization')
                  } else {
                    setNameError(null)
                  }
                }}
                placeholder="Enter your Organization name"
                isRequired
                isInvalid={!!nameError}
                errorMessage={nameError}
                ref={nameRef}
                className="flex w-full"
              />
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                onPress={() => organizationHandler(onClose)}
                color="primary"
                isLoading={isLoading}
                isDisabled={!!nameError}
              >
                {organization ? 'Update' : 'Next step'}
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
