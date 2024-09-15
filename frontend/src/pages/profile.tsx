import { useEmployeeModel } from '../models/Employee'

export const ProfilePage = () => {
  const { currentEmployee } = useEmployeeModel()
  return (
    <>
      <h1>Profile Page</h1>
      <div>
        <div>
          <h2>Name: {currentEmployee?.name}</h2>
          <p>Email: {currentEmployee?.email}</p>
          <p>Job Title: {currentEmployee?.title}</p>
          <p>{currentEmployee?.paidTimeOff} days of paid time off per year</p>
          <p>
            {currentEmployee?.paidTimeOffLeft} days of paid time off left this
            year
          </p>
          <p>Resides in {currentEmployee?.country}</p>
          <p>Manager's email: {currentEmployee?.managedBy}</p>
          {currentEmployee?.subordinates?.length ? (
            <>
              <p>Manages:</p>
              <ul>
                {currentEmployee?.subordinates.map((employee) => (
                  <li>{employee.email}</li>
                ))}
              </ul>
            </>
          ) : (
            ''
          )}
        </div>
      </div>
    </>
  )
}
