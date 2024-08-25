import { useState } from 'react'
import { EmployeeType, useEmployeeModel } from '../../../models/Employee'
import { SetupEmployee } from './setupEmployee'

export const Employee = ({ employee }: { employee: EmployeeType }) => {
  const [isAddSubordinateOpen, setIsAddSubordinateOpen] = useState(false)
  const [isEditEmployeeOpen, setIsEditEmployeeOpen] = useState(false)
  const { deleteEmployee } = useEmployeeModel()
  const isHead = !employee.managedBy
  const isManager = employee.subordinates && employee.subordinates.length > 0
  console.log(employee.email, isHead, isManager)
  const canBeDeleted = !isHead || (isHead && !isManager)
  return (
    <>
      <summary>{employee.email}</summary>
      <p>{employee.paidTimeOff} days</p>
      <p>{employee.country}</p>
      {canBeDeleted && (
        <button
          onClick={async () =>
            await deleteEmployee({
              email: employee.email,
            })
          }
        >
          Delete employee
        </button>
      )}
      {isAddSubordinateOpen ? (
        <>
          <SetupEmployee managerEmail={employee.email} />
          <button onClick={() => setIsAddSubordinateOpen(false)}>
            Close add subordinate
          </button>
        </>
      ) : (
        <button onClick={() => setIsAddSubordinateOpen(true)}>
          Add subordinate
        </button>
      )}
      {isEditEmployeeOpen ? (
        <>
          <SetupEmployee employee={employee} />
          <button onClick={() => setIsEditEmployeeOpen(false)}>
            Close update employee
          </button>
        </>
      ) : (
        <button onClick={() => setIsEditEmployeeOpen(true)}>
          Update employee
        </button>
      )}
    </>
  )
}
