import { expect, test, type Locator, type Page } from '@playwright/test'
import { expectNoAxeViolations, signIn } from './helpers'

async function geometry(page: Page, dialog: Locator, name: string) {
  const box = await dialog.boundingBox()
  const viewport = page.viewportSize()
  if (!box || !viewport) throw new Error('Visible dialog and viewport geometry are required.')
  const observation = { box, viewport, scrollY: await page.evaluate(() => window.scrollY),
    centerXError: Math.abs(box.x + box.width / 2 - viewport.width / 2),
    centerYError: Math.abs(box.y + box.height / 2 - viewport.height / 2) }
  await test.info().attach(name + '-geometry', { body: JSON.stringify(observation, null, 2), contentType: 'application/json' })
  await test.info().attach(name, { body: await page.screenshot(), contentType: 'image/png' })
  expect(observation.centerXError).toBeLessThanOrEqual(2)
  expect(observation.centerYError).toBeLessThanOrEqual(2)
  expect(box.x).toBeGreaterThanOrEqual(16)
  expect(box.y).toBeGreaterThanOrEqual(16)
  expect(box.x + box.width).toBeLessThanOrEqual(viewport.width - 16)
  expect(box.y + box.height).toBeLessThanOrEqual(viewport.height - 16)
}

async function containedFocus(page: Page, dialog: Locator) {
  for (let i = 0; i < 4; i++) {
    await page.keyboard.press('Tab')
    const focus = await dialog.evaluate((element) => ({
      inside: element.contains(document.activeElement), documentFocused: document.hasFocus(),
      activeTag: document.activeElement?.tagName,
    }))
    // Native Chromium may cycle through browser chrome, but never a background page control.
    if (focus.documentFocused) expect(focus.inside).toBe(true)
    else expect(focus.activeTag).toBe('BODY')
  }
}

