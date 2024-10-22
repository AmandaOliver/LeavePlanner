import { withAuthenticationRequired } from '@auth0/auth0-react'
import { LoadingComponent } from './components/loading'

export const AuthenticationGuard = ({
  component,
}: {
  component: React.ComponentType<object>
}) => {
  const Component = withAuthenticationRequired(component, {
    onRedirecting: () => <LoadingComponent />,
  })

  return <Component />
}
