import { useNavigate } from 'react-router-dom'
import { useOrganizationModel } from '../models/Organization'
import { useRef, useState } from 'react'

export const CreateOrganizationAndEmployee = () => {
  const { createOrganizationAndEmployee } = useOrganizationModel()
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()
  const [orgName, setOrgName] = useState('')
  const nameRef = useRef<HTMLInputElement>(null)
  const [nameError, setNameError] = useState<string | null>(null)
  return (
    <>
      <h1>You are not registered in any organization.</h1>
      <h2>But you can create your own and get started with LeavePlanner</h2>
      <label>Your Organization Name*</label>
      <input
        type="text"
        name="orgName"
        id="orgName"
        value={orgName}
        onChange={(e) => {
          setOrgName(e.target.value)
          setNameError(null)
        }}
        onBlur={() => {
          if (nameRef.current && !nameRef.current.value) {
            setNameError('Introduce a name for your organization')
          } else {
            setNameError(null)
          }
        }}
        placeholder="Enter your Organization name"
        required
        ref={nameRef}
      />
      {nameError && <p style={{ color: 'red' }}>{nameError}</p>}

      <button
        onClick={async () => {
          setIsLoading(true)
          try {
            const response = await createOrganizationAndEmployee(orgName)
            if (response) {
              navigate(`/setup-organization/${response.organizationId}`)
            }
          } catch (error) {
            console.error('Failed to create organization and employee:', error)
          } finally {
            setIsLoading(false)
          }
        }}
        disabled={isLoading || !!nameError}
      >
        {isLoading ? 'Creating...' : 'Next step'}
      </button>
    </>
  )
}
