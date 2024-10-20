import { ChangeEvent, FormEvent, useRef, useState } from 'react'
import { useCountriesModel } from '../../../models/Countries'
import { EmployeeType, useEmployeeModel } from '../../../models/Employee'
import { useOrganizationModel } from '../../../models/Organization'
import LoadingPage from '../../loading'

export const SetupEmployee = ({
  managerEmail,
  employee,
  isHead,
}: {
  managerEmail?: string | null
  employee?: EmployeeType
  isHead?: boolean
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
  const [ptoDays, setPtoDays] = useState(employee?.paidTimeOff || 0)
  const [emailError, setEmailError] = useState<string | null>(null)
  const [nameError, setNameError] = useState<string | null>(null)
  const [ptoError, setPtoError] = useState<string | null>(null)
  const [countryError, setCountryError] = useState<string | null>(null)
  const [titleError, setTitleError] = useState<string | null>(null)

  const emailRef = useRef<HTMLInputElement>(null)
  const ptoRef = useRef<HTMLInputElement>(null)
  const countryRef = useRef<HTMLInputElement>(null)
  const titleRef = useRef<HTMLInputElement>(null)
  const nameRef = useRef<HTMLInputElement>(null)

  if (isLoadingCountries || isLoadingOrganization) return <LoadingPage />
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
      setTitleError('Please enter an employee name.')
    } else {
      setTitleError(null)
    }
  }
  const handlePtoBlur = () => {
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
  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
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
    }
    if (employee) {
      // we are updating an employee
      await updateEmployee({
        name: employeeName,
        email: employeeEmail,
        country: selectedCountry,
        paidTimeOff: ptoDays,
        title: employeeTitle,
      })
    }
    if (isHead) {
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
  return (
    <form onSubmit={handleSubmit}>
      {/* if we are editing we can't change the email */}
      {!employee && (
        <div>
          <label>Enter the email *</label>
          <input
            type="email"
            name="email"
            id="email"
            value={employeeEmail}
            onChange={handleEmailChange}
            onBlur={handleEmailBlur}
            placeholder="name@yourorganization.com"
            required
            ref={emailRef}
          />
          {emailError && <p style={{ color: 'red' }}>{emailError}</p>}
        </div>
      )}
      <div>
        <label>Enter the job title *</label>
        <input
          type="text"
          name="title"
          id="title"
          value={employeeTitle}
          onChange={handleTitleChange}
          onBlur={handleTitleBlur}
          placeholder="developer"
          required
          ref={titleRef}
        />
        {titleError && <p style={{ color: 'red' }}>{titleError}</p>}
      </div>
      <div>
        <label>Enter the name *</label>
        <input
          type="text"
          name="name"
          id="name"
          value={employeeName}
          onChange={handleNameChange}
          onBlur={handleNameBlur}
          placeholder="Enter the employee name"
          required
          ref={nameRef}
        />
        {nameError && <p style={{ color: 'red' }}>{nameError}</p>}
      </div>
      <div>
        <label>Enter the country *</label>
        <input
          list="countries"
          id="countryInput"
          name="country"
          value={selectedCountry}
          onBlur={handleCountryBlur}
          onChange={handleCountryChange}
          placeholder="Type to search..."
          required
          ref={countryRef}
        />
        <datalist id="countries">
          {countries.map((country) => (
            <option key={country.code} value={country.name}>
              {country.name}
            </option>
          ))}
        </datalist>
        {countryError && <p style={{ color: 'red' }}>{countryError}</p>}
      </div>
      <div>
        <label>Enter the amount of paid time off in days *</label>
        <input
          type="number"
          name="ptoDays"
          id="ptoDaysInput"
          value={ptoDays}
          onChange={handlePtoChange}
          onBlur={handlePtoBlur}
          ref={ptoRef}
          placeholder="Enter a number"
          required
          min="0"
        />
        {ptoError && <p style={{ color: 'red' }}>{ptoError}</p>}
      </div>

      <button
        type="submit"
        disabled={
          !!emailError ||
          !!ptoError ||
          !!countryError ||
          !!titleError ||
          !!nameError
        }
      >
        {employee ? 'Update' : 'Create'} Employee
      </button>
    </form>
  )
}
