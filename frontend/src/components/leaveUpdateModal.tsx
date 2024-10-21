import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  DateRangePicker,
  ModalFooter,
} from '@nextui-org/react'
import {
  ConflictType,
  LeaveType,
  LeaveTypes,
  useLeavesModel,
} from '../models/Leaves'
import { useState } from 'react'
import { parseDate } from '@internationalized/date'
import LoadingComponent from './loading'

export const LeaveUpdateModal = ({
  isOpen,
  onOpenChange,
  leave,
  leaves,
}: {
  isOpen: boolean
  onOpenChange: () => void
  leave: LeaveType
  leaves: LeaveType[]
}) => {
  const [dateStart, setDateStart] = useState(leave.dateStart)
  const [dateEnd, setDateEnd] = useState(leave.dateEnd)
  const { createLeave, updateLeave, validateLeave } = useLeavesModel()

  const [requestedDays, setRequestedDays] = useState<number | undefined>()
  const [conflicts, setConflicts] = useState<ConflictType[]>([])
  const [errors, setErrors] = useState<string>()
  const [feedback, setFeedback] = useState()
  const [isLoading, setIsLoading] = useState(false)
  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Update leave
            </ModalHeader>
            <ModalBody>
              <DateRangePicker
                allowsNonContiguousRanges
                visibleMonths={2}
                isDateUnavailable={(date) =>
                  leaves.some(
                    (leave) =>
                      new Date(leave.dateStart) <= new Date(date.toString()) &&
                      new Date(leave.dateEnd) > new Date(date.toString())
                  )
                }
                size="md"
                value={{
                  start: parseDate(dateStart),
                  end: parseDate(dateEnd),
                }}
                isRequired
                label="Days on leave"
                minValue={parseDate(
                  new Date(new Date().setDate(new Date().getDate() + 1))
                    .toISOString()
                    .split('T')[0]
                )} // Tomorrow's date
                maxValue={parseDate(
                  new Date(new Date().getFullYear() + 2, 0, 2)
                    .toISOString()
                    .split('T')[0]
                )}
                onChange={async ({ start, end }) => {
                  setDateStart(start.toString())
                  setDateEnd(end.toString())
                  setIsLoading(true)

                  setFeedback(
                    await validateLeave({
                      dateStart,
                      dateEnd: end.toString(),
                      id: leave?.id,
                    })
                  )

                  setIsLoading(false)
                }}
              />
            </ModalBody>
            <ModalFooter>
              {isLoading && <LoadingComponent />}
              {feedback && <p>{feedback}</p>}
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
