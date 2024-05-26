import { useParams } from 'react-router-dom'

export const SetupOrganization = () => {
  const { id } = useParams()
  return <h1>Setup your new organization {id}</h1>
}
