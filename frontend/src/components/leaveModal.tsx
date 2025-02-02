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
  RangeValue,
  Select,
  SelectItem,
} from '@nextui-org/react'
import { LeaveType, LeaveTypes, useLeavesModel } from '../models/Leaves'
import { useCallback, useEffect, useState } from 'react'
import { CalendarDate, parseDate } from '@internationalized/date'

export const LeaveModal = ({
  isOpen,
  onOpenChange,
  leave,
  onCloseCb,
}: {
  isOpen: boolean
  onOpenChange: () => void
  leave?: LeaveType
  onCloseCb: () => void
}) => {
  const [dateStart, setDateStart] = useState(leave?.dateStart)
  const [dateEnd, setDateEnd] = useState(
    leave?.dateEnd ||
      new Date(new Date().setDate(new Date().getDate() + 2))
        .toISOString()
        .split('T')[0]
  )
  const [description, setDescription] = useState(leave?.description || '')
  const [type, setType] = useState(leave?.type || 'paidTimeOff')

  const { createLeave, updateLeave, validateLeave } = useLeavesModel()
  const [feedback, setFeedback] = useState<{
    error?: string
    daysRequested?: number
    daysLeftThisYear: number
    daysLeftNextYear: number
  }>()
  const [isLoadingInfo, setIsLoadingInfo] = useState(false)
  const [isLoading, setIsLoading] = useState(false)

  const leaveHandler = async (onClose: () => void) => {
    if (dateStart && dateEnd) {
      setIsLoading(true)
      if (leave) {
        await updateLeave({
          id: leave.id,
          description,
          dateStart,
          dateEnd,
          type,
        })
      } else {
        await createLeave({
          description,
          dateStart,
          dateEnd,
          type,
        })
      }
      setIsLoading(false)
      onCloseCb()
      onClose()
    }
  }

  const validateLeaveHandler = useCallback(
    async ({ start, end }: RangeValue<CalendarDate>) => {
      setDateStart(start.toString())
      setDateEnd(end.add({ days: 1 }).toString())
      setIsLoadingInfo(true)

      setFeedback(
        await validateLeave({
          dateStart: start.toString(),
          dateEnd: end.add({ days: 1 }).toString(),
          id: leave?.id,
          type: type,
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
      type,
    ]
  )
  useEffect(() => {
    const getFeedback = async () => {
      if (dateStart && dateEnd) {
        setIsLoadingInfo(true)
        setFeedback(
          await validateLeave({
            dateStart: parseDate(dateStart).toString(),
            dateEnd: parseDate(dateEnd).toString(),
            id: leave?.id,
            type: type,
          })
        )
        setIsLoadingInfo(false)
      }
    }
    getFeedback()
  }, [dateEnd, dateStart, leave, validateLeave, type])
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
              {leave ? 'Update leave' : 'Request leave'}
            </ModalHeader>
            <ModalBody>
              {feedback?.error ? (
                <Card className="bg-danger w-full text-white p-4">
                  <p className="whitespace-pre-line">{feedback.error}</p>
                </Card>
              ) : (
                type === 'paidTimeOff' &&
                dateStart &&
                dateEnd &&
                feedback &&
                (isLoadingInfo ? (
                  <Skeleton className="rounded-lg">
                    <div className="h-14 rounded-lg bg-default-300"></div>
                  </Skeleton>
                ) : (
                  <Card className="bg-primary w-full text-white p-4">
                    <p>Days Requested: {feedback.daysRequested}</p>

                    {new Date(dateStart).getFullYear() ===
                    new Date(dateEnd).getFullYear() ? (
                      <p>
                        If approved, you'll have {feedback.daysLeftThisYear}{' '}
                        days left in {new Date(dateStart).getFullYear()}.
                      </p>
                    ) : (
                      <p>
                        If approved, you'll have {feedback.daysLeftThisYear}{' '}
                        days left in {new Date(dateStart).getFullYear()} and{' '}
                        {feedback.daysLeftNextYear} days left in{' '}
                        {new Date(dateEnd).getFullYear()}
                      </p>
                    )}
                  </Card>
                ))
              )}
              {
                <Select
                  label="Type"
                  className=""
                  isDisabled={type === 'bankHoliday'}
                  onChange={(event) =>
                    setType(event.target.value as LeaveTypes)
                  }
                  defaultSelectedKeys={[type]}
                >
                  {type !== 'bankHoliday' ? (
                    <>
                      <SelectItem key="paidTimeOff">Paid Time Off</SelectItem>
                      <SelectItem key="statutoryLeave">
                        Statutory Leave
                      </SelectItem>
                    </>
                  ) : (
                    <SelectItem key="bankHoliday">Public Holiday</SelectItem>
                  )}
                </Select>
              }
              <DateRangePicker
                allowsNonContiguousRanges
                visibleMonths={window.innerWidth > 640 ? 2 : 1}
                size="md"
                value={
                  dateStart && dateEnd
                    ? {
                        start: parseDate(dateStart),
                        end: parseDate(dateEnd).add({ days: -1 }),
                      }
                    : undefined
                }
                isRequired
                label="Days on leave"
                minValue={parseDate(
                  new Date(new Date().setDate(new Date().getDate() + 1))
                    .toISOString()
                    .split('T')[0]
                )} // Tomorrow's date
                maxValue={parseDate(
                  new Date(new Date().getFullYear() + 2, 0, 1)
                    .toISOString()
                    .split('T')[0]
                )}
                onChange={(value) => {
                  if (value) {
                    validateLeaveHandler({ start: value.start, end: value.end })
                  }
                }}
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
