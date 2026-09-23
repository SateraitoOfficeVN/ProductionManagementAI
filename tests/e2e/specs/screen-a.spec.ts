import { expect, test } from '@playwright/test'
import { createOrder, expectNoAxeViolations, futureDate, openCreateForm, productField, signIn } from './helpers'

// Screen A (SCR-001) user journeys — 001_DD E-level test viewpoints.

test.beforeEach(async ({ page }) => {
  await signIn(page)
})

test('create → edit → In progress (locked) → Completed (terminal), accessible at each step', async ({ page }) => {
  await page.getByRole('link', { name: '新規製造指示' }).click()
  await expect(page).toHaveURL(/\/production-orders\/new$/)
  await expect(page.getByText('保存時に採番')).toBeVisible()
  await expectNoAxeViolations(page)

  const { orderNumber } = await createOrder(page, { notes: '2号ライン優先生産' })
  await expect(page).toHaveURL(/\/production-orders\/[0-9a-f-]{36}$/)
  await expect(page.getByRole('heading', { level: 1 })).toHaveText(`製造指示 ${orderNumber}`)
  await expect(page).toHaveTitle(`製造指示 ${orderNumber} — ProductionManagementAI`)

  // Draft → In progress: product/quantity become read-only but stay focusable.
  await page.getByLabel('ステータス').selectOption({ label: '進行中' })
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toHaveText(`製造指示 ${orderNumber} を保存しました。`)
  await expect(page.getByLabel('数量')).toHaveAttribute('readonly', '')
  await expect(productField(page)).toHaveValue('P-1004 — ドライブシャフト')
  await expect(page.getByText('下書き以外の製造指示では変更できません。')).toBeVisible()
  await productField(page).focus()
  await expect(productField(page)).toBeFocused()
  await expect(page.getByLabel('ステータス').locator('option')).toHaveText(['進行中', '完了', '取消'])
  await expectNoAxeViolations(page)

  // In progress → Completed: terminal, the status select is disabled.
  await page.getByLabel('ステータス').selectOption({ label: '完了' })
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toHaveText(`製造指示 ${orderNumber} を保存しました。`)
  await expect(page.getByLabel('ステータス')).toBeDisabled()

  // Reload keeps the saved state (persisted, not just client state).
  await page.reload()
  await expect(page.getByLabel('ステータス')).toHaveValue('Completed')
  await expect(page.getByLabel('備考')).toHaveValue('2号ライン優先生産')
})

test('empty Save shows inline errors, focuses Product, and is accessible', async ({ page }) => {
  await openCreateForm(page)
  await page.getByRole('button', { name: '保存' }).click()

  await expect(page.getByText('製品を選択してください。')).toBeVisible()
  await expect(page.getByText('1以上の整数を入力してください。')).toBeVisible()
  await expect(page.getByText('納期を入力してください。')).toBeVisible()
  await expect(productField(page)).toBeFocused()
  await expectNoAxeViolations(page)

  await page.getByLabel('納期').fill('2000-01-01')
  await page.getByLabel('納期').blur()
  await expect(page.getByText('納期に過去の日付は指定できません。')).toBeVisible()
})

test('Cancel asks before discarding changes; Escape keeps editing; Discard leaves without saving', async ({ page }) => {
  await openCreateForm(page)
  await page.getByLabel('数量').fill('12')
  await page.getByRole('button', { name: 'キャンセル' }).click()

  const dialog = page.getByRole('dialog', { name: '変更を破棄しますか？' })
  await expect(dialog).toBeVisible()
  await expect(dialog.getByRole('button', { name: '編集を続ける' })).toBeFocused()
  await expectNoAxeViolations(page)

  await page.keyboard.press('Escape')
  await expect(dialog).toBeHidden()
  await expect(page.getByLabel('数量')).toHaveValue('12')
  await expect(page.getByRole('button', { name: 'キャンセル' })).toBeFocused()

  await page.getByRole('button', { name: 'キャンセル' }).click()
  await dialog.getByRole('button', { name: '破棄' }).click()
  await expect(page).toHaveURL(/\/production-orders$/)
})

test('Cancel with no changes leaves straight away', async ({ page }) => {
  await openCreateForm(page)
  await page.getByRole('button', { name: 'キャンセル' }).click()
  await expect(page).toHaveURL(/\/production-orders$/)
  await expect(page.getByRole('dialog')).toHaveCount(0)
})

test('two users editing the same order: the second save is rejected and Reload shows the first', async ({ page, browser }) => {
  const { url } = await createOrder(page)

  const otherContext = await browser.newContext()
  const other = await otherContext.newPage()
  await signIn(other)
  await other.goto(url)
  await page.goto(url)

  await other.getByLabel('備考').fill('他のユーザーが保存')
  await other.getByRole('button', { name: '保存' }).click()
  await expect(other.getByRole('status')).toContainText('を保存しました。')

  await page.getByLabel('備考').fill('古い画面からの編集')
  await page.getByRole('button', { name: '保存' }).click()
  const alert = page.getByRole('alert')
  await expect(alert).toContainText('この製造指示は他のユーザーによって変更されました。')
  await expect(page.getByLabel('備考')).toHaveValue('古い画面からの編集')

  await alert.getByRole('button', { name: '再読み込み' }).click()
  await expect(page.getByLabel('備考')).toHaveValue('他のユーザーが保存')
  await otherContext.close()
})

test('unknown order shows the not-found panel', async ({ page }) => {
  await page.goto('/production-orders/00000000-0000-0000-0000-000000000000')
  await expect(page.getByRole('alert')).toHaveText(/この製造指示は存在しません。/)
  await expect(page.getByRole('button', { name: '保存' })).toHaveCount(0)
})

test('the product list shows all 30 seeded products', async ({ page }) => {
  await openCreateForm(page)
  // 30 products + the "Select a product" placeholder.
  await expect(productField(page).locator('option')).toHaveCount(31)
  await expect(page.getByLabel('納期')).toBeEditable()
  expect(futureDate()).toMatch(/^\d{4}-\d{2}-\d{2}$/)
})
