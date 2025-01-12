import { chromium, FullConfig } from '@playwright/test'

async function globalSetup(config: FullConfig) {
  const browser = await chromium.launch()
  const context = await browser.newContext()
  const page = await context.newPage()

  // Perform login steps
  await page.goto('https://localhost:3000')
  await page.getByRole('button', { name: 'Continue with Google' }).click()
  await page.getByLabel('Email or phone').fill('plannerleave95@gmail.com')
  await page.getByRole('button', { name: 'Next' }).click()
  await page.getByLabel('Enter your password').fill('zap*hex5CKY@pmy3jdm')
  await page.getByRole('button', { name: 'Next' }).click()

  // Wait for navigation or ensure login is complete
  await page.waitForURL('https://localhost:3000/') // Adjust the URL as needed

  // Save the authenticated state
  await context.storageState({ path: 'auth.json' })

  await browser.close()
}

export default globalSetup
