import { chromium, FullConfig } from '@playwright/test'

/**
 * Signs in once and caches the authenticated browser state in auth.json, which the
 * test suite reuses so individual specs never touch the login flow.
 *
 * Credentials come from the environment -- never from source. Copy .env.example to
 * .env.local and fill it in for local runs; in CI, supply them as repository secrets.
 * See README.md > Running the e2e tests.
 */
async function globalSetup(config: FullConfig) {
  const baseUrl = process.env.E2E_BASE_URL ?? 'https://localhost:3000'
  const email = process.env.E2E_USER
  const password = process.env.E2E_PASSWORD

  if (!email || !password) {
    throw new Error(
      'E2E_USER and E2E_PASSWORD must be set before running the e2e tests.\n' +
        'Copy frontend/.env.example to frontend/.env.local and fill in a test-tenant ' +
        'user, or set them in your CI secrets. See README.md > Running the e2e tests.'
    )
  }

  const browser = await chromium.launch()
  const context = await browser.newContext()
  const page = await context.newPage()

  // Sign in against the Auth0 test tenant's database connection. Driving a real Google
  // account here would be both a credential we cannot rotate cheaply and a flaky
  // dependency -- Google actively blocks automated sign-ins.
  await page.goto(baseUrl)
  await page.getByRole('button', { name: 'Log In' }).click()
  await page.getByLabel('Email address').fill(email)
  await page.getByLabel('Password').fill(password)
  await page.getByRole('button', { name: 'Continue', exact: true }).click()

  await page.waitForURL(`${baseUrl}/`)

  // Save the authenticated state
  await context.storageState({ path: 'auth.json' })

  await browser.close()
}

export default globalSetup
