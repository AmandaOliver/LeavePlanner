import { Navigation } from '../components/navigation'
import { useEmployeeModel } from '../models/Employee'

export const HomePage = () => {
  const { currentEmployee } = useEmployeeModel()

  return (
    <>
      <Navigation></Navigation>
      {currentEmployee && (
        <div>Welcome to LeavePlanner {currentEmployee.name}</div>
      )}
    </>
  )
}
