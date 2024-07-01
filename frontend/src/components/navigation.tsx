import { NavLink } from 'react-router-dom'
import { LogoutButton } from './logoutButton'
import { useEmployeeModel } from '../models/Employee'
import { useOrganizationModel } from '../models/Organization'

export const Navigation = () => {
  const { currentEmployee } = useEmployeeModel()
  const { currentOrganization } = useOrganizationModel()
  return (
    <nav>
      <ul>
        <li>
          <NavLink to="/">Home</NavLink>
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
