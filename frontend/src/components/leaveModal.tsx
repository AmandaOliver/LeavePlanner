import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  DateRangePicker,
  ModalFooter,
  Card,
  Button,
  Textarea,
  Skeleton,
} from '@nextui-org/react'
import { ConflictType, LeaveType, useLeavesModel } from '../models/Leaves'
import { useCallback, useEffect, useState } from 'react'
import { CalendarDate, parseDate } from '@internationalized/date'
import { useEmployeeModel } from '../models/Employee'

export const LeaveModal = ({
  isOpen,
  onOpenChange,
  leave,
  leaves,
  onCloseCb,
}: {
  isOpen: boolean
  onOpenChange: () => void
  leave?: LeaveType
  leaves: LeaveType[]
  onCloseCb: () => void
}) => {
  const [dateStart, setDateStart] = useState(
    leave?.dateStart ||
      new Date(new Date().setDate(new Date().getDate() + 1))
        .toISOString()
        .split('T')[0]
  )
  const [dateEnd, setDateEnd] = useState(
    leave?.dateEnd ||
      new Date(new Date().setDate(new Date().getDate() + 2))
        .toISOString()
        .split('T')[0]
  )
  const [description, setDescription] = useState(leave?.description || '')

  const { createLeave, updateLeave, validateLeave } = useLeavesModel()
  const { currentEmployee } = useEmployeeModel()
  const [feedback, setFeedback] = useState<{
    error?: string
    daysRequested?: number
    conflicts: ConflictType[]
  }>()
  const [isLoadingInfo, setIsLoadingInfo] = useState(false)
  const [isLoading, setIsLoading] = useState(false)

  const leaveHandler = async (onClose: () => void) => {
    setIsLoading(true)
    if (leave) {
      await updateLeave({
        id: leave.id,
        description,
        dateStart,
        dateEnd,
        type: 'paidTimeOff',
      })
    } else {
      await createLeave({
        description,
        dateStart,
        dateEnd,
        type: 'paidTimeOff',
      })
    }
    setIsLoading(false)
    onCloseCb()
    onClose()
  }

  const validateLeaveHandler = useCallback(
    async ({ start, end }: { start: CalendarDate; end: CalendarDate }) => {
      setDateStart(start.toString())
      setDateEnd(end.add({ days: 1 }).toString())
      setIsLoadingInfo(true)

      setFeedback(
        await validateLeave({
          dateStart: start.toString(),
          dateEnd: end.add({ days: 1 }).toString(),
          id: leave?.id,
        })
      )

      setIsLoadingInfo(false)
    },
    [
      setDateStart,
      setDateEnd,
      setFeedback,
      setIsLoadingInfo,
      leave?.id,
      validateLeave,
    ]
  )
  useEffect(() => {
    const getFeedback = async () => {
      setIsLoadingInfo(true)
      setFeedback(
        await validateLeave({
          dateStart: parseDate(dateStart).toString(),
          dateEnd: parseDate(dateEnd).toString(),
          id: leave?.id,
        })
      )
      setIsLoadingInfo(false)
    }
    getFeedback()
  }, [dateEnd, dateStart, leave, validateLeave])
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
              {leave ? 'Update leave' : 'Request a leave'}
            </ModalHeader>
            <ModalBody>
              {isLoadingInfo ? (
                <Skeleton className="rounded-lg">
                  <div className="h-14 rounded-lg bg-default-300"></div>
                </Skeleton>
              ) : (
                <>
                  {feedback?.error && (
                    <Card className="bg-danger w-full text-white p-4">
                      <p className="whitespace-pre-line">{feedback.error}</p>
                    </Card>
                  )}

                  {feedback?.conflicts && feedback?.conflicts.length > 0 && (
                    <Card className="bg-warning w-full text-white p-4">
                      {feedback.conflicts?.map((conflict) => (
                        <details key={conflict.employeeName}>
                          <summary>{conflict.employeeName} is on leave</summary>
                          {conflict.conflictingLeaves?.map((leave) => (
                            <p>
                              - from {new Date(leave.dateStart).toDateString()}{' '}
                              until {new Date(leave.dateEnd).toDateString()}
                            </p>
                          ))}
                        </details>
                      ))}
                    </Card>
                  )}
                  {feedback?.daysRequested !== undefined &&
                    currentEmployee?.paidTimeOffLeft && (
                      <Card className="bg-primary w-full text-white p-4">
                        <p>Days Requested: {feedback.daysRequested}</p>

                        {new Date(dateStart).getFullYear() ===
                          new Date(dateEnd).getFullYear() && (
                          <p>
                            If approved, you'll have{' '}
                            {currentEmployee.paidTimeOffLeft -
                              feedback.daysRequested}{' '}
                            days left in {new Date(dateStart).getFullYear()}.
                          </p>
                        )}
                      </Card>
                    )}
                </>
              )}
              <DateRangePicker
                allowsNonContiguousRanges
                visibleMonths={window.innerWidth > 640 ? 2 : 1}
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
                  end: parseDate(dateEnd).add({ days: -1 }),
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
                onChange={validateLeaveHandler}
              />
              <Textarea
                label="Description"
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                placeholder="I am going to have the trip of my life..."
              />
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                color="primary"
                isDisabled={!!feedback && !!feedback?.error}
                onPress={() => leaveHandler(onClose)}
                isLoading={isLoading}
              >
                {leave ? 'Update' : 'Request'}
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
