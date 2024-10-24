import { useState } from 'react'
import { LeaveType, useLeavesModel } from '../models/Leaves'
import { useEmployeeModel } from '../models/Employee'
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
} from '@nextui-org/react'
import { WatchIcon } from '../icons/watch'
import { BussinessWatchIcon } from '../icons/bussinesswatch'
import { PartyIcon } from '../icons/party'
import { HistoryIcon } from '../icons/history'
import { PencilIcon } from '../icons/pencil'
import { TrashIcon } from '../icons/trash'
import { EyeIcon } from '../icons/eye'
import { LeaveInfoModal } from '../components/leaveInfoModal'
import { LeaveModal } from '../components/leaveModal'
import { LeaveDeleteModal } from '../components/leaveDeleteModal'
import { LoadingComponent } from '../components/loading'

export const Leaves = () => {
  const { leaves, isLoading } = useLeavesModel()
  const { currentEmployee, isLoading: isLoadingEmployee } = useEmployeeModel()
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
  if (isLoading || isLoadingEmployee) return <LoadingComponent />
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
          leaves={leaves.filter(
            (leave) =>
              new Date(leave.dateStart) >= new Date() &&
              leave.id !== updateLeave.id
          )}
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
        <div className="flex flex-wrap flex-row items-center gap-4">
          <WatchIcon />
          <h1 className=" text-[32px]">Upcomming leaves</h1>
        </div>
        <Divider />
        <Table aria-label="list" className="mt-8">
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No leaves in the next 6 months.'}>
            {leaves
              .filter((leave) => new Date(leave.dateStart) >= new Date())
              .sort((a, b) => (a.dateStart < b.dateStart ? -1 : 1))
              .map((leave) => (
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

                      {leave.type !== 'bankHoliday' && (
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
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </div>
      <div className="m-8">
        <div className="flex flex-wrap flex-row items-center gap-4">
          <HistoryIcon />
          <h1 className=" text-[32px]">History</h1>
        </div>
        <Divider />
        <Table aria-label="list" className="mt-8">
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
          </TableHeader>

          <TableBody emptyContent="No history.">
            {leaves
              .filter((leave) => new Date(leave.dateStart) < new Date())
              .sort((a, b) => (a.dateStart < b.dateStart ? -1 : 1))
              .map((leave) => (
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
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </div>
    </>
  )
}
