import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  Card,
  Skeleton,
  DateRangePicker,
} from '@nextui-org/react'
import { ConflictType, LeaveType } from '../models/Leaves'
import { parseDate } from '@internationalized/date'
import { useLeaveModel } from '../models/Leave'

export const LeaveInfoModal = ({
  isOpen,
  onOpenChange,
  leave,
  onClose,
}: {
  isOpen: boolean
  onOpenChange: () => void
  leave: LeaveType
  onClose: () => void
}) => {
  const { leaveInfo, isLoading } = useLeaveModel(leave.id)
  return (
    <Modal isOpen={isOpen} onClose={onClose} onOpenChange={onOpenChange}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Leave info
            </ModalHeader>
            <ModalBody className="pb-8">
              {isLoading ? (
                <Skeleton className="rounded-lg">
                  <div className="h-14 rounded-lg bg-default-300"></div>
                </Skeleton>
              ) : (
                <>
                  {leaveInfo?.conflicts && leaveInfo?.conflicts.length > 0 && (
                    <Card className="bg-warning w-full text-white p-4">
                      {leaveInfo.conflicts?.map((conflict: ConflictType) => (
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
                  {leaveInfo?.daysRequested !== undefined && (
                    <Card className="bg-primary w-full text-white p-4">
                      <p>Days Requested: {leaveInfo.daysRequested}</p>
                    </Card>
                  )}
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
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
