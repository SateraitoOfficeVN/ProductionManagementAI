import { test, expect, type Locator, type Page } from '@playwright/test'
import { signIn, futureDate, expectNoAxeViolations } from './helpers'

/** Measure rendered controls; CSS-class or jsdom assertions cannot detect stretching. */
async function expectActionHeights(page: Page) {
  const actions = await page.locator('main button, dialog[open] button').evaluateAll(buttons =>
    buttons.filter(button => button.getBoundingClientRect().width > 0 &&
      !/^\d{4}-\d{2}-\d{2} /.test(button.getAttribute('aria-label') ?? '')).map(button => {
      const range = document.createRange()
      range.selectNodeContents(button)
      const lines = new Set(Array.from(range.getClientRects()).filter(r => r.height > 0).map(r => Math.round(r.top)))
      return { text: button.textContent, height: button.getBoundingClientRect().height, lines: lines.size,
        horizontalOverflow: button.scrollWidth - button.clientWidth, verticalOverflow: button.scrollHeight - button.clientHeight }
    }))
  expect(actions.length).toBeGreaterThanOrEqual(2)
  for (const action of actions) {
    expect(action.height, `${action.text}: touch target`).toBeGreaterThanOrEqual(48)
    expect(action.horizontalOverflow, `${action.text}: unclipped width`).toBeLessThanOrEqual(1)
    expect(action.verticalOverflow, `${action.text}: unclipped height`).toBeLessThanOrEqual(1)
    if (action.lines === 1) expect(Math.abs(action.height - 48), `${action.text}: single-line height`).toBeLessThanOrEqual(1)
  }
}
async function expectVerticalGap(upper: Locator, lower: Locator) {
  const a = await upper.boundingBox(), b = await lower.boundingBox()
  expect(a).not.toBeNull(); expect(b).not.toBeNull()
  if (!a || !b) throw new Error('Expected visible controls')
  expect(b.y - a.y - a.height).toBeGreaterThanOrEqual(8)
}
async function expectReflow(page: Page) {
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true)
}

for (const width of [1440, 540, 320]) {
  test(`TC-407/408: command heights and capacity spacing at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    await signIn(page)
    await page.goto('/plant-calendar')
    const panel = page.getByRole('region', { name: '参考能力', exact: true })
    await expect(panel.getByRole('button', { name: '能力を表示', exact: true })).toBeVisible()
    await expectActionHeights(page)
    for (const index of [0, 1]) {
      await expectVerticalGap(panel.getByRole('textbox').nth(index), panel.getByRole('button', { name: '検索', exact: true }).nth(index))
    }
    await expectVerticalGap(panel.getByLabel('日付', { exact: true }), panel.getByRole('button', { name: '能力を表示', exact: true }))
    await expectReflow(page)
    await expectNoAxeViolations(page)
  })

  test(`TC-407/408/409: weekly and retained history action groups at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    await signIn(page)
    const date = futureDate(27)
    await page.goto(`/plant-calendar?month=${date.slice(0, 7)}&date=${date}&mode=weekly`)
    const restart = page.getByRole('button', { name: '最初のページから再読込', exact: true })
    await expect(restart).toBeVisible()
    await expect(page.getByRole('heading', { name: `編集: ${date}`, exact: true })).toBeVisible()
    await expectActionHeights(page)
    await expectVerticalGap(page.getByRole('button', { name: '前へ', exact: true }), restart)
    await expectVerticalGap(page.getByRole('button', { name: '表示', exact: true }), page.getByRole('combobox', { name: '履歴', exact: true }))
    await page.goto(`/plant-calendar?month=${date.slice(0, 7)}&date=${date}&mode=history`)
    await expect(restart).toBeVisible()
    await expectActionHeights(page)
    await expectVerticalGap(page.getByRole('button', { name: 'キャンセル', exact: true }), page.getByRole('button', { name: '履歴', exact: true }))
    await expectVerticalGap(page.getByRole('button', { name: '前へ', exact: true }), restart)
    await expectReflow(page)
    await expectNoAxeViolations(page)
  })

  test(`TC-408/409: Unknown recovery and error actions at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    await signIn(page)
    const date = futureDate(28)
    await page.goto(`/plant-calendar?month=${date.slice(0, 7)}&date=${date}&mode=day`)
    await page.getByRole('combobox', { name: '日付例外', exact: true }).selectOption('no')
    await page.getByLabel('理由', { exact: true }).fill('レイアウト検証')
    await page.route('**/api/plant-calendar/exceptions', route => route.abort('failed'))
    await page.getByRole('button', { name: '保存', exact: true }).click()
    const verify = page.getByRole('button', { name: '現在の状態を確認', exact: true })
    await expect(verify).toBeVisible()
    await expect(page.getByRole('button', { name: '保存', exact: true })).toBeDisabled()
    await expectActionHeights(page)
    await expectVerticalGap(page.getByRole('button', { name: 'キャンセル', exact: true }), verify)
    await expectVerticalGap(page.getByRole('button', { name: '入力を破棄して現在の状態を使用', exact: true }), page.getByRole('button', { name: '履歴', exact: true }))
    await expectReflow(page)
    await expectNoAxeViolations(page)
    await page.unroute('**/api/plant-calendar/exceptions')
    await page.route('**/api/plant-calendar/day?*', route => route.fulfill({ status: 503, contentType: 'application/problem+json', body: JSON.stringify({ code: 'CALENDAR_BUSY' }) }))
    await page.goto(`/plant-calendar?month=${date.slice(0, 7)}&date=${date}&mode=day`)
    await expect(page.getByRole('button', { name: '再読込', exact: true })).toBeVisible()
    await expectActionHeights(page)
    await expectVerticalGap(page.getByRole('alert'), page.getByRole('button', { name: '再読込', exact: true }))
    await expectReflow(page)
  })
}

for (const width of [1440, 320]) {
  test(`TC-408/409: weekly Unknown recovery wraps without clipping at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 })
    await signIn(page)
    const date = futureDate(29)
    await page.goto(`/plant-calendar?month=${date.slice(0, 7)}&date=${date}&mode=weekly`)
    await expect(page.getByRole('heading', { name: `編集: ${date}`, exact: true })).toBeVisible()
    await page.getByRole('checkbox', { name: '土', exact: true }).check()
    await page.route('**/api/plant-calendar/weekly/*', route => route.abort('failed'))
    await page.getByRole('button', { name: '保存', exact: true }).click()
    const verify = page.getByRole('button', { name: '現在の状態を確認', exact: true })
    await expect(verify).toBeVisible()
    await expect(page.getByRole('button', { name: '保存', exact: true })).toBeDisabled()
    await expectActionHeights(page)
    await expectVerticalGap(page.getByRole('button', { name: 'キャンセル', exact: true }), verify)
    await expectReflow(page)
    await expectNoAxeViolations(page)
  })
}
