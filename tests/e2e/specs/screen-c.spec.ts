import { expect, test, type Page } from '@playwright/test'
import { expectNoAxeViolations, futureDate, openCreateForm, signIn } from './helpers'

// Screen C (SCR-003) and the shared navbar — 003_DD E-level viewpoints (TC-201, TC-202, TC-216, TC-217, TC-222,
// TC-223, TC-227). The stack carries 003_DB's demo seed; figures depend on the day the seed ran, so assertions are
// structural or relative (tiles sum to the total, workload sums to Draft + In progress), never absolute.

const nav = (page: Page) => page.getByRole('navigation', { name: 'メインメニュー' })

async function tileValue(page: Page, list: string, label: string): Promise<number> {
  const tile = page.getByRole('list', { name: list }).getByRole('listitem').filter({ hasText: label }).first()
  const text = await tile.locator('span.text-2xl').innerText()
  return Number(text.replace(/,/g, ''))
}

test.beforeEach(async ({ page }) => {
  await signIn(page)
})

test('login lands on the dashboard with every widget, healthy status and no axe violations (TC-201, TC-217)', async ({ page }) => {
  await expect(page).toHaveURL(/\/$/)
  await expect(page).toHaveTitle('ダッシュボード — ProductionManagementAI')
  await expect(page.getByText(/^\d{4}\/\d{2}\/\d{2} \d{2}:\d{2} 時点（Asia\/Tokyo）$/)).toBeVisible()
  await expect(nav(page).getByRole('link', { name: 'ダッシュボード' })).toHaveAttribute('aria-current', 'page')

  for (const heading of ['要注意', '未完了数量の多い製品', '納期週別の未完了作業量', '週別の完了件数（直近12週）']) {
    await expect(page.getByRole('heading', { name: heading })).toBeVisible()
  }
  await expect(page.getByRole('status').filter({ hasText: 'データベース：正常' })).toBeVisible()

  const total = await tileValue(page, 'ステータス別の製造指示', '合計')
  const parts = await Promise.all(
    ['下書き', '進行中', '完了', '取消'].map((label) => tileValue(page, 'ステータス別の製造指示', label)),
  )
  expect(total).toBeGreaterThan(0)
  expect(parts.reduce((a, b) => a + b, 0)).toBe(total)

  // The workload bars sum to the active orders (DEC-009, one snapshot): read them from the chart's own table.
  await page.getByRole('button', { name: '表で表示' }).first().click()
  const orders = await page.getByRole('table', { name: '納期週別の未完了作業量' }).locator('tbody tr td:nth-child(3)').allInnerTexts()
  expect(orders).toHaveLength(10)
  expect(orders.map((n) => Number(n.replace(/,/g, ''))).reduce((a, b) => a + b, 0)).toBe(parts[0] + parts[1])

  await expectNoAxeViolations(page)
})

test('widgets are read-only and the navbar reaches every screen with the right current page (TC-202, TC-222)', async ({ page }) => {
  await expect(page.getByRole('main').getByRole('link')).toHaveCount(0)

  await nav(page).getByRole('link', { name: '製造指示一覧' }).click()
  await expect(page.getByRole('heading', { level: 1, name: '製造指示一覧' })).toBeVisible()
  await expect(nav(page).getByRole('link', { name: '製造指示一覧' })).toHaveAttribute('aria-current', 'page')

  await nav(page).getByRole('link', { name: '新規製造指示' }).click()
  await expect(page.getByRole('heading', { level: 1, name: '新規製造指示' })).toBeVisible()
  await expect(nav(page).getByRole('link', { name: '新規製造指示' })).toHaveAttribute('aria-current', 'page')

  await nav(page).getByRole('link', { name: 'ダッシュボード' }).click()
  await expect(page.getByRole('heading', { level: 1, name: 'ダッシュボード' })).toBeVisible()
  await expectNoAxeViolations(page)
})

test('a failed load shows Retry and no figures; Retry recovers (TC-216)', async ({ page }) => {
  let failures = 1
  await page.route('**/api/dashboard', async (route) => {
    if (failures-- > 0) {
      await route.fulfill({ status: 500, contentType: 'application/problem+json', body: '{"status":500,"code":"MSG-E013"}' })
    } else {
      await route.continue()
    }
  })
  await page.goto('/')

  const alert = page.getByRole('alert')
  await expect(alert).toContainText('問題が発生しました。もう一度お試しください。')
  await expect(page.getByRole('list', { name: 'ステータス別の製造指示' })).toHaveCount(0)
  await expect(nav(page)).toBeVisible()

  await alert.getByRole('button', { name: '再試行' }).click()
  await expect(page.getByRole('list', { name: 'ステータス別の製造指示' })).toBeVisible()
})

test('a chart maximizes to a dialog, keeps focus inside, and restores focus on Escape (TC-227)', async ({ page }) => {
  const expand = page.getByRole('button', { name: '週別の完了件数（直近12週）を拡大' })
  let requests = 0
  page.on('request', (r) => { if (r.url().endsWith('/api/dashboard')) requests++ })

  await expand.click()
  const dialog = page.getByRole('dialog', { name: '週別の完了件数（直近12週）' })
  await expect(dialog).toBeVisible()
  await expect(dialog.getByRole('button', { name: '元に戻す' })).toBeFocused()
  await expect(dialog.getByRole('table')).toBeVisible()
  await expect(dialog.getByRole('img', { name: /^週別の完了件数（直近12週）：/ })).toBeVisible()

  await page.keyboard.press('Tab')
  await page.keyboard.press('Tab')
  expect(await dialog.evaluate((d) => d.contains(document.activeElement))).toBe(true)
  await expectNoAxeViolations(page)

  await page.keyboard.press('Escape')
  await expect(dialog).toBeHidden()
  await expect(expand).toBeFocused()
  expect(requests).toBe(0)
})

test('an edited order asks before a navbar link leaves it; Discard follows the link (TC-223)', async ({ page }) => {
  await openCreateForm(page)
  await page.getByLabel('数量').fill('7')
  await page.getByLabel('納期').fill(futureDate())

  await nav(page).getByRole('link', { name: 'ダッシュボード' }).click()
  const dialog = page.getByRole('dialog', { name: '変更を破棄しますか？' })
  await expect(dialog).toBeVisible()
  await dialog.getByRole('button', { name: '編集を続ける' }).click()
  await expect(page).toHaveURL(/\/production-orders\/new$/)
  await expect(page.getByLabel('数量')).toHaveValue('7')

  await nav(page).getByRole('link', { name: 'ダッシュボード' }).click()
  await page.getByRole('dialog', { name: '変更を破棄しますか？' }).getByRole('button', { name: '破棄' }).click()
  await expect(page.getByRole('heading', { level: 1, name: 'ダッシュボード' })).toBeVisible()
})
