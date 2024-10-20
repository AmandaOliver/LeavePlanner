import type { Meta, StoryObj } from '@storybook/react'
import { Header } from './Header'

import '../../index.css'
const meta = {
  title: 'Header',
  component: Header,

  // This component will have an automatically generated Autodocs entry: https://storybook.js.org/docs/writing-docs/autodocs
  tags: ['autodocs'],
  parameters: {
    // More on how to position stories at: https://storybook.js.org/docs/configure/story-layout
    layout: 'fullscreen',
  },
  args: {},
} satisfies Meta<typeof Header>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {
  args: {
    organizationName: 'ACME',
    menuItems: [
      { link: '/page1', label: 'page1' },
      { link: '/page2', label: 'page2' },
      { link: '/page5', label: 'page5' },
    ],
    activeMenu: '/page2',
    handleLogout: () => {},
    currentUser: {
      email: 'example@email.com',
      pendingRequests: 5,
      avatarPicture: 'https://i.pravatar.cc/150?u=a042581f4e29026704d',
    },
    avatarMenuItems: [
      { link: '/page3', label: 'page3' },
      { link: '/page4', label: 'page4', badge: 5 },
    ],
    mobileMenuItems: [
      { link: '/page1', label: 'page1' },
      { link: '/page2', label: 'page2' },
      { link: '/page4', label: 'page4' },
      { link: '/page5', label: 'page5', badge: 1 },
    ],
  },
}
