import { useEmployeeModel } from '../models/Employee'

export const HomePage = () => {
  const { currentEmployee } = useEmployeeModel()

  return (
    <>
      {currentEmployee && (
        <div>Welcome to LeavePlanner {currentEmployee.name}</div>
      )}
    </>
  )
}
