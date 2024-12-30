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
  Tooltip,
  Pagination,
} from '@nextui-org/react'
import { useState } from 'react'
import { LeaveDeleteModal } from '../../components/leaveDeleteModal'
import { LeaveInfoModal } from '../../components/leaveInfoModal'
import { LeaveModal } from '../../components/leaveModal'
import { LoadingComponent } from '../../components/loading'
import { BanIcon } from '../../icons/ban'
import { BussinessWatchIcon } from '../../icons/bussinesswatch'
import { EyeIcon } from '../../icons/eye'
import { PartyIcon } from '../../icons/party'
import { PencilIcon } from '../../icons/pencil'
import { RequestIcon } from '../../icons/request'
import { TrashIcon } from '../../icons/trash'
import { useLeavesModel, LeaveType } from '../../models/Leaves'

export const MyRequests = () => {
  const { usePaginatedLeavesAwaitingApproval, usePaginatedLeavesRejected } =
    useLeavesModel()
  const [pagePending, setPagePending] = useState(1)
  const [pageRejected, setPageRejected] = useState(1)
  const pageSize = 5
  const { data: pendingLeavesData, isLoading: isLoadingPending } =
    usePaginatedLeavesAwaitingApproval(pagePending, pageSize)

  const { data: rejectedLeavesData, isLoading: isLoadingRejected } =
    usePaginatedLeavesRejected(pageRejected, pageSize)
  const {
    isOpen: isOpenInfoModal,
    onOpen: onOpenInfoModal,
    onOpenChange: onOpenChangeInfoModal,
  } = useDisclosure()

  const {
    isOpen: isOpenDeleteModal,
    onOpen: onOpenDeleteModal,
    onOpenChange: onOpenChangeDeleteModal,
  } = useDisclosure()
  const {
    isOpen: isOpenUpdateModal,
    onOpen: onOpenUpdateModal,
    onOpenChange: onOpenChangeUpdateModal,
  } = useDisclosure()

  const [updateLeave, setUpdateLeave] = useState<LeaveType>()

  const [infoLeave, setInfoLeave] = useState<LeaveType>()
  const [deleteLeave, setDeleteLeave] = useState<LeaveType>()

  const handleDeleteModalOpen = (leave: LeaveType) => {
    setDeleteLeave(leave)
    onOpenDeleteModal()
  }
  const handleInfoModalOpen = (leave: LeaveType) => {
    setInfoLeave(leave)
    onOpenInfoModal()
  }
  const handleUpdateModalOpen = (leave: LeaveType) => {
    setUpdateLeave(leave)
    onOpenUpdateModal()
  }
  if (isLoadingPending || isLoadingRejected) return <LoadingComponent />
  return (
    <>
      {infoLeave?.id && (
        <LeaveInfoModal
          isOpen={isOpenInfoModal}
          onOpenChange={onOpenChangeInfoModal}
          leave={infoLeave}
        />
      )}
      {updateLeave?.id && (
        <LeaveModal
          isOpen={isOpenUpdateModal}
          onOpenChange={onOpenChangeUpdateModal}
          leave={updateLeave}
          onCloseCb={() => setUpdateLeave({} as LeaveType)}
        />
      )}
      {deleteLeave?.id && (
        <LeaveDeleteModal
          isOpen={isOpenDeleteModal}
          leave={deleteLeave}
          onOpenChange={onOpenChangeDeleteModal}
        />
      )}
      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <RequestIcon />
          <h1 className=" text-[32px]">Pending requests</h1>
        </div>
        <Divider />
        <Table
          aria-label="list"
          className="mt-8"
          bottomContent={
            pendingLeavesData?.leaves?.length ? (
              <div className="flex w-full justify-center">
                <Pagination
                  total={Math.ceil(pendingLeavesData.totalCount / pageSize)}
                  page={pagePending}
                  onChange={(page) => setPagePending(page)}
                  isCompact
                  showControls
                  showShadow
                  color="secondary"
                />
              </div>
            ) : undefined
          }
        >
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No pending requests.'}>
            {(pendingLeavesData?.leaves || []).map((leave) => (
              <TableRow key={leave.id}>
                <TableCell>
                  <div className="flex flex-wrap flex-row items-center gap-4">
                    {leave.type === 'bankHoliday' ? (
                      <PartyIcon />
                    ) : (
                      <BussinessWatchIcon />
                    )}
                    <p className="hidden md:block">
                      {leave.type === 'bankHoliday'
                        ? 'Bank Holiday'
                        : 'Paid Time Off'}
                    </p>
                  </div>
                </TableCell>
                <TableCell className="hidden sm:table-cell">
                  {new Date(leave.dateStart).toDateString()} to{' '}
                  {new Date(leave.dateEnd).toDateString()}
                </TableCell>
                <TableCell>{leave.description}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap flex-row">
                    <Tooltip content="See request info">
                      <Button
                        isIconOnly
                        color="default"
                        variant="light"
                        aria-label="edit"
                        size="sm"
                        onPress={() => handleInfoModalOpen(leave)}
                      >
                        <EyeIcon />
                      </Button>
                    </Tooltip>
                    <Tooltip content="Edit request">
                      <Button
                        isIconOnly
                        color="default"
                        variant="light"
                        aria-label="edit"
                        size="sm"
                        onPress={() => handleUpdateModalOpen(leave)}
                      >
                        <PencilIcon />
                      </Button>
                    </Tooltip>
                    <Tooltip content="Delete request">
                      <Button
                        isIconOnly
                        color="default"
                        variant="light"
                        aria-label="edit"
                        size="sm"
                        onPress={() => handleDeleteModalOpen(leave)}
                      >
                        <TrashIcon />
                      </Button>
                    </Tooltip>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <BanIcon />
          <h1 className=" text-[32px]">Rejected requests</h1>
        </div>
        <Divider />
        <Table
          aria-label="list"
          className="mt-8"
          bottomContent={
            rejectedLeavesData?.leaves?.length ? (
              <div className="flex w-full justify-center">
                <Pagination
                  total={Math.ceil(rejectedLeavesData.totalCount / pageSize)}
                  page={pageRejected}
                  onChange={(page) => setPageRejected(page)}
                  isCompact
                  showControls
                  showShadow
                  color="secondary"
                />
              </div>
            ) : undefined
          }
        >
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No rejected requests.'}>
            {(rejectedLeavesData?.leaves || []).map((leave) => (
              <TableRow key={leave.id}>
                <TableCell>
                  <div className="flex flex-wrap flex-row items-center gap-4">
                    {leave.type === 'bankHoliday' ? (
                      <PartyIcon />
                    ) : (
                      <BussinessWatchIcon />
                    )}
                    <p className="hidden md:block">
                      {leave.type === 'bankHoliday'
                        ? 'Bank Holiday'
                        : 'Paid Time Off'}
                    </p>
                  </div>
                </TableCell>
                <TableCell className="hidden sm:table-cell">
                  {new Date(leave.dateStart).toDateString()} to{' '}
                  {new Date(leave.dateEnd).toDateString()}
                </TableCell>
                <TableCell>{leave.description}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap flex-row">
                    <Tooltip content="See request info">
                      <Button
                        isIconOnly
                        color="default"
                        variant="light"
                        aria-label="edit"
                        size="sm"
                        onPress={() => handleInfoModalOpen(leave)}
                      >
                        <EyeIcon />
                      </Button>
                    </Tooltip>
                    <Tooltip content="Edit request">
                      <Button
                        isIconOnly
                        color="default"
                        variant="light"
                        aria-label="edit"
                        size="sm"
                        onPress={() => handleUpdateModalOpen(leave)}
                      >
                        <PencilIcon />
                      </Button>
                    </Tooltip>
                    <Tooltip content="Delete request">
                      <Button
                        isIconOnly
                        color="default"
                        variant="light"
                        aria-label="edit"
                        size="sm"
                        onPress={() => handleDeleteModalOpen(leave)}
                      >
                        <TrashIcon />
                      </Button>
                    </Tooltip>
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
