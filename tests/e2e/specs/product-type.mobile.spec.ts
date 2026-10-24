import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, signIn } from './helpers'

// WI-017 (SP, 390 px): product cards show 「種別：…」 and the type filter fits without horizontal scroll.

test('SP cards show the type and the filter fits the screen', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await signIn(page)
  await page.goto('/products?q=P-1006')
  await expect(page.getByText('種別：半製品')).toBeVisible()
  const filter = page.getByLabel('種別', { exact: true })
  await expect(filter).toBeVisible()
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)
  await expectNoAxeViolations(page)
})
