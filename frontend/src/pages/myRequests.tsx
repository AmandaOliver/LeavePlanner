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
import { LeaveType, useLeavesModel } from '../models/Leaves'
import { RequestIcon } from '../icons/request'
import { EyeIcon } from '../icons/eye'
import { TrashIcon } from '../icons/trash'
import { LeaveDeleteModal } from '../components/leaveDeleteModal'
import { LeaveInfoModal } from '../components/leaveInfoModal'
import { useState } from 'react'
import { LoadingComponent } from '../components/loading'
import { LeaveModal } from '../components/leaveModal'
import { PencilIcon } from '../icons/pencil'
import { BanIcon } from '../icons/ban'
export const MyRequests = () => {
  const { leaves, leavesAwaitingApproval, leavesRejected, isLoading } =
    useLeavesModel()
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
  if (isLoading) return <LoadingComponent />
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
            (leave) => new Date(leave.dateStart) >= new Date()
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
          <RequestIcon />
          <h1 className=" text-[32px]">Pending requests</h1>
        </div>
        <Divider />
        <Table aria-label="list" className="mt-8">
          <TableHeader>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No pending requests.'}>
            {leavesAwaitingApproval.map((leave) => (
              <TableRow key={leave.id}>
                <TableCell className="hidden sm:table-cell">
                  {new Date(leave.dateStart).toDateString()} to{' '}
                  {new Date(leave.dateEnd).toDateString()}
                </TableCell>
                <TableCell>{leave.description}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap flex-row">
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
        <Table aria-label="list" className="mt-8">
          <TableHeader>
            <TableColumn className="hidden sm:table-cell">DATES</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody emptyContent={'No rejected requests.'}>
            {leavesRejected.map((leave) => (
              <TableRow key={leave.id}>
                <TableCell className="hidden sm:table-cell">
                  {new Date(leave.dateStart).toDateString()} to{' '}
                  {new Date(leave.dateEnd).toDateString()}
                </TableCell>
                <TableCell>{leave.description}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap flex-row">
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
