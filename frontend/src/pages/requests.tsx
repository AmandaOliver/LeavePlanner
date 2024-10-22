import {
  Divider,
  Button,
  Table,
  TableBody,
  TableCell,
  TableColumn,
  TableHeader,
  TableRow,
  useDisclosure,
} from '@nextui-org/react'
import { LeaveType } from '../models/Leaves'
import { RequestIcon } from '../icons/request'
import { EyeIcon } from '../icons/eye'
import { useState } from 'react'
import { LoadingComponent } from '../components/loading'

import { useRequestsModel } from '../models/Requests'
import { RequestReviewModal } from '../components/requestReviewModal'
export const Requests = () => {
  const { requests, isLoading } = useRequestsModel()
  const {
    isOpen: isOpenReviewModal,
    onOpen: onOpenReviewModal,
    onOpenChange: onOpenChangeReviewModal,
  } = useDisclosure()

  const [request, setRequest] = useState<LeaveType>()

  const handleReviewModalOpen = (request: LeaveType) => {
    setRequest(request)
    onOpenReviewModal()
  }

  if (isLoading) return <LoadingComponent />
  return (
    <>
      {request?.id && (
        <RequestReviewModal
          isOpen={isOpenReviewModal}
          onOpenChange={onOpenChangeReviewModal}
          request={request}
        />
      )}

      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <RequestIcon />
          <h1 className=" text-[32px]">Requests to review</h1>
        </div>
        <Divider />
        <Table aria-label="list" className="mt-8">
          <TableHeader>
            <TableColumn>OWNER</TableColumn>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No pending requests to review.'}>
            {requests.map((request) => (
              <TableRow key={request.id}>
                <TableCell>{request.ownerName}</TableCell>
                <TableCell className="hidden sm:table-cell">
                  {new Date(request.dateStart).toDateString()} to{' '}
                  {new Date(request.dateEnd).toDateString()}
                </TableCell>
                <TableCell>{request.description}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap flex-row">
                    <Button
                      color="primary"
                      aria-label="edit"
                      size="sm"
                      onPress={() => handleReviewModalOpen(request)}
                    >
                      Review
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </>
  )
}
