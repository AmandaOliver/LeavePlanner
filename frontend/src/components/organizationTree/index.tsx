import { EmployeeType } from '../../models/Employee'
import { useOrganizationModel } from '../../models/Organization'
import { Employee } from './components/employee'

export const OrganizationTree = () => {
  const { currentOrganization } = useOrganizationModel()
  const renderSubordinates = (subordinates: Array<EmployeeType>) =>
    subordinates.map((employee) => {
      return (
        <details key={employee.email}>
          <Employee employee={employee} />
          {employee.subordinates && renderSubordinates(employee.subordinates)}
        </details>
      )
    })
  return <>{renderSubordinates(currentOrganization.tree)}</>
}
