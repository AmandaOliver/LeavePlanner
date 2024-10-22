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

export const LeaveUpdateModal = ({
  isOpen,
  onOpenChange,
  leave,
  leaves,
  onClose,
}: {
  isOpen: boolean
  onOpenChange: () => void
  leave: LeaveType
  leaves: LeaveType[]
  onClose: () => void
}) => {
  const [dateStart, setDateStart] = useState(leave.dateStart)
  const [dateEnd, setDateEnd] = useState(leave.dateEnd)
  const { createLeave, updateLeave, validateLeave } = useLeavesModel()

  const [feedback, setFeedback] = useState<{
    error?: string
    daysRequested?: number
    conflicts: ConflictType[]
  }>()
  const [isLoading, setIsLoading] = useState(false)
  const [description, setDescription] = useState(leave?.description || '')
  const updateLeaveHandler = async () => {
    setIsLoading(true)
    await updateLeave({
      id: leave.id,
      description,
      dateStart,
      dateEnd,
      type: 'paidTimeOff',
    })
    setIsLoading(false)

    onClose()
  }

  const validateLeaveHandler = useCallback(
    async ({ start, end }: { start: CalendarDate; end: CalendarDate }) => {
      setDateStart(start.toString())
      setDateEnd(end.add({ days: 1 }).toString())
      setIsLoading(true)

      setFeedback(
        await validateLeave({
          dateStart: start.toString(),
          dateEnd: end.add({ days: 1 }).toString(),
          id: leave?.id,
        })
      )

      setIsLoading(false)
    },
    [
      setDateStart,
      setDateEnd,
      setFeedback,
      setIsLoading,
      leave.id,
      validateLeave,
    ]
  )
  useEffect(() => {
    const getFeedback = async () => {
      setIsLoading(true)
      setFeedback(
        await validateLeave({
          dateStart: parseDate(dateStart).toString(),
          dateEnd: parseDate(dateEnd).toString(),
          id: leave?.id,
        })
      )
      setIsLoading(false)
    }
    getFeedback()
  }, [dateEnd, dateStart, leave?.id, validateLeave])
  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      size="lg"
      onOpenChange={onOpenChange}
    >
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Update leave
            </ModalHeader>
            <ModalBody>
              {isLoading ? (
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
                  {feedback?.daysRequested !== undefined && (
                    <Card className="bg-primary w-full text-white p-4">
                      <p>Days Requested: {feedback.daysRequested}</p>
                    </Card>
                  )}
                </>
              )}
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
                onPress={updateLeaveHandler}
                isLoading={isLoading}
              >
                Update
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