for (const mode of [
  { name: 'desktop', viewport: { width: 1440, height: 900 }, isMobile: false, hasTouch: false },
  { name: 'mobile', viewport: { width: 412, height: 840 }, isMobile: true, hasTouch: true },
]) {
  test.describe('WI-008 ' + mode.name, () => {
    test.use({ viewport: mode.viewport, isMobile: mode.isMobile, hasTouch: mode.hasTouch })
    test.beforeEach(async ({ page }) => { await signIn(page) })

    test('TC-323/324 retirement dialog is centered after scrolling and preserves cancel/focus', async ({ page }) => {
      let writes = 0
      page.on('request', (request) => {
        if (request.method() === 'POST' && request.url().endsWith('/retire')) writes++
      })
      await page.goto('/products')
      const trigger = page.getByRole('button', { name: 'P-1010を使用停止', exact: true })
      await trigger.scrollIntoViewIfNeeded()
      expect(await page.evaluate(() => window.scrollY)).toBeGreaterThan(0)
      await trigger.click()
      const dialog = page.getByRole('dialog', { name: '製品を使用停止にしますか？' })
      await expect(dialog).toBeVisible()
      await expect(dialog.getByRole('button', { name: 'キャンセル', exact: true })).toBeFocused()
      await geometry(page, dialog, 'retirement')
      await expectNoAxeViolations(page)
      await containedFocus(page, dialog)
      await page.keyboard.press('Escape')
      await expect(dialog).toBeHidden()
      await expect(trigger).toBeFocused()
      await trigger.click()
      await dialog.getByRole('button', { name: 'キャンセル', exact: true }).click()
      await expect(dialog).toBeHidden()
      await expect(trigger).toBeFocused()
      expect(writes).toBe(0)
    })

    test('TC-323/324 discard dialog is centered and keeps draft on Escape', async ({ page }) => {
      let writes = 0
      page.on('request', (request) => {
        if (['POST', 'PUT'].includes(request.method()) && request.url().includes('/api/product-master')) writes++
      })
      await page.goto('/products/new')
      await page.getByLabel(/製品名/).fill('破棄確認用部品')
      const trigger = page.getByRole('button', { name: 'キャンセル', exact: true })
      await trigger.click()
      const dialog = page.getByRole('dialog', { name: '変更を破棄しますか？' })
      await expect(dialog.getByRole('button', { name: '編集を続ける', exact: true })).toBeFocused()
      await geometry(page, dialog, 'discard')
      await expectNoAxeViolations(page)
      await containedFocus(page, dialog)
      await page.keyboard.press('Escape')
      await expect(dialog).toBeHidden()
      await expect(trigger).toBeFocused()
      await expect(page.getByLabel(/製品名/)).toHaveValue('破棄確認用部品')
      await trigger.click()
      await dialog.getByRole('button', { name: '編集を続ける', exact: true }).click()
      await expect(trigger).toBeFocused()
      await trigger.click()
      await dialog.getByRole('button', { name: '破棄', exact: true }).click()
      await expect(page).toHaveURL(/\/products(?:\?.*)?$/)
      expect(writes).toBe(0)
    })

    test('TC-325 navbar distinguishes products from orders with accessible decorative icons', async ({ page }) => {
      await page.goto('/products')
      if (mode.isMobile) await page.getByRole('button', { name: 'メニュー', exact: true }).click()
      const nav = page.getByRole('navigation', { name: 'メインメニュー' })
      const products = nav.getByRole('link', { name: '製品マスタ', exact: true })
      const orders = nav.getByRole('link', { name: '製造指示一覧', exact: true })
      await expect(products).toHaveAttribute('href', '/products')
      await expect(products).toHaveAttribute('aria-current', 'page')
      await expect(orders).toHaveAttribute('href', '/production-orders')
      const productIcon = products.locator('svg')
      const orderIcon = orders.locator('svg')
      await expect(productIcon).toHaveAttribute('aria-hidden', 'true')
      await expect(orderIcon).toHaveAttribute('aria-hidden', 'true')
      await test.info().attach('navbar', { body: await page.screenshot(), contentType: 'image/png' })
      expect(await productIcon.innerHTML()).not.toBe(await orderIcon.innerHTML())
      await expect(productIcon).toHaveClass(/lucide-package/)
      await expect(orderIcon).toHaveClass(/lucide-clipboard-list/)
      await expectNoAxeViolations(page)
      await orders.click()
      await expect(page.getByRole('heading', { level: 1, name: '製造指示一覧' })).toBeVisible()
      if (mode.isMobile) await expect(page.getByRole('button', { name: 'メニュー', exact: true })).toHaveAttribute('aria-expanded', 'false')
    })

    test('TC-324 both dialogs keep actions reachable at 200% zoom and short height', async ({ page }) => {
      await page.setViewportSize({ width: mode.viewport.width, height: 500 })
      await page.goto('/products')
      await page.evaluate(() => { document.documentElement.style.zoom = '2' })
      await page.getByRole('button', { name: 'P-1010を使用停止', exact: true }).click()
      let dialog = page.getByRole('dialog', { name: '製品を使用停止にしますか？' })
      await expect(dialog).toBeVisible()
      await geometry(page, dialog, 'retirement-zoom')
      expect(await dialog.evaluate((element) => element.scrollWidth <= element.clientWidth)).toBe(true)
      await dialog.getByRole('button', { name: 'キャンセル', exact: true }).click()
      await page.goto('/products/new')
      await page.evaluate(() => { document.documentElement.style.zoom = '2' })
      await page.getByLabel(/製品名/).fill('拡大表示の確認')
      await page.getByRole('button', { name: 'キャンセル', exact: true }).click()
      dialog = page.getByRole('dialog', { name: '変更を破棄しますか？' })
      await geometry(page, dialog, 'discard-zoom')
      expect(await dialog.evaluate((element) => element.scrollWidth <= element.clientWidth)).toBe(true)
      await dialog.getByRole('button', { name: '編集を続ける', exact: true }).click()
      await expect(page.getByLabel(/製品名/)).toHaveValue('拡大表示の確認')

    })
  })
}
