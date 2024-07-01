import {
  ChangeEvent,
  Dispatch,
  FormEvent,
  SetStateAction,
  useRef,
  useState,
} from 'react'
import { EmployeeType, CreateEmployeeParamType } from '../models/Employee'
import { OrganizationType } from '../models/Organization'
import { CountriesType } from '../models/Countries'

export const SetupHead = ({
  currentOrganization,
  createEmployee,
  countries,
  setHead,
}: {
  currentOrganization: OrganizationType
  createEmployee: (param: CreateEmployeeParamType) => Promise<EmployeeType>
  countries: CountriesType
  setHead: Dispatch<SetStateAction<EmployeeType>>
}) => {
  const [orgHeadEmail, setOrgHeadEmail] = useState('')
  const [selectedCountry, setSelectedCountry] = useState('')
  const [ptoDays, setPtoDays] = useState(0)
  const [emailError, setEmailError] = useState<string | null>(null)
  const [ptoError, setPtoError] = useState<string | null>(null)

  const emailRef = useRef<HTMLInputElement>(null)
  const ptoRef = useRef<HTMLInputElement>(null)

  const handleEmailChange = (event: ChangeEvent<HTMLInputElement>) => {
    setOrgHeadEmail(event.target.value)
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
  const handleCountryChange = (event: ChangeEvent<HTMLInputElement>) => {
    setSelectedCountry(event.target.value)
  }
  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const head = await createEmployee({
      email: orgHeadEmail,
      country: selectedCountry,
      paidTimeOff: ptoDays,
      isManager: false,
      organization: currentOrganization.id,
    })
    setHead(head)
  }
  return (
    <form onSubmit={handleSubmit}>
      <div>
        <h1>
          To setup the employee hierarchy of {currentOrganization?.name}, first,
          setup the head of the organization
        </h1>
        <label>Enter the email *</label>
        <input
          type="email"
          name="headEmail"
          id="headEmail"
          value={orgHeadEmail}
          onChange={handleEmailChange}
          onBlur={handleEmailBlur}
          placeholder="head@yourorganization.com"
          required
          ref={emailRef}
        />
        {emailError && <p style={{ color: 'red' }}>{emailError}</p>}
      </div>
      <div>
        <label>Enter the country *</label>
        <input
          list="countries"
          id="countryInput"
          name="country"
          value={selectedCountry}
          onChange={handleCountryChange}
          placeholder="Type to search..."
          required
        />
        <datalist id="countries">
          {countries.map((country) => (
            <option key={country.code} value={country.name}>
              {country.name}
            </option>
          ))}
        </datalist>
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
        disabled={!!emailError || !!ptoError || !selectedCountry}
      >
        Next step
      </button>
    </form>
  )
}
