import './App.css'
import { Routes, Route, useNavigate } from 'react-router-dom'
import { HomePage } from './pages/home'

import { NotFoundPage } from './pages/not-found'
import CallbackPage from './pages/callback'
import { useAuth0 } from '@auth0/auth0-react'
import LoadingPage from './pages/loading'
import { AuthenticationGuard } from './authentication-guard'
import { ProfilePage } from './pages/profile'
import { useEffect, useState } from 'react'
import { CreateCompany } from './pages/createCompany'

function App() {
  const { isLoading, user } = useAuth0()
  const [verifyingUser, setVerifyingUser] = useState(true)
  const [isEmployee, setIsEmployee] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    console.log(user)
    const checkUser = async () => {
      setVerifyingUser(true)
      console.log('verifying')
      try {
        const response = await fetch(
          `${process.env.REACT_APP_API_SERVER_URL}/api/employee/check-employee?email=${user?.email}`,
          {
            method: 'GET',
            headers: {
              'Content-Type': 'application/json',
            },
          }
        )
        setIsEmployee(true)
      } catch (error) {
        setIsEmployee(false)
      }
      setVerifyingUser(false)
    }
    if (user?.email) {
      checkUser()
    }
  }, [user])

  if (isLoading) {
    return <LoadingPage />
  }
  // if (!isEmployee) {
  //   navigate('create-company')
  // }
  return (
    <Routes>
      <Route path="/" element={<AuthenticationGuard component={HomePage} />} />
      <Route
        path="/profile"
        element={<AuthenticationGuard component={ProfilePage} />}
      />
      {/* <Route
        path="/create-company"
        element={<AuthenticationGuard component={CreateCompany} />}
      /> */}
      <Route path="/callback" element={<CallbackPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

export default App
