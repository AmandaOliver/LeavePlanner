import { useState } from 'react'
import { EmployeeType } from '../../../models/Employee'
import { SetupEmployee } from './setupEmployee'

export const Employee = ({ employee }: { employee: EmployeeType }) => {
  const [isAddSubordinateOpen, setIsAddSubordinateOpen] = useState(false)
  const [isEditEmployeeOpen, setIsEditEmployeeOpen] = useState(false)

  return (
    <>
      <summary>{employee.email}</summary>
      <p>{employee.paidTimeOff} days</p>
      <p>{employee.country}</p>

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
