import { chromium, FullConfig } from '@playwright/test'

/**
 * Signs in once and caches the browser state in auth.json, which every spec reuses.
 * See README.md > Running the e2e tests.
 */
async function globalSetup(config: FullConfig) {
  const baseUrl = process.env.E2E_BASE_URL ?? 'https://localhost:3000'
  const email = process.env.E2E_USER
  const password = process.env.E2E_PASSWORD

  if (!email || !password) {
    throw new Error(
      'E2E_USER and E2E_PASSWORD must be set before running the e2e tests.\n' +
        'See README.md > Running the e2e tests.'
    )
  }

  const browser = await chromium.launch()
  const context = await browser.newContext()
  const page = await context.newPage()

  // Signs in against the Auth0 test tenant's database connection rather than a social
  // provider, which would block automated sign-ins.
  await page.goto(baseUrl)
  await page.getByRole('button', { name: 'Log In' }).click()
  await page.getByLabel('Email address').fill(email)
  await page.getByLabel('Password').fill(password)
  await page.getByRole('button', { name: 'Continue', exact: true }).click()

  await page.waitForURL(`${baseUrl}/`)

  await context.storageState({ path: 'auth.json' })

  await browser.close()
}

export default globalSetup
