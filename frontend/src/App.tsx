import './App.css'

import { useAuth0 } from '@auth0/auth0-react'
import LoadingComponent from './components/loading'

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { EmployeeRoutes } from './employeeRoutes'
import { AuthenticationGuard } from './authentication-guard'
const queryClient = new QueryClient()

function App() {
  const { isLoading } = useAuth0()
  if (isLoading) {
    return <LoadingComponent />
  }
  return (
    <QueryClientProvider client={queryClient}>
      <ReactQueryDevtools initialIsOpen={false} />
      <AuthenticationGuard component={EmployeeRoutes} />
    </QueryClientProvider>
  )
}

export default App
