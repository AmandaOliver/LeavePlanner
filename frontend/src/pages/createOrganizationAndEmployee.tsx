import { useNavigate } from 'react-router-dom'
import { useOrganizationModel } from '../models/Organization'
import { useState } from 'react'

export const CreateOrganizationAndEmployee = () => {
  const { createOrganizationAndEmployee } = useOrganizationModel()
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()
  const [orgName, setOrgName] = useState('')

  if (isLoading) return <div>Loading</div>
  return (
    <>
      <h1>You are not registered on any organization.</h1>
      <h2>But you can create your own and get started with LeavePlanner</h2>
      <label>Your Organization Name*</label>
      <input
        type="text"
        name="orgName"
        id="orgName"
        value={orgName}
        onChange={(e) => setOrgName(e.target.value)}
        placeholder="Enter your Organization name"
        required
      />
      <button
        onClick={async () => {
          setIsLoading(true)
          const response = await createOrganizationAndEmployee(orgName)
          setIsLoading(false)
          if (response) {
            navigate(`/setup-organization/${response.organizationId}`)
          }
        }}
      >
        Next step
      </button>
    </>
  )
}
