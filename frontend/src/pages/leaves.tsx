import { useState } from 'react'
import { useLeavesModel } from '../models/Leaves'
import { RequestLeave } from '../components/requestLeave'
import { Leave } from '../components/leave'
type TabsType = 'leaves' | 'leavesAwaitingApproval'
export const Leaves = ({ employeeEmail }: { employeeEmail: string }) => {
  const { leaves, leavesAwaitingApproval } = useLeavesModel(employeeEmail)
  const [isRequestLeaveFormOpen, setIsRequestLeaveFormOpen] = useState(false)
  const [activeTab, setActiveTab] = useState<TabsType>('leaves')
  const leavesToDisplay =
    activeTab === 'leaves' ? leaves : leavesAwaitingApproval
  return (
    <>
      <button onClick={() => setIsRequestLeaveFormOpen(true)}>
        Request Leave
      </button>
      {isRequestLeaveFormOpen && (
        <>
          <RequestLeave employeeEmail={employeeEmail} />
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
      </div>
      <ul>
        {leavesToDisplay.map((leave) => (
          <Leave key={leave.id} leave={leave} employeeEmail={employeeEmail} />
        ))}
      </ul>
    </>
  )
}
