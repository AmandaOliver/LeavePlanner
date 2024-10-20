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
  const avatarMenuItemsEmployee = { link: '/profile', label: 'My Profile' }

  const menuItemsEmployee = [
    { link: '/', label: 'Home' },
    { link: '/profile', label: 'My Profile' },
    { link: '/leaves', label: 'My Leaves' },
  ]
  const menuItemManager = {
    link: `/requests/${currentEmployee?.email}`,
    label: 'Leave requests',
    badge: currentEmployee?.pendingRequests,
  }

  const menuItemOrgOwner = {
    link: `/setup-organization/${currentEmployee?.organization}`,
    label: 'Setup your organization',
  }

  const menuItems = []
  const mobileMenuItems = []
  const avatarMenuItems = []
  let currentUser = {
    email: user?.email || '',
    pendingRequests: 0,
    avatarPicture:
      user?.picture || 'https://cdn-icons-png.flaticon.com/512/126/126486.png',
  }
  if (currentEmployee) {
    currentUser = {
      ...currentUser,
      email: currentEmployee.email,
      pendingRequests: currentEmployee.pendingRequests,
    }
    avatarMenuItems.push(avatarMenuItemsEmployee)
    menuItems.push(...menuItemsEmployee)
    mobileMenuItems.push(...menuItemsEmployee)
    if (!!currentEmployee.subordinates?.length) {
      avatarMenuItems.push(menuItemManager)
      mobileMenuItems.push(menuItemManager)
    }
    if (currentEmployee.isOrgOwner) {
      avatarMenuItems.push(menuItemOrgOwner)
      mobileMenuItems.push(menuItemOrgOwner)
    }
  }
  return (
    <Header
      organizationName={currentOrganization?.name}
      menuItems={menuItems}
      activeMenu={location.pathname}
      handleLogout={handleLogout}
      currentUser={currentUser}
      avatarMenuItems={avatarMenuItems}
      mobileMenuItems={mobileMenuItems}
    />
  )
}
