import { expect, test, type Page } from '@playwright/test'
import { expectNoAxeViolations, signIn } from './helpers'

// WI-017 product type (004_DD-TYPE / 004_DD-SPD-TYPE): create, list column, filter kept in the URL, edit.

test.beforeEach(async ({ page }) => { await signIn(page) })

async function createProduct(page: Page, sku: string, type: string) {
  const response = await page.request.post('/api/product-master', { data: { sku, name: `種別E2E ${sku}`, type, unit: '個' } })
  expect(response.status()).toBe(201)
  return await response.json() as { id: string; sku: string }
}

test('create with a type, see it in the list, and require it on save', async ({ page }) => {
  const sku = `TYPE-${Date.now()}`
  await page.goto('/products/new')
  await expect(page.getByRole('heading', { level: 1, name: '製品登録' })).toBeFocused()
  await page.getByLabel(/製品コード/).fill(sku)
  await page.getByLabel(/製品名/).fill('種別必須確認')
  await page.getByLabel(/単位/).selectOption('個')
  await page.getByRole('button', { name: '保存' }).click()
  const typeSelect = page.getByLabel(/種別/)
  await expect(typeSelect).toBeFocused()
  await expect(page.getByText('種別を選択してください。')).toBeVisible()
  await expectNoAxeViolations(page)

  await typeSelect.selectOption({ label: '原材料' })
  await page.getByRole('button', { name: '保存' }).click()
  await expect(page.getByRole('status')).toContainText('製品を登録しました。')
  await page.getByLabel('製品コード・製品名で検索').fill(sku)
  await page.getByRole('button', { name: '検索', exact: true }).click()
  const row = page.getByRole('row', { name: new RegExp(sku) })
  await expect(row.getByRole('cell', { name: '原材料' })).toBeVisible()
})

test('filter by type keeps it through paging and return after save', async ({ page }) => {
  const prefix = `TF${Date.now()}`
  const parts = []
  for (let i = 0; i < 11; i++) parts.push(await createProduct(page, `${prefix}-${String(i).padStart(2, '0')}`, 'PART'))
  await createProduct(page, `${prefix}-RAW`, 'RAW_MATERIAL')

  await page.goto(`/products?q=${prefix}&pageSize=10`)
  await page.getByLabel('種別', { exact: true }).selectOption({ label: '部品' })
  await page.getByRole('button', { name: '検索', exact: true }).click()
  await expect(page).toHaveURL(/type=PART/)
  await expect(page.getByText('11件', { exact: true })).toBeVisible()
  await expect(page.getByRole('table').getByRole('cell', { name: '原材料' })).toHaveCount(0)
  await expectNoAxeViolations(page)

  await page.getByRole('button', { name: '次へ' }).click()
  await expect(page).toHaveURL(/type=PART/)
  await expect(page).toHaveURL(/page=2/)

  await page.getByRole('link', { name: `${parts[10].sku}を編集` }).click()
  // WI-018 BUG-010: until the edit page renders, /種別/ also matches the list's type filter (value PART), so wait for it.
  await expect(page.getByRole('heading', { level: 1, name: '製品編集' })).toBeVisible()
  const typeSelect = page.getByLabel(/種別/)
  await expect(typeSelect).toHaveValue('PART')
  await typeSelect.selectOption({ label: '半製品' })
  await expect(typeSelect).toHaveValue('SEMI_FINISHED')
  const saved = page.waitForResponse((r) => r.request().method() === 'PUT' && r.url().includes('/api/product-master/'))
  await page.getByRole('button', { name: '保存' }).click()
  expect((await (await saved).json()).type).toBe('SEMI_FINISHED')
  await expect(page).toHaveURL(/type=PART/)
  await expect(page.getByRole('status')).toContainText('製品を保存しました。現在の検索条件では表示されません。')
  await expect(page.getByLabel('種別', { exact: true })).toHaveValue('PART')

  await page.getByRole('button', { name: 'クリア' }).click()
  await expect(page).not.toHaveURL(/type=/)
  await expect(page.getByLabel('種別', { exact: true })).toHaveValue('')
})

test('an unknown type in the URL is dropped', async ({ page }) => {
  await page.goto('/products?type=part')
  await expect(page.getByRole('table')).toBeVisible()
  await expect(page).not.toHaveURL(/type=/)
})
