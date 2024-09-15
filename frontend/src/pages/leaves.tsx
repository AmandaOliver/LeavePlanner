import { useState } from 'react'
import { useLeavesModel } from '../models/Leaves'
import { SetupLeave } from '../components/setupLeave'
import { Leave } from '../components/leave'
import { EmployeeType } from '../models/Employee'
type TabsType = 'leaves' | 'leavesAwaitingApproval'
export const Leaves = ({ employee }: { employee: EmployeeType }) => {
  const { leaves, leavesAwaitingApproval } = useLeavesModel(employee.email)
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
          <SetupLeave employee={employee} />
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
          <Leave key={leave.id} leave={leave} employee={employee} />
        ))}
      </ul>
    </>
  )
}
