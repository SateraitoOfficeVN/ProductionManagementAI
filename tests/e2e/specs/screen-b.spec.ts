import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, signIn } from './helpers'

// Screen B (SCR-002) user journeys — 002_DD E-level test viewpoints (TC-101, TC-109, TC-113, TC-114, TC-115, TC-116).
// The stack carries the 124 seeded demo orders (002_DB, 003_DB); assertions use counts and relative dates, never absolute ones,
// because the seed's due dates follow the migration's run date (DEC-011).

const summary = /^\d+件中 \d+〜\d+件$/

test.beforeEach(async ({ page }) => {
  await signIn(page)
})

async function openList(page: import('@playwright/test').Page, query = '') {
  await page.goto(`/production-orders${query}`)
  await expect(page.getByRole('heading', { level: 1, name: '製造指示一覧' })).toBeVisible()
}

test('opens from the navbar with the default view, and is accessible (TC-101)', async ({ page }) => {
  await page.getByRole('navigation', { name: 'メインメニュー' }).getByRole('link', { name: '製造指示一覧' }).click()

  await expect(page).toHaveURL(/\/production-orders$/)
  await expect(page).toHaveTitle('製造指示一覧 — ProductionManagementAI')
  await expect(page.getByRole('status').first()).toHaveText(summary)

  const rows = page.getByRole('table').getByRole('row')
  await expect(rows).toHaveCount(21) // header + the default page size of 20

  // Default sort: due date ascending, marked on that column only.
  const sorted = page.getByRole('columnheader', { name: /納期/ })
  await expect(sorted).toHaveAttribute('aria-sort', 'ascending')
  await expect(page.getByRole('columnheader').filter({ has: page.locator('[aria-sort]') })).toHaveCount(0)

  // No filter is pre-applied, so terminal statuses appear too (DEC-006).
  await page.getByLabel('表示件数').selectOption('100')
  await expect(page.getByRole('table').getByText('完了').first()).toBeVisible()
  await expect(page.getByRole('table').getByText('取消').first()).toBeVisible()

  await expectNoAxeViolations(page)
})

test('sorting reorders the whole result set and toggles direction (TC-109)', async ({ page }) => {
  await openList(page)

  // Synchronize on the rendered state, not on the URL: the URL changes first and the query follows, so reading the
  // rows right after toHaveURL can still see the previous response. aria-sort comes from the response the table was
  // rendered from, which makes it the honest "this page now shows that sort" signal.
  const orderNumberHeader = page.getByRole('columnheader', { name: /指示番号/ })

  await page.getByRole('button', { name: /指示番号/ }).click()
  await expect(orderNumberHeader).toHaveAttribute('aria-sort', 'ascending')
  await expect(page).toHaveURL(/sort=orderNumber/)
  const ascending = await page.getByRole('table').getByRole('link').allInnerTexts()
  expect(ascending).toEqual([...ascending].sort())

  await page.getByRole('button', { name: /指示番号/ }).click()
  await expect(orderNumberHeader).toHaveAttribute('aria-sort', 'descending')
  await expect(page).toHaveURL(/dir=desc/)
  const descending = await page.getByRole('table').getByRole('link').allInnerTexts()
  expect(descending).toEqual([...descending].sort().reverse())
  // Descending page 1 is not ascending page 1, i.e. the sort reached the database, not just this page.
  expect(descending[0]).not.toEqual(ascending[0])

  await expectNoAxeViolations(page)
})

