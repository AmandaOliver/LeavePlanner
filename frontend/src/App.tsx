import './App.css'
import { Routes, Route } from 'react-router-dom'
import { HomePage } from './pages/home'

import { NotFoundPage } from './pages/not-found'
import CallbackPage from './pages/callback'
import { useAuth0 } from '@auth0/auth0-react'
import LoadingPage from './pages/loading'
import { AuthenticationGuard } from './authentication-guard'
import { ProfilePage } from './pages/profile'
import { CreateOrganizationAndEmployee } from './pages/createOrganizationAndEmployee'
import { SetupOrganization } from './pages/setupOrganization'
import { Navigation } from './components/navigation'

function App() {
  const { isLoading } = useAuth0()

  if (isLoading) {
    return <LoadingPage />
  }
  return (
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
        <Route
          path="/create-organization"
          element={
            <AuthenticationGuard component={CreateOrganizationAndEmployee} />
          }
        />
        <Route
          path="/setup-organization/:id"
          element={<AuthenticationGuard component={SetupOrganization} />}
        />
        <Route path="/callback" element={<CallbackPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </>
  )
}

export default App
