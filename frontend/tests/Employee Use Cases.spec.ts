/* eslint-disable testing-library/prefer-screen-queries */
import test, { expect } from '@playwright/test'
import { DateTime } from 'luxon'
test('UC-006 Employee Views Personal Calendar', async ({ page }) => {
  await page.goto('/')

  const now = DateTime.now()

  await expect(
    page.getByRole('heading', { name: now.toFormat('MMMM yyyy') })
  ).toBeVisible()
  await page.getByTestId('month-control-next').click()
  const nextMonth = DateTime.now().plus({ months: 1 })

  await expect(
    page.getByRole('heading', {
      name: nextMonth.toFormat('MMMM yyyy'),
    })
  ).toBeVisible()

  await page.getByText('Go to Today').click()
  await expect(
    page.getByRole('heading', { name: now.toFormat('MMMM yyyy') })
  ).toBeVisible()
})
test('UC-009 Employee Views Their Profile', async ({ page }) => {
  await page.goto('/profile')
  await expect(
    page.getByText('You have 15 days of paid time off anually.')
  ).toBeVisible()
})
test('UC-010 Employee Views Their Leaves', async ({ page }) => {
  await page.goto('/leaves')

  await expect(page.getByText('This year ')).toBeVisible()
  await expect(page.getByText('15').first()).toBeVisible()

  await expect(
    page.getByText('Hispanic Day observed (regional holiday)')
  ).toBeVisible()
  await expect(page.getByText('No past leaves')).toBeVisible()
})

test('UC-011 Employee Updates a Leave', async ({ page }) => {
  await page.goto('/leaves')

  await page
    .locator('tr')
    .filter({ hasText: 'Assumption of Mary' })
    .getByLabel('edit')
    .nth(1)
    .click()
  await page
    .getByPlaceholder('I am going to have the trip')
    .fill('Updated description')
  await page.getByRole('button', { name: 'Update' }).click()
  await expect(page.getByRole('dialog')).not.toBeVisible()
  await expect(page.getByText('Updated description')).toBeVisible()
})
test('UC-013 Employee Views Leave Details', async ({ page }) => {
  await page.goto('/leaves')

  await page
    .locator('tr')
    .filter({ hasText: 'May Day' })
    .getByLabel('edit')
    .first()
    .click()
  await expect(page.getByText('Type: Public Holiday')).toBeVisible()
})
test('UC-018 Employee Requests a Leave of type Paid Time Off', async ({
  page,
}) => {
  await page.goto('/leaves')
  await page.getByRole('button', { name: 'Request leave' }).click()
  await page.getByPlaceholder('I am going to have the trip').click()
  await page
    .getByPlaceholder('I am going to have the trip')
    .fill('some description')
  await page.getByRole('button', { name: 'Calendar Days on leave*' }).click()
  await page
    .locator('div')
    .filter({ hasText: /^February 2025March 2025$/ })
    .getByLabel('Next')
    .click()
  await page.getByLabel('April 1,').click()
  await page.getByLabel('May 26, 2025').click()
  await expect(page.getByText('You cannot request')).toBeVisible()
  await page.getByRole('button', { name: 'Calendar Days on leave*' }).click()
  await page.getByLabel('Sunday, April 20,').click()
  await page.getByLabel('Saturday, April 26, 2025').click()
  await expect(page.getByText('Days Requested: 4')).toBeVisible()
  await page.getByRole('button', { name: 'Request' }).click()
  await expect(page.getByText('some description')).toBeVisible()
})
test('UC-014 Employee Views Leave Requests', async ({ page }) => {
  await page.goto('/requests')
  await expect(page.getByText('some description')).toBeVisible()
  await expect(page.getByText('No rejected requests.')).toBeVisible()
})
test('UC-015 Employee Updates a Leave Request', async ({ page }) => {
  await page.goto('/requests')
  await page.getByLabel('edit').nth(3).click()
  await page.getByPlaceholder('I am going to have the trip').click()
  await page
    .getByPlaceholder('I am going to have the trip')
    .fill('some description updated')

  await page.getByRole('button', { name: 'Update' }).click()
  await expect(page.getByText('some description updated')).toBeVisible()
})
test('UC-017 Employee Views Request Details', async ({ page }) => {
  await page.goto('/requests')
  await page.getByLabel('edit').nth(2).click()
  await expect(page.getByText("If approved, you'll have 11")).toBeVisible()
})
test('UC-016 Employee Deletes a Request of type Paid Time Off', async ({
  page,
}) => {
  await page.goto('/requests')
  await page.getByLabel('edit').nth(4).click()
  await page.getByRole('button', { name: 'Delete' }).click()
  await expect(page.getByText('some description updated')).not.toBeVisible()
})
