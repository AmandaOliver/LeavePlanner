import { NavLink } from 'react-router-dom'
import { LogoutButton } from './logoutButton'
import { useEmployeeModel } from '../models/Employee'
import logo from '../../src/android-chrome-192x192.png'
import { useOrganizationModel } from '../models/Organization'

export const Navigation = () => {
  const { currentEmployee } = useEmployeeModel()
  const { currentOrganization } = useOrganizationModel()
  return (
    <nav>
      <ul>
        <li>
          <NavLink to="/">
            <img
              src={logo}
              alt="LeavePlanner Logo"
              style={{ height: '40px', width: '40px' }}
            />
            <h1>{currentOrganization?.name}</h1>
          </NavLink>
        </li>
        {currentEmployee && (
          <li>
            <NavLink to="/profile">Profile</NavLink>
          </li>
        )}
        {currentEmployee?.isOrgOwner && (
          <li>
            <NavLink
              to={`/setup-organization/${currentEmployee?.organization}`}
            >
              Setup your organization
            </NavLink>
          </li>
        )}
        <li>
          <LogoutButton />
        </li>
      </ul>
    </nav>
  )
}
