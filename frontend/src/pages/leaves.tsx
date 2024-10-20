import { useState } from 'react'
import { useLeavesModel } from '../models/Leaves'
import { SetupLeave } from '../components/setupLeave'
import { Leave } from '../components/leave'
import LoadingPage from '../components/loading'
import { useEmployeeModel } from '../models/Employee'
type TabsType = 'leaves' | 'leavesAwaitingApproval' | 'leavesRejected'

export const Leaves = () => {
  const { leaves, leavesAwaitingApproval, leavesRejected, isLoading } =
    useLeavesModel()
  const { currentEmployee } = useEmployeeModel()
  const [isRequestLeaveFormOpen, setIsRequestLeaveFormOpen] = useState(false)
  const [activeTab, setActiveTab] = useState<TabsType>('leaves')
  const leavesToDisplay =
    activeTab === 'leaves'
      ? leaves
      : activeTab === 'leavesAwaitingApproval'
        ? leavesAwaitingApproval
        : leavesRejected
  if (isLoading) return <LoadingPage />
  return (
    <>
      <h3>Paid Time off left this year: {currentEmployee?.paidTimeOffLeft}</h3>
      <h3>
        Paid Time off left next year: {currentEmployee?.paidTimeOffLeftNextYear}
      </h3>

      <button onClick={() => setIsRequestLeaveFormOpen(true)}>
        Request Leave
      </button>
      {isRequestLeaveFormOpen && (
        <>
          <SetupLeave />
          <button onClick={() => setIsRequestLeaveFormOpen(false)}>
            Close request form
          </button>
        </>
      )}
      <div id="leaves-tabs">
        <button
          onClick={() => setActiveTab('leaves')}
          disabled={activeTab === 'leaves'}
        >
          Leaves
        </button>
        <button
          disabled={activeTab === 'leavesAwaitingApproval'}
          onClick={() => setActiveTab('leavesAwaitingApproval')}
        >
          Leaves Awaiting Approval
        </button>
        <button
          disabled={activeTab === 'leavesRejected'}
          onClick={() => setActiveTab('leavesRejected')}
        >
          Leaves Rejected
        </button>
      </div>
      <ul>
        {leavesToDisplay?.map((leave) => (
          <Leave key={leave.id} leave={leave} />
        ))}
      </ul>
    </>
  )
}
