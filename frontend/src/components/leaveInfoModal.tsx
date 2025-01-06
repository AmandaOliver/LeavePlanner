import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  Card,
  Skeleton,
  DateRangePicker,
  Button,
  ModalFooter,
} from '@nextui-org/react'
import { LeaveType, useLeaveModel } from '../models/Leaves'
import { parseDate } from '@internationalized/date'

export const LeaveInfoModal = ({
  isOpen,
  onOpenChange,
  leave,
}: {
  isOpen: boolean
  onOpenChange: () => void
  leave: LeaveType
}) => {
  const { leaveInfo, isLoading } = useLeaveModel(leave.id)

  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              {leave.approvedBy
                ? 'Leave'
                : leave.rejectedBy
                  ? 'Request'
                  : 'Request'}{' '}
              info
            </ModalHeader>
            <ModalBody>
              {isLoading ? (
                <Skeleton className="rounded-lg">
                  <div className="h-14 rounded-lg bg-default-300"></div>
                </Skeleton>
              ) : (
                <>
                  <Card className="shadow-none bg-default-100 w-full text-default-600 p-4">
                    <p>
                      Status:{' '}
                      {leave.approvedBy
                        ? 'Approved'
                        : leave.rejectedBy
                          ? 'Rejected'
                          : 'Pending approval'}
                    </p>
                  </Card>
                  <Card className="shadow-none bg-default-100 w-full text-default-600 p-4">
                    <p>
                      Type:{' '}
                      {leave.type === 'bankHoliday'
                        ? 'Public Holiday'
                        : 'Paid Time Off'}
                    </p>
                  </Card>
                  {leaveInfo?.daysRequested !== undefined &&
                    leave.type !== 'bankHoliday' && (
                      <Card className="shadow-none bg-default-100 w-full text-default-600  p-4">
                        <p>Days Requested: {leaveInfo.daysRequested}</p>
                        {!leave.approvedBy &&
                          (new Date(leaveInfo.dateStart).getFullYear() ===
                          new Date(leaveInfo.dateEnd).getFullYear() ? (
                            <p>
                              If approved, you'll have{' '}
                              {leaveInfo.daysLeftThisYear} days left in{' '}
                              {new Date(leaveInfo.dateStart).getFullYear()}.
                            </p>
                          ) : (
                            <p>
                              If approved, you'll have{' '}
                              {leaveInfo.daysLeftThisYear} days left in{' '}
                              {new Date(leaveInfo.dateStart).getFullYear()} and{' '}
                              {leaveInfo.daysLeftNextYear} days left in{' '}
                              {new Date(leaveInfo.dateEnd).getFullYear()}
                            </p>
                          ))}
                      </Card>
                    )}{' '}
                  <DateRangePicker
                    visibleMonths={2}
                    size="md"
                    value={{
                      start: parseDate(leave.dateStart),
                      end: parseDate(leave.dateEnd).add({ days: -1 }),
                    }}
                    label="Days on leave"
                    isReadOnly
                  />
                  <Card className="shadow-none bg-default-100 w-full text-default-600 p-4">
                    <p>Description: {leave.description}</p>
                  </Card>
                </>
              )}
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
