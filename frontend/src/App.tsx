import './App.css'
import { Routes, Route } from 'react-router-dom'
import { HomePage } from './pages/home'

import { NotFoundPage } from './pages/not-found'
import CallbackPage from './pages/callback'
import { useAuth0 } from '@auth0/auth0-react'
import LoadingPage from './pages/loading'
import { AuthenticationGuard } from './authentication-guard'
import { ProfilePage } from './pages/profile'

function App() {
  const { isLoading } = useAuth0()

  if (isLoading) {
    return <LoadingPage />
  }
  return (
    <Routes>
      <Route path="/" element={<AuthenticationGuard component={HomePage} />} />
      <Route
        path="/profile"
        element={<AuthenticationGuard component={ProfilePage} />}
      />

      <Route path="/callback" element={<CallbackPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

export default App
