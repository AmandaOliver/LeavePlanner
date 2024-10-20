import { useEmployeeModel } from '../models/Employee'
import LoadingComponent from '../components/loading'

export const HomePage = () => {
  const { currentEmployee, isLoading } = useEmployeeModel()
  if (isLoading) {
    return <LoadingComponent />
  }
  return (
    <>
      <div>Welcome to LeavePlanner {currentEmployee?.name}</div>
    </>
  )
}
