import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  ModalFooter,
  Button,
  Switch,
  Card,
} from '@nextui-org/react'
import { useState } from 'react'

import {
  useOrganizationModel,
  WorkingDaysType,
} from '../../models/Organization'

export const WorkingDaysModal = ({
  isOpen,
  onOpenChange,
  onCloseCb,
}: {
  isOpen: boolean
  onOpenChange: () => void
  onCloseCb: () => void
}) => {
  const { updateWorkingDays, currentOrganization } = useOrganizationModel()

  const [workingDays, setWorkingDays] = useState<WorkingDaysType>(
    currentOrganization?.workingDays || [1, 2, 3, 4, 5]
  )
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(false)

  const organizationHandler = async (onClose: () => void) => {
    setIsLoading(true)
    const response = await updateWorkingDays(workingDays)

    setIsLoading(false)
    if (response?.error) {
      setError(response.error)
    } else {
      onCloseCb()
      onClose()
    }
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
              {'Update working days'}
            </ModalHeader>
            <ModalBody>
              {error && (
                <Card className="bg-danger w-full text-white p-4">
                  <p className="whitespace-pre-line">{error}</p>
                </Card>
              )}

              <Switch
                isSelected={workingDays.includes(1)}
                onValueChange={(value) => {
                  if (value) {
                    setWorkingDays((prev) => {
                      const newArray = [1, ...prev]
                      return newArray.sort()
                    })
                  } else {
                    setWorkingDays((prev) => prev.filter((day) => day !== 1))
                  }
                }}
              >
                Monday
              </Switch>
              <Switch
                isSelected={workingDays.includes(2)}
                onValueChange={(value) => {
                  if (value) {
                    setWorkingDays((prev) => {
                      const newArray = [2, ...prev]
                      return newArray.sort()
                    })
                  } else {
                    setWorkingDays((prev) => prev.filter((day) => day !== 2))
                  }
                }}
              >
                Tuesday
              </Switch>
              <Switch
                isSelected={workingDays.includes(3)}
                onValueChange={(value) => {
                  if (value) {
                    setWorkingDays((prev) => {
                      const newArray = [3, ...prev]
                      return newArray.sort()
                    })
                  } else {
                    setWorkingDays((prev) => prev.filter((day) => day !== 3))
                  }
                }}
              >
                Wednesday
              </Switch>
              <Switch
                isSelected={workingDays.includes(4)}
                onValueChange={(value) => {
                  if (value) {
                    setWorkingDays((prev) => {
                      const newArray = [4, ...prev]
                      return newArray.sort()
                    })
                  } else {
                    setWorkingDays((prev) => prev.filter((day) => day !== 4))
                  }
                }}
              >
                Thursday
              </Switch>
              <Switch
                isSelected={workingDays.includes(5)}
                onValueChange={(value) => {
                  if (value) {
                    setWorkingDays((prev) => {
                      const newArray = [5, ...prev]
                      return newArray.sort()
                    })
                  } else {
                    setWorkingDays((prev) => prev.filter((day) => day !== 5))
                  }
                }}
              >
                Friday
              </Switch>
              <Switch
                isSelected={workingDays.includes(6)}
                onValueChange={(value) => {
                  if (value) {
                    setWorkingDays((prev) => {
                      const newArray = [6, ...prev]
                      return newArray.sort()
                    })
                  } else {
                    setWorkingDays((prev) => prev.filter((day) => day !== 6))
                  }
                }}
              >
                Saturday
              </Switch>
              <Switch
                isSelected={workingDays.includes(7)}
                onValueChange={(value) => {
                  if (value) {
                    setWorkingDays((prev) => {
                      const newArray = [7, ...prev]
                      return newArray.sort()
                    })
                  } else {
                    setWorkingDays((prev) => prev.filter((day) => day !== 7))
                  }
                }}
              >
                Sunday
              </Switch>
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                onPress={() => organizationHandler(onClose)}
                color="primary"
                isLoading={isLoading}
              >
                {'Update'}
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
