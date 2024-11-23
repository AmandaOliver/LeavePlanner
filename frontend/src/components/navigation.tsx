import { useLocation } from 'react-router-dom'
import { useEmployeeModel } from '../models/Employee'
import { useOrganizationModel } from '../models/Organization'
import { Header } from '../stories/Header/Header'
import { useAuth0 } from '@auth0/auth0-react'
import { Skeleton, useDisclosure } from '@nextui-org/react'
import { LeaveModal } from './leaveModal'
import { useState } from 'react'

export const Navigation = () => {
  const { user } = useAuth0()

  const { currentEmployee, isLoading: isLoadingEmployee } = useEmployeeModel()
  const { currentOrganization, isLoading: isLoadingOrg } =
    useOrganizationModel()
  const { logout, isLoading: isLoadingUser } = useAuth0()
  const location = useLocation()
  const { isOpen, onOpen, onOpenChange } = useDisclosure()
  const [isLeaveModalOpen, setIsLeaveModalOpen] = useState(false)
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
    { link: '/requests', label: 'My Requests' },
  ]
  const menuItemManager = {
    link: `/requests/${currentEmployee?.email}`,
    label: 'Review leave requests',
    badge: currentEmployee?.pendingRequests,
  }

  const menuItemOrgOwner = {
    link: `/setup-organization`,
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
  if (currentEmployee?.country) {
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
  }
  if (currentEmployee?.isOrgOwner) {
    avatarMenuItems.push(menuItemOrgOwner)
    mobileMenuItems.push(menuItemOrgOwner)
  }
  if (isLoadingEmployee || isLoadingOrg || isLoadingUser)
    return <Skeleton className="w-full h-16" />
  return (
    <>
      {isLeaveModalOpen && (
        <LeaveModal
          isOpen={isOpen}
          onOpenChange={onOpenChange}
          onCloseCb={() => setIsLeaveModalOpen(false)}
        />
      )}

      <Header
        organizationName={currentOrganization?.name}
        menuItems={menuItems}
        activeMenu={location.pathname}
        handleLogout={handleLogout}
        currentUser={currentUser}
        avatarMenuItems={avatarMenuItems}
        mobileMenuItems={mobileMenuItems}
        handleButtonClick={() => {
          setIsLeaveModalOpen(true)
          onOpen()
        }}
        buttonLabel={currentEmployee?.country ? 'Request a leave' : undefined}
      />
    </>
  )
}
