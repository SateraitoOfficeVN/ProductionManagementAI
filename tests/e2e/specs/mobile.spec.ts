import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, openCreateForm, signIn } from './helpers'


// 001_BD §1 SP layout (< 640px): single column, full-width buttons with Save above Cancel, back link breadcrumb.

test('SP layout stacks fields and puts Save above Cancel', async ({ page }) => {
  await signIn(page)
  await openCreateForm(page)

  await expect(page.getByRole('link', { name: '‹ 製造指示一覧' })).toBeVisible()

  const save = await page.getByRole('button', { name: '保存' }).boundingBox()
  const cancel = await page.getByRole('button', { name: 'キャンセル' }).boundingBox()
  const quantity = await page.getByLabel('数量').boundingBox()
  const due = await page.getByLabel('納期').boundingBox()
  const viewport = page.viewportSize()!

  expect(save!.y).toBeLessThan(cancel!.y) // Save above Cancel
  expect(save!.width).toBeGreaterThan(viewport.width * 0.8) // full width
  expect(due!.y).toBeGreaterThan(quantity!.y) // quantity and due date stacked, not side by side
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)
  await expectNoAxeViolations(page)
})

// 002_BD §1 SP layout (< 640px): the results table becomes one card per order, each card being the link to Screen A.

test('SP layout shows the order list as cards, not a table', async ({ page }) => {
  await signIn(page)
  await page.goto('/production-orders')
  await expect(page.getByRole('heading', { level: 1, name: '製造指示一覧' })).toBeVisible()

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

// 003_BD SP layout (TC-220, TC-222): the navbar sits behind Menu; tiles two per row; charts scroll inside their card.

test('SP dashboard: menu navbar, two tiles per row, charts scroll in their card, never the page', async ({ page }) => {
  await signIn(page)

  const menu = page.getByRole('button', { name: 'メニュー' })
  await expect(menu).toHaveAttribute('aria-expanded', 'false')
  await expect(page.getByRole('navigation', { name: 'メインメニュー' })).toBeHidden()

  const tiles = page.getByRole('list', { name: 'ステータス別の製造指示' }).getByRole('listitem')
  const first = await tiles.nth(0).boundingBox()
  const second = await tiles.nth(1).boundingBox()
  const third = await tiles.nth(2).boundingBox()
  expect(Math.abs(first!.y - second!.y)).toBeLessThan(2) // side by side
  expect(third!.y).toBeGreaterThan(first!.y) // then the next row

  const scroller = page.getByLabel('納期週別の未完了作業量（スクロール可能）')
  expect(await scroller.evaluate((el) => el.scrollWidth > el.clientWidth)).toBe(true)
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)

  await menu.click()
  await expect(page.getByRole('button', { name: '閉じる' })).toHaveAttribute('aria-expanded', 'true')
  await page.getByRole('navigation', { name: 'メインメニュー' }).getByRole('link', { name: '製造指示一覧' }).click()
  await expect(page.getByRole('heading', { level: 1, name: '製造指示一覧' })).toBeVisible()
  await expectNoAxeViolations(page)
})

