import { EmployeeType } from '../models/Employee'
import { useOrganizationModel } from '../models/Organization'
import { SetupEmployee } from './setupEmployee'

export const OrganizationTree = () => {
  const { currentOrganization } = useOrganizationModel()
  const renderSubordinates = (subordinates: Array<EmployeeType>) =>
    subordinates.map((employee) => {
      if (employee.subordinates) renderSubordinates(employee.subordinates)
      return (
        <div>
          <p>{employee.email}</p>
          <SetupEmployee managerEmail={employee.email} />
        </div>
      )
    })
  return <p>{renderSubordinates(currentOrganization.tree)}</p>
}
