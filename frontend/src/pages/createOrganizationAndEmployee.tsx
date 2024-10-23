import { useNavigate } from 'react-router-dom'
import { useOrganizationModel } from '../models/Organization'
import { useRef, useState } from 'react'
import { Button, Card, CardBody, CardHeader, Input } from '@nextui-org/react'
import { BanIcon } from '../icons/ban'

export const CreateOrganizationAndEmployee = () => {
  const { createOrganizationAndEmployee } = useOrganizationModel()
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()
  const [orgName, setOrgName] = useState('')
  const nameRef = useRef<HTMLInputElement>(null)
  const [nameError, setNameError] = useState<string | null>(null)
  return (
    <Card className="bg-default-200 w-[80vw] m-auto mt-2">
      <CardHeader>
        <h1 className="text-[26px] text-bold text-danger-500 m-auto">
          You are not part of any organization.
        </h1>
      </CardHeader>
      <CardBody>
        {' '}
        <h2 className="text-[24px] m-auto">
          But you can create your own and get started with LeavePlanner
        </h2>
        <div className="p-8 flex flex-wrap flex-col gap-4">
          <Input
            type="text"
            name="orgName"
            label="Your organization name"
            id="orgName"
            value={orgName}
            onChange={(e) => {
              setOrgName(e.target.value)
              setNameError(null)
            }}
            onBlur={() => {
              if (nameRef.current && !nameRef.current.value) {
                setNameError('Enter a name for your organization')
              } else {
                setNameError(null)
              }
            }}
            placeholder="Enter your Organization name"
            isRequired
            isInvalid={!!nameError}
            errorMessage={nameError}
            ref={nameRef}
            className="flex w-full"
          />
          <Button
            onClick={async () => {
              setIsLoading(true)

              const response = await createOrganizationAndEmployee(orgName)
              if (response) {
                navigate(`/setup-organization/${response.organizationId}`)
              }

              setIsLoading(false)
            }}
            size="lg"
            color="primary"
            isLoading={isLoading}
            isDisabled={!!nameError}
          >
            Next step
          </Button>
        </div>
      </CardBody>
    </Card>
  )
}
