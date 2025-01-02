import { useEmployeeModel } from '../../models/Employees'
import { LoadingComponent } from '../../components/loading'
import { CalendarContextProvider } from './Calendar/CalendarContext'

export const HomePage = () => {
  const { isLoading } = useEmployeeModel()
  if (isLoading) {
    return <LoadingComponent />
  }
  return (
    <>
      <CalendarContextProvider />
    </>
  )
}
