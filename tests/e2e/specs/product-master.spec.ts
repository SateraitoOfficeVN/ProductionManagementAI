import { expect, test } from '@playwright/test'
import { expectNoAxeViolations, futureDate, signIn } from './helpers'

test.beforeEach(async ({ page }) => { await signIn(page) })

test('create, search, reference, edit and retire a product with accessible controls', async ({ page }) => {
  const sku = `T-${Date.now()}`
  await page.goto('/products')
  await expect(page.getByRole('heading', { level: 1, name: '製品マスタ' })).toBeVisible()
  await expectNoAxeViolations(page)

  await page.getByRole('link', { name: '製品を登録' }).click()
  // Wait for the form's mount focus effect before inserting text in its first field.
  await expect(page.getByRole('heading', { level: 1, name: '製品登録' })).toBeFocused()
  await page.getByLabel(/製品コード/).fill(` ${sku} `)
  await page.getByLabel(/製品名/).fill('試作部品')
  await page.getByLabel(/単位/).selectOption('kg')
  await page.getByLabel(/図面番号/).fill(' D-100 ')
  await page.getByRole('button', { name: '保存' }).click()
  // WI-013: a save returns to the list it came from and announces itself there.
  await expect(page).toHaveURL(/\/products\?/)
  await expect(page.getByRole('status')).toContainText('製品を登録しました。')
  await page.getByLabel('製品コード・製品名で検索').fill(sku)
  await page.getByRole('button', { name: '検索', exact: true }).click()
  const editLink = page.getByRole('link', { name: `${sku}を編集` }).first()
  await expect(editLink).toHaveAttribute('href', /\/products\/[0-9a-f-]{36}\/edit$/)
  const productUrl = (await editLink.getAttribute('href'))!
  await page.goto(productUrl)
  await expect(page.getByLabel(/製品コード/)).toHaveValue(sku)
  await expect(page.getByLabel(/図面番号/)).toHaveValue('D-100')
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
  await expect(page).toHaveURL(/\/products(\?.*)?$/)
  await expect(page.getByRole('status')).toContainText('製品を保存しました。')

  await page.goto('/products')
  await page.getByLabel('製品コード・製品名で検索').fill(sku)
  await page.getByRole('button', { name: '検索' }).click()
  const row = page.getByRole('row').filter({ hasText: sku })
  await expect(row).toContainText('改訂試作部品')
  await row.getByRole('button', { name: `${sku}を使用停止` }).click()
  const dialog = page.getByRole('dialog', { name: '製品を使用停止にしますか？' })
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
  await page.goto(`/products/${original.id}/edit`)
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
  await page.getByRole('link', { name: '製品一覧で確認' }).click()
  await page.getByRole('dialog').getByRole('button', { name: '破棄', exact: true }).click()
  await expect(page.getByRole('row').filter({ hasText: sku })).toBeVisible()
  expect(writes).toBe(1)
})

