import { useEmployeeModel } from '../../models/Employees'
import { LoadingComponent } from '../../components/loading'
import { CalendarContextProvider } from './Calendar/CalendarContext'
import { useNavigate } from 'react-router-dom'
import { useEffect } from 'react'

export const HomePage = () => {
  const { currentEmployee, isLoading } = useEmployeeModel()
  const navigate = useNavigate()

  useEffect(() => {
    if (currentEmployee && !currentEmployee.country) {
      navigate('/setup-organization')
    }
  }, [currentEmployee, navigate])
  if (isLoading) {
    return <LoadingComponent />
  }
  return (
    <>
      <CalendarContextProvider />
    </>
  )
}
