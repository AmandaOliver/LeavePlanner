import { Card, CardBody, CardHeader } from '@nextui-org/react'
import { EmployeeType } from '../../../models/Employee'
import { useOrganizationModel } from '../../../models/Organization'
import { Employee } from './components/employee'

export const OrganizationTree = () => {
  const { currentOrganization } = useOrganizationModel()
  if (!currentOrganization || !currentOrganization.tree.length) return null
  const renderSubordinates = (subordinates: Array<EmployeeType>) =>
    subordinates.map((employee) => {
      return (
        <Card className="bg-default-200 p-4 pr-0 m-4 mr-0" key={employee.id}>
          <CardHeader>
            <Employee employee={employee} />
          </CardHeader>
          <CardBody>
            {employee.subordinates && employee.subordinates.length > 0 && (
              <details key={employee.id}>
                <summary className="z-0 group cursor-pointer relative inline-flex items-center justify-center box-border appearance-none select-none whitespace-nowrap font-normal subpixel-antialiased overflow-hidden tap-highlight-transparent data-[pressed=true]:scale-[0.97] outline-none data-[focus-visible=true]:z-10 data-[focus-visible=true]:outline-2 data-[focus-visible=true]:outline-focus data-[focus-visible=true]:outline-offset-2 px-4 min-w-20 h-10 text-small gap-2 rounded-medium [&>svg]:max-w-[theme(spacing.8)] transition-transform-colors-opacity motion-reduce:transition-none bg-default text-default-foreground data-[hover=true]:opacity-hover">
                  See subordinates
                </summary>
                {renderSubordinates(employee.subordinates)}
              </details>
            )}
          </CardBody>
        </Card>
      )
    })
  return <>{renderSubordinates(currentOrganization?.tree)}</>
}
