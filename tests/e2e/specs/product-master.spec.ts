import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, futureDate, signIn } from './helpers'

test.beforeEach(async ({ page }) => { await signIn(page) })

test('create, search, reference, edit and retire a product with accessible controls', async ({ page }) => {
  const sku = `T-${Date.now()}`
  await page.goto('/products')
  await expect(page.getByRole('heading', { level: 1, name: '製品マスタ' })).toBeVisible()
  await expectNoAxeViolations(page)

  await page.getByRole('link', { name: '新規製品' }).click()
  // Wait for the form's mount focus effect before inserting text in its first field.
  await expect(page.getByRole('heading', { level: 1, name: '新規製品' })).toBeFocused()
  await page.getByLabel(/製品コード/).fill(` ${sku} `)
  await page.getByLabel(/製品名/).fill('試作部品')
  await page.getByLabel(/単位/).selectOption('kg')
  await page.getByLabel('図面番号').fill(' D-100 ')
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toHaveText('製品を登録しました。')
  await expect(page).toHaveURL(/\/products\/[0-9a-f-]{36}$/)
  const productUrl = page.url()
  await expect(page.getByLabel(/製品コード/)).toHaveValue(sku)
  await expect(page.getByLabel('図面番号')).toHaveValue('D-100')
  await expectNoAxeViolations(page)

  await page.goto('/production-orders/new')
  await expect(page.getByRole('button', { name: '保存' })).toBeVisible()
  await page.locator('#productId').selectOption({ label: `${sku} — 試作部品` })
  await page.locator('#quantity').fill('1.234')
  await page.getByLabel('納期').fill(futureDate())
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toContainText('登録しました。')

  await page.goto(productUrl)
  await expect(page.getByLabel(/単位/)).toHaveAttribute('readonly', '')
  await expect(page.getByText('製造指示で使用されているため、単位は変更できません。')).toBeVisible()
  await page.getByLabel(/製品名/).fill('改訂試作部品')
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toHaveText('製品を保存しました。')

  await page.goto('/products')
  await page.getByLabel('製品コード・製品名').fill(sku)
  await page.getByRole('button', { name: '検索' }).click()
  const row = page.getByRole('row').filter({ hasText: sku })
  await expect(row).toContainText('改訂試作部品')
  await row.getByRole('button', { name: `${sku}を使用停止` }).click()
  const dialog = page.getByRole('dialog', { name: 'この製品を使用停止にしますか？' })
  await expect(dialog).toBeVisible()
  await expect(dialog.getByRole('button', { name: 'キャンセル' })).toBeFocused()
  await expectNoAxeViolations(page)
  await dialog.getByRole('button', { name: 'キャンセル' }).click()
  await expect(dialog).toBeHidden()
  await expect(row.getByRole('button', { name: `${sku}を使用停止` })).toBeFocused()
  await row.getByRole('button', { name: `${sku}を使用停止` }).click()
  await dialog.getByRole('button', { name: '使用停止' }).click()
  await expect(page.getByText('製品を使用停止にしました。')).toBeVisible()
  await expect(row).toContainText('使用停止')
})

test('stale save retains draft and reload asks before discarding it', async ({ page }) => {
  const response = await page.request.post('/api/product-master', { data: {
    sku: `STALE-${Date.now()}`, name: '初期製品', unit: 'm', drawingNumber: null,
  } })
  expect(response.status()).toBe(201)
  const original = await response.json()
  await page.goto(`/products/${original.id}`)
  await expect(page.getByLabel(/製品名/)).toHaveValue('初期製品')
  await page.getByLabel(/製品名/).fill('編集中')
  const competing = await page.request.put(`/api/product-master/${original.id}`, { data: {
    sku: original.sku, name: '他の更新', unit: 'm', drawingNumber: null, version: original.version,
  } })
  expect(competing.status()).toBe(200)
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toContainText('製品が変更されました。')
  await expect(page.getByLabel(/製品名/)).toHaveValue('編集中')
  await page.getByRole('button', { name: '再読み込み' }).click()
  const dialog = page.getByRole('dialog')
  await expect(dialog).toBeVisible()
  await expectNoAxeViolations(page)
  await page.keyboard.press('Escape')
  await expect(dialog).toBeHidden()
  await expect(page.getByLabel(/製品名/)).toHaveValue('編集中')
  await page.getByRole('button', { name: '再読み込み' }).click()
  await dialog.getByRole('button', { name: '破棄', exact: true }).click()
  await expect(page.getByLabel(/製品名/)).toHaveValue('他の更新')
})

test('uncertain create never replays the write and offers catalog lookup', async ({ page }) => {
  const sku = `UNKNOWN-${Date.now()}`
  let writes = 0
  await page.route('**/api/product-master', async (route) => {
    if (route.request().method() !== 'POST') { await route.continue(); return }
    writes++
    const committed = await route.fetch()
    expect(committed.status()).toBe(201)
    await route.fulfill({ status: 503, contentType: 'application/problem+json', body: '{}' })
  })
  await page.goto('/products/new')
  await page.getByLabel(/製品コード/).fill(sku)
  await page.getByLabel(/製品名/).fill('結果確認部品')
  await page.getByLabel(/単位/).selectOption('m')
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toContainText('処理結果を確認できません。')
  await expect(page.getByLabel(/製品コード/)).toHaveValue(sku)
  expect(writes).toBe(1)
  await page.getByRole('link', { name: '製品コード・製品名' }).click()
  await page.getByRole('dialog').getByRole('button', { name: '破棄', exact: true }).click()
  await expect(page.getByRole('row').filter({ hasText: sku })).toBeVisible()
  expect(writes).toBe(1)
})
