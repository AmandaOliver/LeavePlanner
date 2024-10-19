import React from 'react'
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
  menuItem,
} from '@nextui-org/react'
import { NavLink } from 'react-router-dom'
import { Logo } from './logo'

type MenuItemType = {
  link: string
  label: string
}
type PropsType = {
  organizationName: string
  menuItems: MenuItemType[]
  activeMenu: string
  handleLogout: () => void
  currentEmployee?: null | {
    name: string
    email: string
  }
  avatarMenuItems?: MenuItemType[]
  avatarPicture: string
  mobileMenuItems: MenuItemType[]
}
export function Header({
  organizationName,
  menuItems,
  activeMenu,
  handleLogout,
  currentEmployee,
  avatarMenuItems,
  avatarPicture,
  mobileMenuItems,
}: PropsType) {
  const [isMenuOpen, setIsMenuOpen] = React.useState(false)

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
          'data-[active=true]:after:bg-default',
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
        {currentEmployee && (
          <Dropdown placement="bottom-end">
            <DropdownTrigger className="hidden sm:block">
              <Avatar
                isBordered
                as="button"
                className="transition-transform"
                color="default"
                size="sm"
                src={avatarPicture}
              />
            </DropdownTrigger>
            <DropdownMenu aria-label="Profile Actions" variant="flat">
              <DropdownItem key="profile" className="h-14 gap-2">
                <p className="font-semibold">Signed in as</p>
                <p className="font-bold">{currentEmployee.email}</p>
              </DropdownItem>
              <>
                {avatarMenuItems?.length &&
                  avatarMenuItems.map((menuItem) => (
                    <DropdownItem key={menuItem.label}>
                      <NavLink to={menuItem.link}>{menuItem.label}</NavLink>
                    </DropdownItem>
                  ))}
              </>

              <DropdownItem key="logout" color="danger" onClick={handleLogout}>
                Log Out
              </DropdownItem>
            </DropdownMenu>
          </Dropdown>
        )}
        <NavbarMenuToggle
          aria-label={isMenuOpen ? 'Close menu' : 'Open menu'}
          className="sm:hidden"
        />
      </NavbarContent>
      <NavbarMenu>
        {mobileMenuItems.map((menuItem, index) => {
          const isActive = menuItem.link === activeMenu

          return (
            <NavbarMenuItem
              isActive={isActive}
              key={`${menuItem.link}-${index}`}
            >
              <NavLink onClick={() => setIsMenuOpen(false)} to={menuItem.link}>
                {menuItem.label}
              </NavLink>
            </NavbarMenuItem>
          )
        })}
        <NavbarMenuItem className="w-full h-full mb-3 content-end">
          <Button onClick={handleLogout} color="danger" variant="bordered">
            Logout
          </Button>
        </NavbarMenuItem>
      </NavbarMenu>
    </Navbar>
  )
}
