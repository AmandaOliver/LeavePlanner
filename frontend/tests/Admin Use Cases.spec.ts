/* eslint-disable testing-library/prefer-screen-queries */

import test, { expect } from '@playwright/test'
test('UC-001 User Logs in with Google Credentials', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'avatar' }).click()
  await expect(page.getByText('plannerleave95@gmail.com')).toBeVisible()
  await expect(page.getByText('Get started', { exact: true })).toBeVisible()
})
test('UC-003 User Creates Organization', async ({ page }) => {
  await page.goto('/')
  await page.getByText('Get started', { exact: true }).click()
  await page.getByPlaceholder('Enter your Organization name').click()
  await expect(page.getByRole('button', { name: 'Next step' })).toBeDisabled()

  await page.getByText('Close').click()

  await page.getByText('Get started', { exact: true }).click()
  await page
    .getByPlaceholder('Enter your Organization name')
    .fill('LeavePlanner')
  await page.getByRole('button', { name: 'Next step' }).click()
  await expect(
    page.getByRole('heading', { name: 'Name: LeavePlanner' })
  ).toBeVisible()
  await expect(page.getByRole('button', { name: 'Delete' })).toBeVisible()
})
test('UC-023 Admin Renames Organization', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Rename' }).click()
  await expect(page.getByText('Close')).toBeVisible()
  await page
    .getByPlaceholder('Enter your Organization name')
    .fill('LeavePlannerRenamed')
  await page.getByRole('button', { name: 'Update' }).click()
  await expect(
    page.getByRole('heading', { name: 'Name: LeavePlannerRenamed' })
  ).toBeVisible()
})
test('UC-024 Admin Configures Organization’s Working Days', async ({
  page,
}) => {
  await page.goto('/')

  await expect(
    page.getByRole('heading', {
      name: 'Working Days: Monday, Tuesday, Wednesday, Thursday, Friday',
    })
  ).toBeVisible()
  await page.getByRole('button', { name: 'Configure working days' }).click()
  await page.getByLabel('Friday').click()
  await page.getByText('Close').click()
  await page.getByRole('button', { name: 'Configure working days' }).click()
  await page.getByLabel('Friday').click()
  await page.getByRole('button', { name: 'Update' }).click()
  await expect(
    page.getByRole('heading', {
      name: 'Working Days: Monday, Tuesday, Wednesday, Thursday',
    })
  ).toBeVisible()
})
test('UC-027 Admin Creates the Head Employee', async ({ page }) => {
  await page.goto('/setup-organization')
  await page.getByRole('button', { name: 'Create Employees Manually' }).click()
  await page
    .getByPlaceholder('name@yourorganization.com')
    .fill('notifications.leaveplanner@gmail.com')
  await page.getByPlaceholder('Developer').fill('CEO')
  await page.getByPlaceholder('Enter the employee full name').fill('Head')
  await page.getByPlaceholder('Type to search...').click()
  await page.getByPlaceholder('Type to search...').fill('Spain')
  await page.getByPlaceholder('Type to search...').click()

  await page.getByPlaceholder('Enter a number').click()
  await page.getByPlaceholder('Enter a number').fill('15')
  await page.getByRole('button', { name: 'Create' }).click()
  await expect(
    page.getByRole('button', { name: 'Update employee' })
  ).toBeVisible()
})
test('UC-029 Admin Updates an Employee', async ({ page }) => {
  await page.goto('/setup-organization')

  await page.getByRole('button', { name: 'Update employee' }).click()
  await page.getByPlaceholder('Developer').fill('QA')
  await page.getByRole('button', { name: 'Update' }).click()
  await expect(page.getByText('Title: QA')).toBeVisible()
})
test('UC-028 Admin Creates a Subordinate Employee', async ({ page }) => {
  await page.goto('/setup-organization')
  await page.getByRole('button', { name: 'Add Subordinate' }).click()
  await page
    .getByPlaceholder('name@yourorganization.com')
    .fill('notifications.leaveplanner@gmail.com')
  await page.getByPlaceholder('Developer').fill('fakeQA')
  await page
    .getByPlaceholder('Enter the employee full name')
    .fill('Planner Tester Subordinate')
  await page.getByPlaceholder('Type to search...').click()
  await page.getByPlaceholder('Type to search...').fill('Spain')
  await page.getByPlaceholder('Type to search...').click()

  await page.getByPlaceholder('Enter a number').click()
  await page.getByPlaceholder('Enter a number').fill('15')
  await page.getByRole('button', { name: 'Create' }).click()
  await expect(page.getByText('There is an existing ')).toBeVisible()
  await page
    .getByPlaceholder('name@yourorganization.com')
    .fill('something@gmail.com')
  await page.getByRole('button', { name: 'Create' }).click()
  await page.getByText('See subordinates').click()
  await expect(page.getByText('something@gmail.com')).toBeVisible()
  await expect(page.getByText('Title: fakeQA')).toBeVisible()

  await page.goto('/setup-organization')
  await page.getByRole('button', { name: 'Add Subordinate' }).click()
  await page
    .getByPlaceholder('name@yourorganization.com')
    .fill('plannerleave95@gmail.com')
  await page.getByLabel('Has Admin Rights').check()

  await page.getByPlaceholder('Developer').fill('subQA')
  await page
    .getByPlaceholder('Enter the employee full name')
    .fill('Planner Tester')
  await page.getByPlaceholder('Type to search...').click()
  await page.getByPlaceholder('Type to search...').fill('Spain')
  await page.getByPlaceholder('Enter a number').click()
  await page.getByPlaceholder('Enter a number').fill('15')
  await page.getByRole('button', { name: 'Create' }).click()
})
test('UC-030 Admin Deletes an Employee', async ({ page }) => {
  await page.goto('/setup-organization')
  await page.getByText('See subordinates').click()
  await expect(page.getByText('Title: fakeQA')).toBeVisible()

  await page.getByTestId('delete-something@gmail.com').click()
  await page.getByRole('button', { name: 'Delete' }).click()
  await expect(page.getByText('Title: fakeQA')).not.toBeVisible()
})
