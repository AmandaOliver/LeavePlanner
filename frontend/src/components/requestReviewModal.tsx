import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  Card,
  DateRangePicker,
  Button,
  ModalFooter,
} from '@nextui-org/react'
import { ConflictType, LeaveType } from '../models/Leaves'
import { parseDate } from '@internationalized/date'

import { useRequestsModel } from '../models/Requests'
import { useState } from 'react'

export const RequestReviewModal = ({
  isOpen,
  onOpenChange,
  request,
}: {
  isOpen: boolean
  onOpenChange: () => void
  request: LeaveType
}) => {
  const { approveRequest, rejectRequest } = useRequestsModel()
  const [isLoadingApprove, setIsLoadingApprove] = useState(false)
  const [isLoadingReject, setIsLoadingReject] = useState(false)
  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Review request
            </ModalHeader>
            <ModalBody>
              <>
                <Card className="shadow-none bg-default-100 w-full text-default-600  p-4">
                  <p>
                    Type:{' '}
                    {request.type === 'bankHoliday'
                      ? 'Bank Holiday'
                      : 'Paid Time Off'}
                  </p>
                </Card>
                <Card className="shadow-none bg-default-100 w-full text-default-600 p-4">
                  <p>Owner: {request.ownerName}</p>
                </Card>
                {request?.conflicts && request?.conflicts.length > 0 && (
                  <Card className="shadow-none bg-warning w-full text-white  p-4">
                    {request.conflicts?.map((conflict: ConflictType) => (
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
                {request?.daysRequested !== undefined && (
                  <Card className="shadow-none bg-default-100 w-full text-default-600  p-4">
                    <p>Days Requested: {request.daysRequested}</p>
                  </Card>
                )}

                <DateRangePicker
                  visibleMonths={2}
                  size="md"
                  value={{
                    start: parseDate(request.dateStart),
                    end: parseDate(request.dateEnd).add({ days: -1 }),
                  }}
                  label="Days on leave"
                  isReadOnly
                />
                <Card className="shadow-none bg-default-100 w-full text-default-600 p-4">
                  <p>Description: {request.description}</p>
                </Card>
              </>
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>

              <Button
                color="primary"
                isLoading={isLoadingApprove}
                onPress={async () => {
                  setIsLoadingApprove(true)
                  await approveRequest({ requestId: request.id })
                  onClose()
                }}
              >
                Approve
              </Button>

              <Button
                color="danger"
                isLoading={isLoadingReject}
                onPress={async () => {
                  setIsLoadingReject(true)
                  await rejectRequest({ requestId: request.id })
                  onClose()
                }}
              >
                Reject
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
