import { NavLink } from 'react-router-dom'
import { LogoutButton } from './logoutButton'
import { useEmployeeModel } from '../models/Employee'
import { useOrganizationModel } from '../models/Organization'
import logo from '../../src/android-chrome-192x192.png' // Adjust the path to your logo

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
          </NavLink>
        </li>
        <li>
          <NavLink to="/profile">Profile</NavLink>
        </li>
        {currentEmployee?.isOrgOwner && currentOrganization?.id && (
          <li>
            <NavLink to={`/setup-organization/${currentOrganization?.id}`}>
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
