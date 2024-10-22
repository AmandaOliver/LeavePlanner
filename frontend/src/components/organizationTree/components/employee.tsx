import { EmployeeType, useEmployeeModel } from '../../../models/Employee'
import { Button, CardFooter } from '@nextui-org/react'

export const Employee = ({ employee }: { employee: EmployeeType }) => {
  const { deleteEmployee } = useEmployeeModel()
  const isHead = !employee.managedBy
  const isManager = employee.subordinates && employee.subordinates.length > 0
  const canBeDeleted = !isHead || (isHead && !isManager)
  return (
    <div className="flex flex-wrap flex-row w-full justify-between">
      <div>
        <p>Name: {employee.name}</p>
        <p>Title: {employee.title}</p>
        <p>Email: {employee.email}</p>
        <p>Days of paid time off: {employee.paidTimeOff}</p>
        <p>Country: {employee.country}</p>
      </div>
      <div className="flex flex-wrap flex-col gap-4 ">
        <Button color="primary">Add subordinate</Button>

        <Button color="default">Update employee</Button>
        {canBeDeleted && (
          <Button
            onClick={async () =>
              await deleteEmployee({
                email: employee.email,
              })
            }
            color="danger"
          >
            Delete employee
          </Button>
        )}
      </div>
    </div>
  )
}
