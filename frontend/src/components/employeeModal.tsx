import {
  Modal,
  ModalContent,
  ModalBody,
  ModalHeader,
  DateRangePicker,
  ModalFooter,
  Card,
  Button,
  Textarea,
  Skeleton,
  Input,
} from '@nextui-org/react'
import { ConflictType, LeaveType, useLeavesModel } from '../models/Leaves'
import { ChangeEvent, useCallback, useEffect, useRef, useState } from 'react'
import { CalendarDate, parseDate } from '@internationalized/date'
import { EmployeeType, useEmployeeModel } from '../models/Employee'
import { useCountriesModel } from '../models/Countries'
import { useOrganizationModel } from '../models/Organization'
import { LoadingComponent } from './loading'

export const EmployeeModal = ({
  isOpen,
  onOpenChange,
  employee,
  onCloseCb,
  managerEmail,
  label,
}: {
  isOpen: boolean
  onOpenChange: () => void
  employee?: EmployeeType
  onCloseCb: () => void
  managerEmail?: string
  label: string
}) => {
  const { countries, isLoading: isLoadingCountries } = useCountriesModel()
  const { currentOrganization, isLoading: isLoadingOrganization } =
    useOrganizationModel()
  const { createEmployee, updateEmployee } = useEmployeeModel()
  const [employeeEmail, setEmployeeEmail] = useState(employee?.email || '')
  const [employeeTitle, setEmployeeTitle] = useState(employee?.title || '')
  const [employeeName, setEmployeeName] = useState(employee?.name || '')
  const [selectedCountry, setSelectedCountry] = useState(
    employee?.country || ''
  )
  const [ptoDays, setPtoDays] = useState(employee?.paidTimeOff || 1)
  const [emailError, setEmailError] = useState<string | null>(null)
  const [nameError, setNameError] = useState<string | null>(null)
  const [ptoError, setPtoError] = useState<string | null>(null)
  const [countryError, setCountryError] = useState<string | null>(null)
  const [titleError, setTitleError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const emailRef = useRef<HTMLInputElement>(null)
  const ptoRef = useRef<HTMLInputElement>(null)
  const countryRef = useRef<HTMLInputElement>(null)
  const titleRef = useRef<HTMLInputElement>(null)
  const nameRef = useRef<HTMLInputElement>(null)

  if (isLoadingCountries || isLoadingOrganization) return <LoadingComponent />
  const handleEmailChange = (event: ChangeEvent<HTMLInputElement>) => {
    setEmployeeEmail(event.target.value)
    setEmailError(null)
  }
  const handleNameChange = (event: ChangeEvent<HTMLInputElement>) => {
    setEmployeeName(event.target.value)
    setNameError(null)
  }
  const handleTitleChange = (event: ChangeEvent<HTMLInputElement>) => {
    setEmployeeTitle(event.target.value)
    setTitleError(null)
  }
  const handlePtoChange = (event: ChangeEvent<HTMLInputElement>) => {
    setPtoDays(parseInt(event.target.value))
    setPtoError(null)
  }
  const handleEmailBlur = () => {
    if (emailRef.current && !emailRef.current.checkValidity()) {
      setEmailError('Please enter a valid email address.')
    } else {
      setEmailError(null)
    }
  }
  const handleTitleBlur = () => {
    if (titleRef.current && !titleRef.current.value) {
      setTitleError('Please enter a job title.')
    } else {
      setTitleError(null)
    }
  }
  const handleNameBlur = () => {
    if (nameRef.current && !nameRef.current.value) {
      setNameError('Please enter an employee name.')
    } else {
      setNameError(null)
    }
  }
  const handlePtoBlur = () => {
    console.log(ptoRef.current?.value)
    if (ptoRef.current && !ptoRef.current.checkValidity()) {
      setPtoError('Please enter number of days of paid time off')
    } else {
      setPtoError(null)
    }
  }
  const handleCountryBlur = () => {
    const countryNames = countries.map((country) => country.name)
    if (
      countryRef.current &&
      (!countryRef.current.value ||
        !countryNames.includes(countryRef.current.value))
    ) {
      setCountryError('Please enter a country from the list.')
    } else {
      setCountryError(null)
    }
  }
  const handleCountryChange = (event: ChangeEvent<HTMLInputElement>) => {
    setSelectedCountry(event.target.value)
    setCountryError(null)
  }

  const employeeHandler = async (onClose: () => void) => {
    setIsLoading(true)
    if (employee) {
      // we are updating an employee
      await updateEmployee({
        name: employeeName,
        email: employeeEmail,
        country: selectedCountry,
        paidTimeOff: ptoDays,
        title: employeeTitle,
      })
    } else {
      if (managerEmail) {
        // we are creating an employee
        await createEmployee({
          name: employeeName,
          email: employeeEmail,
          country: selectedCountry,
          paidTimeOff: ptoDays,
          managedBy: managerEmail,
          organization: currentOrganization.id,
          title: employeeTitle,
        })
      } else {
        // we are creating the head of the organization
        await createEmployee({
          name: employeeName,
          email: employeeEmail,
          country: selectedCountry,
          paidTimeOff: ptoDays,
          managedBy: null,
          organization: currentOrganization.id,
          title: employeeTitle,
        })
      }
    }
    setIsLoading(false)
    onCloseCb()
    onClose()
  }
  return (
    <Modal
      isOpen={isOpen}
      onClose={onCloseCb}
      size="lg"
      onOpenChange={onOpenChange}
    >
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">{label}</ModalHeader>
            <ModalBody>
              {/* if we are editing we can't change the email */}
              {!employee && (
                <Input
                  type="email"
                  name="email"
                  id="email"
                  value={employeeEmail}
                  onChange={handleEmailChange}
                  onBlur={handleEmailBlur}
                  placeholder="name@yourorganization.com"
                  isRequired
                  ref={emailRef}
                  errorMessage={emailError}
                  label="Email"
                  isInvalid={!!emailError}
                />
              )}

              <Input
                type="text"
                name="title"
                id="title"
                value={employeeTitle}
                onChange={handleTitleChange}
                onBlur={handleTitleBlur}
                placeholder="developer"
                isRequired
                errorMessage={titleError}
                isInvalid={!!titleError}
                ref={titleRef}
                label="Job Title"
              />

              <Input
                type="text"
                name="name"
                id="name"
                label="Name"
                value={employeeName}
                onChange={handleNameChange}
                onBlur={handleNameBlur}
                placeholder="Enter the employee name"
                isRequired
                errorMessage={nameError}
                ref={nameRef}
                isInvalid={!!nameError}
              />

              <Input
                list="countries"
                id="countryInput"
                name="country"
                label="Country of residence"
                value={selectedCountry}
                onBlur={handleCountryBlur}
                onChange={handleCountryChange}
                placeholder="Type to search..."
                isRequired
                errorMessage={countryError}
                isInvalid={!!countryError}
                ref={countryRef}
              />
              <datalist id="countries">
                {countries.map((country) => (
                  <option key={country.code} value={country.name}>
                    {country.name}
                  </option>
                ))}
              </datalist>

              <Input
                type="number"
                name="ptoDays"
                id="ptoDaysInput"
                label="days of paid time off per year"
                value={(ptoDays as unknown as string) || '0'}
                onChange={handlePtoChange}
                onBlur={handlePtoBlur}
                ref={ptoRef}
                placeholder="Enter a number"
                min="1"
                max="365"
                isInvalid={!!ptoError}
                errorMessage={ptoError}
                isRequired
              />
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                color="primary"
                isDisabled={
                  !!emailError ||
                  !!ptoError ||
                  !!countryError ||
                  !!titleError ||
                  !!nameError ||
                  !employeeEmail ||
                  !employeeName ||
                  !employeeTitle ||
                  !selectedCountry
                }
                onPress={() => employeeHandler(onClose)}
                isLoading={isLoading}
              >
                {employee ? 'Update' : 'Create'}
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
