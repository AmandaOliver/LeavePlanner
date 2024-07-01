import { useState } from 'react'
import { useOrganizationModel } from '../models/Organization'
import { useCountriesModel } from '../models/Countries'
import { EmployeeType, useEmployeeModel } from '../models/Employee'
import { SetupHead } from '../components/setupHead'
import LoadingPage from './loading'

export const SetupOrganization = () => {
  const { currentOrganization, isLoadingOrganization } = useOrganizationModel()
  const { createEmployee } = useEmployeeModel()
  const { countries } = useCountriesModel()
  const [head, setHead] = useState({} as EmployeeType)
  if (isLoadingOrganization) return <LoadingPage />
  if (!currentOrganization)
    return <h1>ERROR Please create your organization first</h1>
  if (!head.email)
    return (
      <SetupHead
        countries={countries}
        createEmployee={createEmployee}
        currentOrganization={currentOrganization}
        setHead={setHead}
      ></SetupHead>
    )
  return <div>HEAD: {head.email}</div>
}
