import { useDisclosure, Button } from '@nextui-org/react'
import { useState } from 'react'
import { EmployeeType } from '../../../models/Employees'
import { DeleteEmployeeModal } from './deleteEmployeeModal'
import { EmployeeModal } from './employeeModal'

export const Employee = ({ employee }: { employee: EmployeeType }) => {
  const isHead = !employee.managedBy
  const isManager = employee.subordinates && employee.subordinates.length > 0
  const canBeDeleted = !isHead || (isHead && !isManager)
  const [updateEmployee, setUpdateEmployee] = useState<EmployeeType>(
    {} as EmployeeType
  )
  const [createEmployee, setCreateEmployee] = useState<EmployeeType>(
    {} as EmployeeType
  )
  const [deleteEmployee, setDeleteEmployee] = useState<EmployeeType>(
    {} as EmployeeType
  )

  const {
    isOpen: isOpenUpdateModal,
    onOpen: onOpenUpdateModal,
    onOpenChange: onOpenChangeUpdateModal,
  } = useDisclosure()
  const {
    isOpen: isOpenDeleteModal,
    onOpen: onOpenDeleteModal,
    onOpenChange: onOpenChangeDeleteModal,
  } = useDisclosure()
  const handleUpdateModalOpen = (employee: EmployeeType) => {
    setUpdateEmployee(employee)
    onOpenUpdateModal()
  }
  const handleCreateModalOpen = (employee: EmployeeType) => {
    setCreateEmployee(employee)
    onOpenUpdateModal()
  }
  const handleDeleteModalOpen = (employee: EmployeeType) => {
    setDeleteEmployee(employee)
    onOpenDeleteModal()
  }
  return (
    <>
      {updateEmployee?.id && (
        <EmployeeModal
          isOpen={isOpenUpdateModal}
          employee={updateEmployee}
          onOpenChange={onOpenChangeUpdateModal}
          onCloseCb={() => setUpdateEmployee({} as EmployeeType)}
          label={'Update Employee'}
        />
      )}
      {createEmployee?.id && (
        <EmployeeModal
          isOpen={isOpenUpdateModal}
          onOpenChange={onOpenChangeUpdateModal}
          onCloseCb={() => setCreateEmployee({} as EmployeeType)}
          managerId={createEmployee.id}
          label={'Create Employee'}
        />
      )}
      {deleteEmployee?.id && (
        <DeleteEmployeeModal
          isOpen={isOpenDeleteModal}
          onOpenChange={onOpenChangeDeleteModal}
          employee={deleteEmployee}
        />
      )}
      <div className="flex flex-wrap flex-row w-full justify-between">
        <div>
          {employee.isOrgOwner && <p className="font-bold">Is Admin</p>}
          <p>Full Name: {employee.name}</p>
          <p>Title: {employee.title}</p>
          <p>Email: {employee.email}</p>
          <p>Days of paid time off: {employee.paidTimeOff}</p>
          <p>Country: {employee.country}</p>
        </div>
        <div className="flex flex-wrap flex-col gap-4 ">
          <Button
            color="primary"
            onPress={() => handleCreateModalOpen(employee)}
          >
            Add subordinate
          </Button>

          <Button
            color="default"
            onPress={() => handleUpdateModalOpen(employee)}
          >
            Update employee
          </Button>
          {canBeDeleted && (
            <Button
              onPress={() => handleDeleteModalOpen(employee)}
              color="danger"
            >
              Delete employee
            </Button>
          )}
        </div>
      </div>
    </>
  )
}
