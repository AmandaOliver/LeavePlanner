import { useState } from 'react'
import { useLeavesModel } from '../models/Leaves'
import { SetupLeave } from '../components/setupLeave'
import { Leave } from '../components/leave'
import LoadingPage from '../components/loading'
import { useEmployeeModel } from '../models/Employee'
import {
  Card,
  CardBody,
  CardHeader,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableColumn,
  TableHeader,
  TableRow,
} from '@nextui-org/react'
import { WatchIcon } from '../icons/watch'
import { BussinessWatchIcon } from '../icons/bussinesswatch'
import { PartyIcon } from '../icons/party'
import { HistoryIcon } from '../icons/history'
type TabsType = 'leaves' | 'leavesAwaitingApproval' | 'leavesRejected'

export const Leaves = () => {
  const { leaves, leavesAwaitingApproval, leavesRejected, isLoading } =
    useLeavesModel()
  const { currentEmployee, isLoading: isLoadingEmployee } = useEmployeeModel()
  const [isRequestLeaveFormOpen, setIsRequestLeaveFormOpen] = useState(false)
  const [activeTab, setActiveTab] = useState<TabsType>('leaves')
  const leavesToDisplay =
    activeTab === 'leaves'
      ? leaves
      : activeTab === 'leavesAwaitingApproval'
        ? leavesAwaitingApproval
        : leavesRejected
  if (isLoading || isLoadingEmployee) return <LoadingPage />
  return (
    <>
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
        <Table aria-label="Example static collection table" className="mt-8">
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn>START DATE</TableColumn>
            <TableColumn>END DATE</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
            <TableColumn>ACTIONS</TableColumn>
          </TableHeader>
          <TableBody>
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
                      <p className="hidden sm:block">
                        {leave.type === 'bankHoliday'
                          ? 'Bank Holiday'
                          : 'Paid Time Off'}
                      </p>
                    </div>
                  </TableCell>
                  <TableCell>{leave.dateStart}</TableCell>
                  <TableCell>{leave.dateEnd}</TableCell>
                  <TableCell>{leave.description}</TableCell>
                  <TableCell>N/A</TableCell>
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
        <Table aria-label="Example static collection table" className="mt-8">
          <TableHeader>
            <TableColumn>TYPE</TableColumn>
            <TableColumn>START DATE</TableColumn>
            <TableColumn>END DATE</TableColumn>
            <TableColumn>DESCRIPTION</TableColumn>
          </TableHeader>

          <TableBody>
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
                      <p className="hidden sm:block">
                        {leave.type === 'bankHoliday'
                          ? 'Bank Holiday'
                          : 'Paid Time Off'}
                      </p>
                    </div>
                  </TableCell>
                  <TableCell>{leave.dateStart}</TableCell>
                  <TableCell>{leave.dateEnd}</TableCell>
                  <TableCell>{leave.description}</TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </div>
    </>
  )
  // return (
  //   <>
  //

  //     <button onClick={() => setIsRequestLeaveFormOpen(true)}>
  //       Request Leave
  //     </button>
  //     {isRequestLeaveFormOpen && (
  //       <>
  //         <SetupLeave />
  //         <button onClick={() => setIsRequestLeaveFormOpen(false)}>
  //           Close request form
  //         </button>
  //       </>
  //     )}
  //     <div id="leaves-tabs">
  //       <button
  //         onClick={() => setActiveTab('leaves')}
  //         disabled={activeTab === 'leaves'}
  //       >
  //         Leaves
  //       </button>
  //       <button
  //         disabled={activeTab === 'leavesAwaitingApproval'}
  //         onClick={() => setActiveTab('leavesAwaitingApproval')}
  //       >
  //         Leaves Awaiting Approval
  //       </button>
  //       <button
  //         disabled={activeTab === 'leavesRejected'}
  //         onClick={() => setActiveTab('leavesRejected')}
  //       >
  //         Leaves Rejected
  //       </button>
  //     </div>
  //     <ul>
  //       {leavesToDisplay?.map((leave) => (
  //         <Leave key={leave.id} leave={leave} />
  //       ))}
  //     </ul>
  //   </>
  // )
}
