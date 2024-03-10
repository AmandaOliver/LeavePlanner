import { NavLink } from 'react-router-dom'
import { LogoutButton } from './logoutButton'

export const Navigation = () => {
  return (
    <nav>
      <>
        <NavLink to="/">home</NavLink>
        <NavLink to="profile">profile</NavLink>
        <LogoutButton />
      </>
    </nav>
  )
}
