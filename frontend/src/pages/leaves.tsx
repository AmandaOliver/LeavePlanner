import { useState } from 'react'
import { useLeavesModel } from '../models/Leaves'
import { RequestLeave } from '../components/requestLeave'
import { Leave } from '../components/leave'

export const Leaves = ({ employeeEmail }: { employeeEmail: string }) => {
  const { leaves } = useLeavesModel(employeeEmail)
  const [isRequestLeaveFormOpen, setIsRequestLeaveFormOpen] = useState(false)
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
      <ul>
        {leaves.map((leave) => (
          <Leave leave={leave} employeeEmail={employeeEmail} />
        ))}
      </ul>
    </>
  )
}
