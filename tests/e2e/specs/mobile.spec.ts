import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, openCreateForm, signIn } from './helpers'


// BD-001 §1 SP layout (< 640px): single column, full-width buttons with Save above Cancel, back link breadcrumb.

test('SP layout stacks fields and puts Save above Cancel', async ({ page }) => {
  await signIn(page)
  await openCreateForm(page)

  await expect(page.getByRole('link', { name: '‹ Production orders' })).toBeVisible()

  const save = await page.getByRole('button', { name: 'Save' }).boundingBox()
  const cancel = await page.getByRole('button', { name: 'Cancel' }).boundingBox()
  const quantity = await page.getByLabel('Quantity').boundingBox()
  const due = await page.getByLabel('Due date').boundingBox()
  const viewport = page.viewportSize()!

  expect(save!.y).toBeLessThan(cancel!.y) // Save above Cancel
  expect(save!.width).toBeGreaterThan(viewport.width * 0.8) // full width
  expect(due!.y).toBeGreaterThan(quantity!.y) // quantity and due date stacked, not side by side
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)
  await expectNoAxeViolations(page)
})

// BD-002 §1 SP layout (< 640px): the results table becomes one card per order, each card being the link to Screen A.

test('SP layout shows the order list as cards, not a table', async ({ page }) => {
  await signIn(page)
  await page.goto('/production-orders')
  await expect(page.getByRole('heading', { level: 1, name: 'Production orders' })).toBeVisible()

  await expect(page.getByRole('table')).toBeHidden()
  const cards = page.getByRole('link', { name: /PO-\d{4}-\d{5}/ })
  await expect(cards.first()).toBeVisible()

  const card = await cards.first().boundingBox()
  const viewport = page.viewportSize()!
  expect(card!.width).toBeGreaterThan(viewport.width * 0.8) // full-width card
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)

  await cards.first().click()
  await expect(page).toHaveURL(/\/production-orders\/[0-9a-f-]{36}$/)

  await page.goBack()
  await expectNoAxeViolations(page)
})

// BD-003 SP layout (TC-220, TC-222): the navbar sits behind Menu; tiles two per row; charts scroll inside their card.

test('SP dashboard: menu navbar, two tiles per row, charts scroll in their card, never the page', async ({ page }) => {
  await signIn(page)

  const menu = page.getByRole('button', { name: 'Menu' })
  await expect(menu).toHaveAttribute('aria-expanded', 'false')
  await expect(page.getByRole('navigation', { name: 'Main' })).toBeHidden()

  const tiles = page.getByRole('list', { name: 'Orders by status' }).getByRole('listitem')
  const first = await tiles.nth(0).boundingBox()
  const second = await tiles.nth(1).boundingBox()
  const third = await tiles.nth(2).boundingBox()
  expect(Math.abs(first!.y - second!.y)).toBeLessThan(2) // side by side
  expect(third!.y).toBeGreaterThan(first!.y) // then the next row

  const scroller = page.getByLabel('Open workload by due week, scrollable')
  expect(await scroller.evaluate((el) => el.scrollWidth > el.clientWidth)).toBe(true)
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)

  await menu.click()
  await expect(page.getByRole('button', { name: 'Close' })).toHaveAttribute('aria-expanded', 'true')
  await page.getByRole('navigation', { name: 'Main' }).getByRole('link', { name: 'Production orders' }).click()
  await expect(page.getByRole('heading', { level: 1, name: 'Production orders' })).toBeVisible()
  await expectNoAxeViolations(page)
})

