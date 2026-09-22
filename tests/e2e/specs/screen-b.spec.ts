import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, signIn } from './helpers'

// Screen B (SCR-002) user journeys — DD-002 E-level test viewpoints (TC-101, TC-109, TC-113, TC-114, TC-115, TC-116).
// The stack carries the 80 seeded demo orders (DB-003); assertions use counts and relative dates, never absolute ones,
// because the seed's due dates follow the migration's run date (DEC-011).

const summary = /^\d+–\d+ of \d+ orders$/

test.beforeEach(async ({ page }) => {
  await signIn(page)
})

async function openList(page: import('@playwright/test').Page, query = '') {
  await page.goto(`/production-orders${query}`)
  await expect(page.getByRole('heading', { level: 1, name: 'Production orders' })).toBeVisible()
}

test('opens from the home page with the default view, and is accessible (TC-101)', async ({ page }) => {
  await page.getByRole('link', { name: 'Production orders' }).click();

  await expect(page).toHaveURL(/\/production-orders$/)
  await expect(page).toHaveTitle('Production orders — ProductionManagementAI')
  await expect(page.getByRole('status').first()).toHaveText(summary)

  const rows = page.getByRole('table').getByRole('row')
  await expect(rows).toHaveCount(21) // header + the default page size of 20

  // Default sort: due date ascending, marked on that column only.
  const sorted = page.getByRole('columnheader', { name: /Due date/ })
  await expect(sorted).toHaveAttribute('aria-sort', 'ascending')
  await expect(page.getByRole('columnheader').filter({ has: page.locator('[aria-sort]') })).toHaveCount(0)

  // No filter is pre-applied, so terminal statuses appear too (DEC-006).
  await page.getByLabel('Rows').selectOption('100')
  await expect(page.getByRole('table').getByText('Completed').first()).toBeVisible()
  await expect(page.getByRole('table').getByText('Cancelled').first()).toBeVisible()

  await expectNoAxeViolations(page)
})

test('sorting reorders the whole result set and toggles direction (TC-109)', async ({ page }) => {
  await openList(page)

  await page.getByRole('button', { name: /Order no\./ }).click()
  await expect(page).toHaveURL(/sort=orderNumber/)
  const ascending = await page.getByRole('table').getByRole('link').allInnerTexts()
  expect(ascending).toEqual([...ascending].sort())

  await page.getByRole('button', { name: /Order no\./ }).click()
  await expect(page).toHaveURL(/dir=desc/)
  const descending = await page.getByRole('table').getByRole('link').allInnerTexts()
  expect(descending).toEqual([...descending].sort().reverse())
  // Descending page 1 is not ascending page 1, i.e. the sort reached the database, not just this page.
  expect(descending[0]).not.toEqual(ascending[0])

  await expectNoAxeViolations(page)
})

test('filtering, paging and page size keep the view in the URL (TC-115, TC-116)', async ({ page }) => {
  await openList(page)

  await page.getByLabel('Draft').check()
  await page.getByRole('button', { name: 'Search' }).click()
  await expect(page).toHaveURL(/status=Draft/)
  await expect(page.getByRole('status').first()).toHaveText(summary)
  const draftTotal = (await page.getByRole('status').first().textContent())!

  // Paging keeps the filter, and the page number is in the URL.
  await page.getByLabel('Rows').selectOption('10')
  await expect(page).toHaveURL(/pageSize=10/)
  await page.getByRole('button', { name: /Next/ }).click()
  await expect(page).toHaveURL(/page=2/)
  await expect(page).toHaveURL(/status=Draft/)

  // Reloading the URL reproduces the same view.
  const url = page.url()
  await page.reload()
  await expect(page).toHaveURL(url)
  await expect(page.getByLabel('Draft')).toBeChecked()
  await expect(page.getByText('Page 2 of', { exact: false })).toBeVisible()

  // Back returns to the previous view.
  await page.goBack()
  await expect(page.getByText('Page 1 of', { exact: false })).toBeVisible()

  // A filter that matches nothing shows the no-match state, keeping the values, and Clear filters restores the list.
  await page.getByLabel('Order number').fill('ZZZNOMATCH')
  await page.getByRole('button', { name: 'Search' }).click()
  await expect(page.getByText('No orders match your filters.')).toBeVisible()
  await expect(page.getByLabel('Order number')).toHaveValue('ZZZNOMATCH')
  await expectNoAxeViolations(page)

  await page.getByRole('button', { name: 'Clear filters' }).click()
  await expect(page.getByRole('status').first()).toHaveText(summary)
  expect(await page.getByRole('status').first().textContent()).not.toEqual(draftTotal)
})

test('an invalid filter is rejected inline and leaves the rows alone (V-10)', async ({ page }) => {
  await openList(page)
  const firstOrder = await page.getByRole('table').getByRole('link').first().innerText()

  await page.getByLabel('Due from').fill('2026-12-31')
  await page.getByLabel('Due to').fill('2026-01-01')
  await page.getByRole('button', { name: 'Search' }).click()

  await expect(page.getByText("'Due from' must be on or before 'Due to'.")).toBeVisible()
  await expect(page.getByLabel('Due from')).toBeFocused()
  await expect(page.getByRole('table').getByRole('link').first()).toHaveText(firstOrder)
  await expectNoAxeViolations(page)
})

test('a row opens Screen A by mouse and by keyboard, and New opens create mode (TC-113)', async ({ page }) => {
  await openList(page)

  // Keyboard: the order-number link is a real link, so it is reachable and activatable without a mouse.
  const firstLink = page.getByRole('table').getByRole('link').first()
  const orderNumber = await firstLink.innerText()
  await firstLink.focus()
  await expect(firstLink).toBeFocused()
  await page.keyboard.press('Enter')
  await expect(page).toHaveURL(/\/production-orders\/[0-9a-f-]{36}$/)
  await expect(page.getByRole('heading', { level: 1 })).toHaveText(`Production order ${orderNumber}`)

  // Cancel comes back to the list (the Screen A transition WI-003 changes).
  await page.getByRole('button', { name: 'Cancel' }).click()
  await expect(page).toHaveURL(/\/production-orders$/)

  // Mouse: clicking the row body resolves to the same link.
  await page.getByRole('table').getByRole('row').nth(1).getByRole('cell').nth(1).click()
  await expect(page).toHaveURL(/\/production-orders\/[0-9a-f-]{36}$/)

  await page.goto('/production-orders')
  await page.getByRole('link', { name: '+ New production order' }).click()
  await expect(page).toHaveURL(/\/production-orders\/new$/)
})

test('the list exposes no way to change an order (DEC-003)', async ({ page }) => {
  await openList(page)

  await expect(page.getByRole('button', { name: /^Save$/ })).toHaveCount(0)
  await expect(page.getByRole('button', { name: /Delete/ })).toHaveCount(0)
  await expect(page.getByRole('table').getByRole('combobox')).toHaveCount(0)
})

test('a signed-out visitor is sent to the login screen (TC-114)', async ({ page, context }) => {
  await context.clearCookies()
  await page.goto('/production-orders')

  await expect(page).toHaveURL(/\/login$/)
})
