import { ChangeEvent, FormEvent, useRef, useState } from 'react'
import { useCountriesModel } from '../../../models/Countries'
import { EmployeeType, useEmployeeModel } from '../../../models/Employee'
import { useOrganizationModel } from '../../../models/Organization'
import LoadingPage from '../../../pages/loading'

export const SetupEmployee = ({
  managerEmail,
  employee,
}: {
  managerEmail?: string | null
  employee?: EmployeeType
}) => {
  const { countries, isLoading: isLoadingCountries } = useCountriesModel()
  const { currentOrganization, isLoading: isLoadingOrganization } =
    useOrganizationModel()
  const { createEmployee, updateEmployee } = useEmployeeModel()
  const [employeeEmail, setEmployeeEmail] = useState(employee?.email || '')
  const [selectedCountry, setSelectedCountry] = useState(
    employee?.country || ''
  )
  const [ptoDays, setPtoDays] = useState(employee?.paidTimeOff || 0)
  const [emailError, setEmailError] = useState<string | null>(null)
  const [ptoError, setPtoError] = useState<string | null>(null)
  const [countryError, setCountryError] = useState<string | null>(null)
  const emailRef = useRef<HTMLInputElement>(null)
  const ptoRef = useRef<HTMLInputElement>(null)
  const countryRef = useRef<HTMLInputElement>(null)

  if (isLoadingCountries || isLoadingOrganization) return <LoadingPage />
  const handleEmailChange = (event: ChangeEvent<HTMLInputElement>) => {
    setEmployeeEmail(event.target.value)
    setEmailError(null)
  }

  const handlePtoChange = (event: ChangeEvent<HTMLInputElement>) => {
    setPtoDays(parseInt(event.target.value))
    setPtoError(null)
  }
  const handleEmailBlur = () => {
    // Check validity on blur
    if (emailRef.current && !emailRef.current.checkValidity()) {
      setEmailError('Please enter a valid email address.')
    } else {
      setEmailError(null)
    }
  }
  const handlePtoBlur = () => {
    // Check validity on blur
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
      await createEmployee({
        email: employeeEmail,
        country: selectedCountry,
        paidTimeOff: ptoDays,
        managedBy: managerEmail,
        organization: currentOrganization.id,
      })
    }
    if (employee) {
      await updateEmployee({
        country: selectedCountry,
        paidTimeOff: ptoDays,
      })
    }
  }
  return (
    <form onSubmit={handleSubmit}>
      {/* if managerEmail is present, we are editing, and can't edit emails as they are the key */}
      {managerEmail && (
        <div>
          <label>Enter the email *</label>
          <input
            type="email"
            name="headEmail"
            id="headEmail"
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
          placeholder="26"
          required
          min="0"
        />
        {ptoError && <p style={{ color: 'red' }}>{ptoError}</p>}
      </div>

      <button
        type="submit"
        disabled={!!emailError || !!ptoError || !!countryError}
      >
        {employee ? 'Update' : 'Create'} Employee
      </button>
    </form>
  )
}
