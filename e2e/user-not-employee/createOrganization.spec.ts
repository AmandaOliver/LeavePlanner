import { test, expect, type Page } from '@playwright/test'

test.beforeEach(async ({ page }) => {
  await page.goto('https://leaveplanner.org')
})

test.describe('UC-003 User Creates Organization', () => {
  test('Main Scenario: Successful Organization Creation', async ({
    page,
  }) => {})
})
