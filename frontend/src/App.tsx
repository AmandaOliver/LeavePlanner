import { useAuth0 } from '@auth0/auth0-react'

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { EmployeeRoutes } from './employeeRoutes'
import { AuthenticationGuard } from './authentication-guard'
import { LoadingComponent } from './components/loading'
const queryClient = new QueryClient()

function App() {
  const { isLoading } = useAuth0()
  if (isLoading) {
    return <LoadingComponent />
  }
  return (
    <QueryClientProvider client={queryClient}>
      <AuthenticationGuard component={EmployeeRoutes} />
    </QueryClientProvider>
  )
}

export default App
