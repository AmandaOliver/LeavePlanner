import React, { useState } from 'react'
import {
  Navbar,
  NavbarBrand,
  NavbarContent,
  NavbarItem,
  NavbarMenuToggle,
  NavbarMenu,
  NavbarMenuItem,
  Button,
  Dropdown,
  DropdownTrigger,
  Avatar,
  DropdownMenu,
  DropdownItem,
  Badge,
  Chip,
} from '@nextui-org/react'
import { NavLink } from 'react-router-dom'
import { Logo } from './logo'

type MenuItemType = {
  link: string
  label: string
  badge?: number
}
type PropsType = {
  organizationName: string
  menuItems: MenuItemType[]
  activeMenu: string
  handleLogout: () => void
  currentUser: {
    email: string
    pendingRequests?: number
    avatarPicture: string
  }
  avatarMenuItems?: MenuItemType[]
  mobileMenuItems: MenuItemType[]
  handleButtonClick: () => void
  buttonLabel?: string
}
export function Header({
  organizationName,
  menuItems,
  activeMenu,
  handleLogout,
  currentUser,
  avatarMenuItems,
  mobileMenuItems,
  handleButtonClick,
  buttonLabel,
}: PropsType) {
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [isImageError, setIsImageError] = useState(false)
  return (
    <Navbar
      isMenuOpen={isMenuOpen}
      onMenuOpenChange={setIsMenuOpen}
      isBordered
      maxWidth={'full'}
      classNames={{
        item: [
          'flex',
          'relative',
          'h-full',
          'items-center',
          "data-[active=true]:after:content-['']",
          'data-[active=true]:after:absolute',
          'data-[active=true]:after:bottom-0',
          'data-[active=true]:after:left-0',
          'data-[active=true]:after:right-0',
          'data-[active=true]:after:h-[2px]',
          'data-[active=true]:after:rounded-[2px]',
          'data-[active=true]:after:bg-primary',
        ],
      }}
    >
      <NavbarContent>
        <NavbarBrand>
          <Logo />
          <p className="font-bold text-inherit ml-4">{organizationName}</p>
        </NavbarBrand>
      </NavbarContent>

      <NavbarContent className="hidden sm:flex gap-4" justify="center">
        {menuItems.map((menuItem, index) => {
          const isActive = menuItem.link === activeMenu
          return (
            <NavbarItem isActive={isActive} key={`${menuItem.link}-${index}`}>
              <NavLink to={menuItem.link}>{menuItem.label}</NavLink>
            </NavbarItem>
          )
        })}
      </NavbarContent>

      <NavbarContent justify="end">
        {buttonLabel && (
          <Button color="primary" onPress={handleButtonClick}>
            {buttonLabel}
          </Button>
        )}

        <div className="hidden sm:flex flex-wrap flex-grow-0 w-fit justify-items-end">
          <Dropdown placement="bottom-end">
            <Badge
              content={currentUser.pendingRequests}
              color="primary"
              size="lg"
              isInvisible={!currentUser.pendingRequests}
            >
              <DropdownTrigger>
                <Avatar
                  isBordered
                  as="button"
                  className="transition-transform"
                  color="primary"
                  size="sm"
                  src={
                    isImageError ? '/userIcon.svg' : currentUser.avatarPicture
                  }
                  onError={() => setIsImageError(true)}
                />
              </DropdownTrigger>
            </Badge>

            <DropdownMenu aria-label="Profile Actions" variant="flat">
              <DropdownItem key="profile" className="h-14 gap-2" showDivider>
                <p className="font-semibold">Signed in as</p>
                <p className="font-bold">{currentUser.email}</p>
              </DropdownItem>
              <>
                {avatarMenuItems?.length &&
                  avatarMenuItems.map((menuItem) => (
                    <DropdownItem key={menuItem.label}>
                      {menuItem.badge ? (
                        <Chip
                          color="primary"
                          className="mr-1"
                          size="sm"
                          radius="full"
                          variant="shadow"
                        >
                          {menuItem.badge}
                        </Chip>
                      ) : (
                        ''
                      )}
                      <NavLink to={menuItem.link}>{menuItem.label}</NavLink>
                    </DropdownItem>
                  ))}
              </>

              <DropdownItem
                key="logout"
                className="text-danger"
                color="danger"
                onClick={handleLogout}
              >
                Log Out
              </DropdownItem>
            </DropdownMenu>
          </Dropdown>
        </div>
      </NavbarContent>
      <Badge
        content={currentUser.pendingRequests}
        color="primary"
        size="md"
        isInvisible={!currentUser.pendingRequests}
        className="sm:hidden "
      >
        <NavbarMenuToggle
          aria-label={isMenuOpen ? 'Close menu' : 'Open menu'}
          className="sm:hidden w-9 h-9"
        />
      </Badge>

      <NavbarMenu>
        {mobileMenuItems.map((menuItem, index) => {
          const isActive = menuItem.link === activeMenu

          return (
            <NavbarMenuItem
              isActive={isActive}
              key={`${menuItem.link}-${index}`}
            >
              {menuItem.badge ? (
                <Chip
                  color="primary"
                  className="mr-1"
                  size="sm"
                  radius="full"
                  variant="shadow"
                >
                  {menuItem.badge}
                </Chip>
              ) : (
                ''
              )}
              <NavLink onClick={() => setIsMenuOpen(false)} to={menuItem.link}>
                {menuItem.label}
              </NavLink>
            </NavbarMenuItem>
          )
        })}
        <NavbarMenuItem className="w-full h-full mb-3 content-end flex-wrap flex justify-end">
          <Button onClick={handleLogout} color="danger" variant="bordered">
            Logout
          </Button>
        </NavbarMenuItem>
      </NavbarMenu>
    </Navbar>
  )
}