// WI-013 DEC-006/DEC-007.
test('product controls match production-order heights and rows per page is selectable', async ({ page }) => {
  const height = async (locator: import('@playwright/test').Locator) => (await locator.boundingBox())!.height
  await page.goto('/production-orders')
  const orders = page.getByRole('main')
  await expect(orders.getByRole('button', { name: '前のページ' })).toBeVisible()
  const orderAdd = await height(orders.getByRole('link', { name: '+ 新規製造指示' }))
  const orderSearch = await height(orders.locator('form button[type=submit]'))
  const orderOutlined = await height(orders.getByRole('button', { name: 'クリア', exact: true }))
  const orderPager = await height(orders.getByRole('button', { name: '前のページ' }))

  await page.goto('/products')
  const products = page.getByRole('main')
  await expect(products.getByRole('button', { name: '前へ', exact: true })).toBeVisible()
  expect(await height(products.getByRole('button', { name: '検索', exact: true }))).toBeCloseTo(orderSearch, 0)
  expect(await height(products.getByRole('link', { name: '製品を登録' }))).toBeCloseTo(orderAdd, 0)
  expect(await height(products.getByRole('button', { name: 'クリア', exact: true }))).toBeCloseTo(orderOutlined, 0)
  expect(await height(products.getByRole('link', { name: 'P-1001を編集' }))).toBeCloseTo(orderOutlined, 0)
  expect(await height(products.getByRole('button', { name: '前へ', exact: true }))).toBeCloseTo(orderPager, 0)

  // DEC-010: count top-left and rows per page top-right, both above the table.
  const count = (await products.getByText(/^\d+件$/).boundingBox())!
  const size = (await page.getByLabel('表示件数').boundingBox())!
  const table = (await page.getByRole('table', { name: '製品一覧' }).boundingBox())!
  expect(count.y + count.height).toBeLessThanOrEqual(table.y)
  expect(size.y + size.height).toBeLessThanOrEqual(table.y)
  expect(count.x).toBeCloseTo(table.x, -1)
  expect(size.x + size.width).toBeCloseTo(table.x + table.width, -1)

  await page.getByLabel('表示件数').selectOption('10')
  await expect(page).toHaveURL(/page=1&pageSize=10/)
  await expect(page.getByRole('table', { name: '製品一覧' }).locator('tbody tr')).toHaveCount(10)
  await page.getByRole('button', { name: '次へ', exact: true }).click()
  await expect(page).toHaveURL(/page=2&pageSize=10/)
  await expect(page.getByText(/^2 \/ \d+$/)).toBeVisible()
  await page.getByRole('button', { name: 'クリア', exact: true }).click()
  await expect(page).toHaveURL(/pageSize=10/)
  await expectNoAxeViolations(page)
})

test('old product links redirect to the designed edit route', async ({ page }) => {
  await page.goto('/products')
  const href = (await page.getByRole('link', { name: 'P-1001を編集' }).first().getAttribute('href'))!
  const id = href.split('/')[2]
  await page.goto(`/products/${id}`)
  await expect(page).toHaveURL(new RegExp(`/products/${id}/edit$`))
  await expect(page.getByRole('heading', { level: 1, name: '製品編集' })).toBeVisible()
  await expect(page.getByText('（変更不可）').first()).toBeVisible()
})

// WI-013 DEC-012: a long product name must not wrap the PC headers, status badge or row actions.
test('long product names leave headers, status and actions on one line', async ({ page }) => {
  const sku = `LONG-${Date.now()}`
  const response = await page.request.post('/api/product-master', { data: {
    sku, name: 'B155 レーザーマーキング #4 '.repeat(8).trim(), unit: '個', drawingNumber: null,
  } })
  expect(response.status()).toBe(201)
  await page.goto(`/products?q=${sku}`)
  const table = page.getByRole('table', { name: '製品一覧' })
  const row = table.getByRole('row').filter({ hasText: sku })
  await expect(row).toBeVisible()
  const lineHeight = await table.evaluate((element) => Number.parseFloat(getComputedStyle(element).lineHeight))
  for (const header of await table.locator('thead th').all()) {
    expect(await header.evaluate((element) => element.clientHeight - Number.parseFloat(getComputedStyle(element).paddingTop) - Number.parseFloat(getComputedStyle(element).paddingBottom))).toBeLessThanOrEqual(lineHeight + 1)
  }
  const status = (await row.getByText('使用中', { exact: true }).boundingBox())!
  expect(status.height).toBeLessThan(2 * lineHeight)
  const edit = (await row.getByRole('link', { name: `${sku}を編集` }).boundingBox())!
  const retire = (await row.getByRole('button', { name: `${sku}を使用停止` }).boundingBox())!
  expect(retire.y).toBeCloseTo(edit.y, 0)
  expect(retire.x).toBeGreaterThan(edit.x + edit.width)
})
