import { useState } from 'react'

import {
  Card,
  CardBody,
  Button,
  Divider,
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
import { LeaveDeleteModal } from '../components/leaveDeleteModal'
import { LeaveInfoModal } from '../components/leaveInfoModal'
import { LeaveModal } from '../components/leaveModal'
import { LoadingComponent } from '../components/loading'
import { BussinessWatchIcon } from '../icons/bussinesswatch'
import { EyeIcon } from '../icons/eye'
import { HistoryIcon } from '../icons/history'
import { PartyIcon } from '../icons/party'
import { PencilIcon } from '../icons/pencil'
import { TrashIcon } from '../icons/trash'
import { WatchIcon } from '../icons/watch'
import { useEmployeeModel } from '../models/Employees'
import { useLeavesModel, LeaveType } from '../models/Leaves'

export const Leaves = () => {
  const { usePaginatedLeaves, usePaginatedPastLeaves } = useLeavesModel()
  const [page, setPage] = useState(1)
  const [pastPage, setPastPage] = useState(1)
  const pageSize = 5
  const { currentEmployee, isLoading: isLoadingEmployee } = useEmployeeModel()
  const { data: paginatedLeaves, isLoading } = usePaginatedLeaves(
    page,
    pageSize
  )
  const { data: paginatedPastLeaves, isLoading: isLoadingPast } =
    usePaginatedPastLeaves(pastPage, pageSize)
  const {
    isOpen: isOpenInfoModal,
    onOpen: onOpenInfoModal,
    onOpenChange: onOpenChangeInfoModal,
  } = useDisclosure()
  const {
    isOpen: isOpenUpdateModal,
    onOpen: onOpenUpdateModal,
    onOpenChange: onOpenChangeUpdateModal,
  } = useDisclosure()
  const {
    isOpen: isOpenDeleteModal,
    onOpen: onOpenDeleteModal,
    onOpenChange: onOpenChangeDeleteModal,
  } = useDisclosure()
  const [updateLeave, setUpdateLeave] = useState<LeaveType>()
  const [infoLeave, setInfoLeave] = useState<LeaveType>()
  const [deleteLeave, setDeleteLeave] = useState<LeaveType>()
  const handleUpdateModalOpen = (leave: LeaveType) => {
    setUpdateLeave(leave)
    onOpenUpdateModal()
  }
  const handleDeleteModalOpen = (leave: LeaveType) => {
    setDeleteLeave(leave)
    onOpenDeleteModal()
  }
  const handleInfoModalOpen = (leave: LeaveType) => {
    setInfoLeave(leave)
    onOpenInfoModal()
  }
  if (isLoading || isLoadingEmployee || isLoadingPast)
    return <LoadingComponent />
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
        <div className="hidden sm:flex flex-wrap flex-row items-center gap-4">
          <BussinessWatchIcon />
          <h1 className=" text-[32px]">Available leaves</h1>
        </div>
        <Divider />
        <div className="flex flex-wrap flex-row gap-12 justify-center mt-8">
          <Card shadow="lg" className="bg-default-200 w-[200px]">
            <CardBody>
              <h1 className="text-default-600 font-bold text-[24px] m-auto">
                This year
              </h1>
              <h1 className="text-primary font-bold text-[36px] m-auto">
                {currentEmployee?.paidTimeOffLeft}
              </h1>
              <h1 className="text-primary text-[16px] m-auto">available</h1>
            </CardBody>
          </Card>
          <Card shadow="lg" className="bg-default-200 w-[200px]">
            <CardBody>
              <h1 className="text-default-600 font-bold text-[24px] m-auto">
                Next year
              </h1>
              <h1 className="text-primary font-bold text-[36px] m-auto">
                {currentEmployee?.paidTimeOffLeftNextYear}
              </h1>
              <h1 className="text-primary text-[16px] m-auto">available</h1>
            </CardBody>
          </Card>
        </div>
      </div>
      <div className="m-8">
        <div className="hidden sm:flex flex-wrap flex-row items-center gap-4">
          <WatchIcon />
          <h1 className=" text-[32px]">Upcoming leaves</h1>
        </div>
        <Divider />
        <Table
          aria-label="list"
          className="mt-8"
          bottomContent={
            paginatedLeaves?.leaves?.length ? (
              <div className="flex w-full justify-center">
                <Pagination
                  total={Math.ceil(paginatedLeaves.totalCount / pageSize)}
                  page={page}
                  onChange={(page) => setPage(page)}
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
          <TableBody emptyContent={'No upcoming leaves'}>
            {(paginatedLeaves?.leaves || []).map((leave) => (
              <TableRow key={leave.id}>
                <TableCell>
                  <div className="flex flex-wrap flex-row items-center gap-4">
                    {leave.type === 'bankHoliday' ? (
                      <PartyIcon />
                    ) : (
                      <BussinessWatchIcon />
                    )}
                    <p className="hidden lg:block">
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
                  <div className="flex flex-wrap flex-row ">
                    <Tooltip content="See leave info">
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

                    <>
                      <Tooltip content="Edit leave">
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
                      <Tooltip content="Delete leave">
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
                    </>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
      <div className="m-8">
        <div className="hidden sm:flex flex-wrap flex-row items-center gap-4">
          <HistoryIcon />
          <h1 className=" text-[32px]">History</h1>
        </div>
        <Divider />
        <Table
          aria-label="list"
          className="mt-8"
          bottomContent={
            paginatedPastLeaves?.leaves?.length ? (
              <div className="flex w-full justify-center">
                <Pagination
                  total={Math.ceil(paginatedPastLeaves.totalCount / pageSize)}
                  page={pastPage}
                  onChange={(page) => setPastPage(page)}
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
          </TableHeader>
          <TableBody emptyContent={'No past leaves'}>
            {(paginatedPastLeaves?.leaves || []).map((leave) => (
              <TableRow key={leave.id}>
                <TableCell>
                  <div className="flex flex-wrap flex-row items-center gap-4">
                    {leave.type === 'bankHoliday' ? (
                      <PartyIcon />
                    ) : (
                      <BussinessWatchIcon />
                    )}
                    <p className="hidden lg:block">
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
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </>
  )
}
