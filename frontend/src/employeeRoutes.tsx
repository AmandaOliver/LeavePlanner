import { Routes, Route } from 'react-router-dom'
import { AuthenticationGuard } from './authentication-guard'
import { useEmployeeModel } from './models/Employees'
import { LoadingComponent } from './components/loading'
import { Navigation } from './components/navigation/navigation'
import { CreateOrganizationAndEmployee } from './pages/createOrganization'
import { HomePage } from './pages/Home/home'
import { Leaves } from './pages/myLeaves'
import { MyRequests } from './pages/myRequests'
import { ProfilePage } from './pages/profile'
import { Requests } from './pages/ReviewRequests/requests'
import { SetupOrganization } from './pages/SetupOrganization/setupOrganization'
import { NotFoundPage } from './pages/not-found'

export const EmployeeRoutes = () => {
  const { currentEmployee, isLoading } = useEmployeeModel()
  if (isLoading) {
    return <LoadingComponent />
  }
  return (
    <>
      <Navigation />
      {currentEmployee ? (
        <Routes>
          <Route
            path="/"
            element={<AuthenticationGuard component={HomePage} />}
          />

          <Route
            path="/profile"
            element={<AuthenticationGuard component={ProfilePage} />}
          />
          <Route
            path="/leaves"
            element={<AuthenticationGuard component={Leaves} />}
          />
          <Route
            path="/requests"
            element={<AuthenticationGuard component={MyRequests} />}
          />
          <Route
            path="/requests/:id"
            element={<AuthenticationGuard component={Requests} />}
          />
          {currentEmployee.isOrgOwner && (
            <Route
              path="/setup-organization"
              element={<AuthenticationGuard component={SetupOrganization} />}
            />
          )}

          <Route path="/home" element={<HomePage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      ) : (
        <CreateOrganizationAndEmployee />
      )}
    </>
  )
}
