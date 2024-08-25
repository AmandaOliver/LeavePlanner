import './App.css'

import { useAuth0 } from '@auth0/auth0-react'
import LoadingPage from './pages/loading'

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { EmployeeRoutes } from './components/employeeRoutes'

const queryClient = new QueryClient()

function App() {
  const { isLoading } = useAuth0()
  if (isLoading) {
    return <LoadingPage />
  }
  return (
    <QueryClientProvider client={queryClient}>
      <ReactQueryDevtools initialIsOpen={false} />
      <EmployeeRoutes />
    </QueryClientProvider>
  )
}

export default App
