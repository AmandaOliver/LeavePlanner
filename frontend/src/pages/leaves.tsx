import { useState } from 'react'
import { useLeavesModel } from '../models/Leaves'
import { SetupLeave } from '../components/setupLeave'
import { Leave } from '../components/leave'
import LoadingPage from '../components/loading'
import { useEmployeeModel } from '../models/Employee'
import { Card, CardBody, CardHeader } from '@nextui-org/react'
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
