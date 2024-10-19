import { useLocation } from 'react-router-dom'
import { useEmployeeModel } from '../models/Employee'
import { useOrganizationModel } from '../models/Organization'
import { Header } from '../stories/Header/Header'
import { useAuth0 } from '@auth0/auth0-react'

export const Navigation = () => {
  const { user } = useAuth0()
  const { currentEmployee } = useEmployeeModel()
  const { currentOrganization } = useOrganizationModel()
  const { logout } = useAuth0()
  const location = useLocation()

  const handleLogout = () => {
    logout({
      logoutParams: {
        returnTo: window.location.origin,
      },
    })
  }
  const menuItemsEmployee = [
    { link: '/', label: 'Home' },
    { link: '/profile', label: 'My Profile' },
    { link: '/leaves', label: 'My Leaves' },
  ]
  const menuItemsManager = [
    { link: `/requests/${currentEmployee?.email}`, label: 'Leave requests' },
  ]
  const menuItemOrgOwner = {
    link: `/setup-organization/${currentEmployee?.organization}`,
    label: 'Setup your organization',
  }

  const menuItems = []
  if (currentEmployee) {
    menuItems.push(...menuItemsEmployee)
    if (!!currentEmployee.subordinates?.length) {
      menuItems.push(...menuItemsManager)
    }
  }
  const mobileMenuItems = [...menuItems]
  const avatarMenuItems = [{ link: '/profile', label: 'My Profile' }]
  if (currentEmployee?.isOrgOwner) {
    avatarMenuItems.push(menuItemOrgOwner)
    mobileMenuItems.push(menuItemOrgOwner)
  }
  return (
    <Header
      organizationName={currentOrganization?.name}
      menuItems={menuItems}
      activeMenu={location.pathname}
      handleLogout={handleLogout}
      currentEmployee={currentEmployee}
      avatarMenuItems={avatarMenuItems}
      mobileMenuItems={mobileMenuItems}
      avatarPicture={user?.picture || ''}
    />
  )
}
