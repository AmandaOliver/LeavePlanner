import { useNavigate } from 'react-router-dom'
import { useOrganizationModel } from '../models/Organization'
import { useState } from 'react'

export const CreateOrganizationAndEmployee = () => {
  const { createOrganizationAndEmployee } = useOrganizationModel()
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()
  return (
    <>
      <h1>You are not registered on any organization.</h1>
      <h2>But you can create your own and get started with LeavePlanner</h2>
      {isLoading && <div>Loading</div>}
      {!isLoading && (
        <button
          onClick={async () => {
            setIsLoading(true)
            const response = await createOrganizationAndEmployee('orgname')
            setIsLoading(false)
            if (response) {
              navigate(`/setup-organization/${response.organizationId}`)
            }
          }}
        >
          Create org
        </button>
      )}
    </>
  )
}
