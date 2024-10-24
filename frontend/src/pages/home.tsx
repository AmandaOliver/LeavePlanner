import { useEmployeeModel } from '../models/Employee'
import { LoadingComponent } from '../components/loading'
import { CalendarContextProvider } from './Calendar/CalendarContext'

export const HomePage = () => {
  const { currentEmployee, isLoading } = useEmployeeModel()
  if (isLoading) {
    return <LoadingComponent />
  }
  return (
    <>
      <CalendarContextProvider />
    </>
  )
}
