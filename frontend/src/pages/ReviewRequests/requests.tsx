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
  Pagination,
} from '@nextui-org/react'
import { useState } from 'react'
import { LoadingComponent } from '../../components/loading'
import { BussinessWatchIcon } from '../../icons/bussinesswatch'
import { EyeIcon } from '../../icons/eye'
import { PartyIcon } from '../../icons/party'
import { RequestIcon } from '../../icons/request'
import { LeaveType } from '../../models/Leaves'
import { useRequestsModel } from '../../models/Requests'
import { RequestReviewModal } from './requestReviewModal'

export const Requests = () => {
  const { usePaginatedRequests, usePaginatedReviewedRequests } =
    useRequestsModel()
  const [requestsPage, setRequestsPage] = useState(1)
  const [reviewedRequestsPage, setReviewedRequestsPage] = useState(1)

  const pageSize = 5

  const { data: paginatedRequests, isLoading: isLoadingRequests } =
    usePaginatedRequests(requestsPage, pageSize)

  const {
    data: paginatedReviewedRequests,
    isLoading: isLoadingReviewedRequests,
  } = usePaginatedReviewedRequests(reviewedRequestsPage, pageSize)
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

  if (isLoadingRequests || isLoadingReviewedRequests)
    return <LoadingComponent />
  return (
    <>
      {request?.id && (
        <RequestReviewModal
          isOpen={isOpenReviewModal}
          onOpenChange={onOpenChangeReviewModal}
          request={request}
          onCloseCb={() => setRequest({} as LeaveType)}
        />
      )}

      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <RequestIcon />
          <h1 className=" text-[32px]">Requests to review</h1>
        </div>
        <Divider />
        <Table
          aria-label="list"
          className="mt-8"
          bottomContent={
            paginatedRequests?.requests?.length ? (
              <div className="flex w-full justify-center">
                <Pagination
                  isCompact
                  showControls
                  showShadow
                  color="secondary"
                  page={requestsPage}
                  total={Math.ceil(
                    (paginatedRequests?.totalCount || 1) / pageSize
                  )}
                  onChange={(newPage) => setRequestsPage(newPage)}
                />
              </div>
            ) : undefined
          }
        >
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn>OWNER</TableColumn>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No pending requests to review.'}>
            {(paginatedRequests?.requests || []).map((request) => (
              <TableRow key={request.id}>
                <TableCell>
                  <div className="flex flex-wrap flex-row items-center gap-4">
                    {request.type === 'bankHoliday' ? (
                      <PartyIcon />
                    ) : (
                      <BussinessWatchIcon />
                    )}
                    <p className="hidden md:block">
                      {request.type === 'bankHoliday'
                        ? 'Public Holiday'
                        : 'Paid Time Off'}
                    </p>
                  </div>
                </TableCell>
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
      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <EyeIcon />
          <h1 className=" text-[32px]">Reviewed requests</h1>
        </div>
        <Divider />
        <Table
          aria-label="list"
          className="mt-8"
          bottomContent={
            paginatedReviewedRequests?.requests?.length ? (
              <div className="flex w-full justify-center">
                <Pagination
                  isCompact
                  showControls
                  showShadow
                  color="secondary"
                  page={reviewedRequestsPage}
                  total={Math.ceil(
                    (paginatedReviewedRequests?.totalCount || 1) / pageSize
                  )}
                  onChange={(newPage) => setReviewedRequestsPage(newPage)}
                />
              </div>
            ) : undefined
          }
        >
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn>OWNER</TableColumn>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>STATUS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No reviewed requests.'}>
            {(paginatedReviewedRequests?.requests || []).map((request) => (
              <TableRow key={request.id}>
                <TableCell>
                  <div className="flex flex-wrap flex-row items-center gap-4">
                    {request.type === 'bankHoliday' ? (
                      <PartyIcon />
                    ) : (
                      <BussinessWatchIcon />
                    )}
                    <p className="hidden md:block">
                      {request.type === 'bankHoliday'
                        ? 'Public Holiday'
                        : 'Paid Time Off'}
                    </p>
                  </div>
                </TableCell>
                <TableCell>{request.ownerName}</TableCell>
                <TableCell className="hidden sm:table-cell">
                  {new Date(request.dateStart).toDateString()} to{' '}
                  {new Date(request.dateEnd).toDateString()}
                </TableCell>
                <TableCell>{request.description}</TableCell>
                <TableCell>
                  {request.approvedBy ? 'Approved' : 'Rejected'}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </>
  )
}
