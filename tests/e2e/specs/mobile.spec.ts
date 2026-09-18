import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, openCreateForm, signIn } from './helpers'

// BD-001 §1 SP layout (< 640px): single column, full-width buttons with Save above Cancel, back link breadcrumb.

test('SP layout stacks fields and puts Save above Cancel', async ({ page }) => {
  await signIn(page)
  await openCreateForm(page)

  await expect(page.getByRole('link', { name: '‹ Home' })).toBeVisible()

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
