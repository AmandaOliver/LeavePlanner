import { Routes, Route } from 'react-router-dom'
import { AuthenticationGuard } from '../authentication-guard'
import { useEmployeeModel } from '../models/Employee'
import CallbackPage from '../pages/callback'
import { CreateOrganizationAndEmployee } from '../pages/createOrganizationAndEmployee'
import { HomePage } from '../pages/home'
import { NotFoundPage } from '../pages/not-found'
import { ProfilePage } from '../pages/profile'
import { Navigation } from './navigation'
import LoadingPage from '../pages/loading'
import { SetupOrganization } from '../pages/setupOrganization'

export const EmployeeRoutes = () => {
  const { currentEmployee, isLoading } = useEmployeeModel()
  if (isLoading) {
    return <LoadingPage />
  }
  return currentEmployee ? (
    <>
      <Navigation />

      <Routes>
        <Route
          path="/"
          element={<AuthenticationGuard component={HomePage} />}
        />
        <Route
          path="/profile"
          element={<AuthenticationGuard component={ProfilePage} />}
        />
        {currentEmployee?.isOrgOwner && (
          <Route
            path="/setup-organization/:id"
            element={<AuthenticationGuard component={SetupOrganization} />}
          />
        )}
        <Route path="/callback" element={<CallbackPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </>
  ) : (
    <CreateOrganizationAndEmployee />
  )
}
