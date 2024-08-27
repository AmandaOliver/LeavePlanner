import { useState } from 'react'
import { EmployeeType, useEmployeeModel } from '../../../models/Employee'
import { SetupEmployee } from './setupEmployee'

export const Employee = ({ employee }: { employee: EmployeeType }) => {
  const [isAddSubordinateOpen, setIsAddSubordinateOpen] = useState(false)
  const [isEditEmployeeOpen, setIsEditEmployeeOpen] = useState(false)
  const { deleteEmployee } = useEmployeeModel()
  const isHead = !employee.managedBy
  const isManager = employee.subordinates && employee.subordinates.length > 0
  const canBeDeleted = !isHead || (isHead && !isManager)
  return (
    <>
      <summary>{employee.email}</summary>
      <p>Name: {employee.name}</p>
      <p>Days of paid time off: {employee.paidTimeOff}</p>
      <p>Country: {employee.country}</p>
      <p>Title: {employee.title}</p>
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
      <hr />
    </>
  )
}