test('filtering, paging and page size keep the view in the URL (TC-115, TC-116)', async ({ page }) => {
  await openList(page)

  await page.getByLabel('下書き').check()
  await page.getByRole('button', { name: '検索' }).click()
  await expect(page).toHaveURL(/status=Draft/)
  // Again: wait for the rendered result, not just the URL. Once no "In progress" badge is left, the filtered
  // response is the one on screen, so the total read below belongs to it.
  await expect(page.getByRole('table').getByText('進行中')).toHaveCount(0)
  await expect(page.getByRole('status').first()).toHaveText(summary)
  const draftTotal = (await page.getByRole('status').first().textContent())!

  // Paging keeps the filter, and the page number is in the URL.
  await page.getByLabel('表示件数').selectOption('10')
  await expect(page).toHaveURL(/pageSize=10/)
  await page.getByRole('button', { name: '次のページ' }).click()
  await expect(page).toHaveURL(/page=2/)
  await expect(page).toHaveURL(/status=Draft/)

  // Reloading the URL reproduces the same view.
  const url = page.url()
  await page.reload()
  await expect(page).toHaveURL(url)
  await expect(page.getByLabel('下書き')).toBeChecked()
  await expect(page.getByText(/^2 \/ \d+ ページ$/)).toBeVisible()

  // Back returns to the previous view.
  await page.goBack()
  await expect(page.getByText(/^1 \/ \d+ ページ$/)).toBeVisible()

  // A filter that matches nothing shows the no-match state, keeping the values, and Clear filters restores the list.
  await page.getByLabel('指示番号').fill('ZZZNOMATCH')
  await page.getByRole('button', { name: '検索' }).click()
  await expect(page.getByText('絞り込みに一致する製造指示はありません。')).toBeVisible()
  await expect(page.getByLabel('指示番号')).toHaveValue('ZZZNOMATCH')
  await expectNoAxeViolations(page)

  await page.getByRole('button', { name: '絞り込みをクリア' }).click()
  await expect(page.getByRole('status').first()).toHaveText(summary)
  // The unfiltered list contains statuses the Draft filter excluded, which is what says the clear took effect. Since
  // WI-004's seed, the oldest due dates (page 1 of the default sort) belong to historical completed orders.
  await expect(page.getByRole('table').getByText('完了').first()).toBeVisible()
  expect(await page.getByRole('status').first().textContent()).not.toEqual(draftTotal)
})

test('an invalid filter is rejected inline and leaves the rows alone (V-10)', async ({ page }) => {
  await openList(page)
  const firstOrder = await page.getByRole('table').getByRole('link').first().innerText()

  await page.getByLabel('納期（開始）').fill('2026-12-31')
  await page.getByLabel('納期（終了）').fill('2026-01-01')
  await page.getByRole('button', { name: '検索' }).click()

  await expect(page.getByText('納期（開始）は納期（終了）以前の日付にしてください。')).toBeVisible()
  await expect(page.getByLabel('納期（開始）')).toBeFocused()
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
  await expect(page.getByRole('heading', { level: 1 })).toHaveText(`製造指示 ${orderNumber}`)

  // Cancel comes back to the list (the Screen A transition WI-003 changes).
  await page.getByRole('button', { name: 'キャンセル' }).click()
  await expect(page).toHaveURL(/\/production-orders$/)

  // Mouse: clicking the row body resolves to the same link.
  await page.getByRole('table').getByRole('row').nth(1).getByRole('cell').nth(1).click()
  await expect(page).toHaveURL(/\/production-orders\/[0-9a-f-]{36}$/)

  await page.goto('/production-orders')
  await page.getByRole('link', { name: '+ 新規製造指示' }).click()
  await expect(page).toHaveURL(/\/production-orders\/new$/)
})

test('the list exposes no way to change an order (DEC-003)', async ({ page }) => {
  await openList(page)

  await expect(page.getByRole('button', { name: /^保存$/ })).toHaveCount(0)
  await expect(page.getByRole('button', { name: /削除/ })).toHaveCount(0)
  await expect(page.getByRole('table').getByRole('combobox')).toHaveCount(0)
})

test('a signed-out visitor is sent to the login screen (TC-114)', async ({ page, context }) => {
  await context.clearCookies()
  await page.goto('/production-orders')

  await expect(page).toHaveURL(/\/login$/)
})
