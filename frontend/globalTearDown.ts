import { chromium } from '@playwright/test'
const logout = async (page) => {
  // Perform the logout steps
  await page.getByRole('button', { name: 'avatar' }).click()
  await page.getByText('Log Out').click()

  // Verify the user is logged out (optional, but recommended)
  await page.getByRole('button', { name: 'Continue with Google' })
}
async function globalTeardown() {
  // Launch the browser
  const browser = await chromium.launch()
  const context = await browser.newContext({
    storageState: 'auth.json', // Use the logged-in state from tests
  })
  const page = await context.newPage()

  // Navigate to the homepage
  await page.goto('https://localhost:3000/setup-organization')
  await page.getByRole('button', { name: 'Delete' }).click()
  await page.getByRole('button', { name: 'Delete' }).click()
  await logout(page)

  // Close the browser
  await browser.close()
}

export default globalTeardown
