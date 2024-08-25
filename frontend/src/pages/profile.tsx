import { useEmployeeModel } from '../models/Employee'

export const ProfilePage = () => {
  const { currentEmployee } = useEmployeeModel()

  return (
    <>
      <h1>Profile Page</h1>
      <div>
        <img src={currentEmployee?.picture} alt="Profile" />
        <div>
          <h2>{currentEmployee?.name}</h2>
          <p>{currentEmployee?.email}</p>
          <p>{currentEmployee?.paidTimeOff} days of paid time off</p>
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
