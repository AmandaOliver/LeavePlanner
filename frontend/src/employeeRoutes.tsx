import { Routes, Route } from 'react-router-dom'
import { AuthenticationGuard } from './authentication-guard'
import { useEmployeeModel } from './models/Employee'
import CallbackPage from './pages/callback'
import { CreateOrganizationAndEmployee } from './pages/createOrganizationAndEmployee'
import { HomePage } from './pages/home'
import { NotFoundPage } from './pages/not-found'
import { ProfilePage } from './pages/profile'
import { Navigation } from './components/navigation'
import { SetupOrganization } from './pages/setupOrganization'
import { Leaves } from './pages/leaves'
import { Requests } from './pages/requests'
import { MyRequests } from './pages/myRequests'
import { LoadingComponent } from './components/loading'

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

          <Route path="/callback" element={<CallbackPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      ) : (
        <CreateOrganizationAndEmployee />
      )}
    </>
  )
}
