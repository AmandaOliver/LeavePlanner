import { withAuthenticationRequired } from '@auth0/auth0-react'
import LoadingPage from './pages/loading'

export const AuthenticationGuard = ({
  component,
}: {
  component: React.ComponentType<object>
}) => {
  const Component = withAuthenticationRequired(component, {
    onRedirecting: () => <LoadingPage />,
  })

  return <Component />
}
