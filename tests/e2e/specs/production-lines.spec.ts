import { expect, test, type Page } from '@playwright/test'
import { expectNoAxeViolations, futureDate, productField, signIn } from './helpers'

async function fillLine(page: Page, code: string) {
  await page.getByLabel('ラインコード', { exact: true }).fill(code)
  await page.getByLabel('ライン名', { exact: true }).fill('試験生産ライン')
  await page.getByLabel('稼働時間／日', { exact: true }).fill('7.5')
}
async function centered(page: Page) {
  const geometry = await page.getByRole('dialog').evaluate(element => {
    const bounds = element.getBoundingClientRect()
    return { x: bounds.x, y: bounds.y, width: bounds.width, height: bounds.height, viewportWidth: innerWidth, viewportHeight: innerHeight }
  })
  expect(Math.abs(geometry.x + geometry.width / 2 - geometry.viewportWidth / 2)).toBeLessThanOrEqual(2)
  expect(Math.abs(geometry.y + geometry.height / 2 - geometry.viewportHeight / 2)).toBeLessThanOrEqual(2)
  expect(geometry.x).toBeGreaterThanOrEqual(15)
  expect(geometry.y).toBeGreaterThanOrEqual(15)
}

test.beforeEach(async ({ page }) => { await signIn(page) })

test('TC-360/361/363: create exact product timing, stage pair retirement and persist only on Save', async ({ page }) => {
  const code = `WEB-${Date.now()}`
  await page.goto('/production-lines')
  await page.getByRole('link', { name: '生産ラインを登録' }).click()
  await fillLine(page, code)
  await page.getByLabel('製品を検索', { exact: true }).fill('P-1001')
  await page.getByRole('button', { name: '検索', exact: true }).click()
  await expect(page.getByRole('button', { name: '製品を追加', exact: true })).toHaveCount(1)
  await page.getByRole('button', { name: '製品を追加', exact: true }).click()
  await page.getByLabel('生産時間 (分／1単位)', { exact: true }).fill('0.125')
  await page.getByLabel('表示単位で生産時間を確認しました').check()
  await expectNoAxeViolations(page)
  await page.getByRole('button', { name: '保存', exact: true }).click()
  await expect(page).toHaveURL(/\/production-lines$/)
  await expect(page.getByText('生産ラインを保存しました。')).toBeVisible()
  await page.getByLabel('コード・名称で検索').fill(code)
  await page.getByRole('button', { name: '検索', exact: true }).click()
  await page.getByRole('link', { name: `${code}を編集`, exact: true }).click()
  await expect(page.getByLabel('ラインコード')).toHaveAttribute('readonly', '')
  await expect(page.getByLabel('生産時間 (分／1単位)')).toHaveValue('0.125')
  const id = page.url().split('/').at(-2)
  const retire = page.getByRole('button', { name: '使用停止', exact: true })
  await retire.click()
  await centered(page)
  const modal = page.getByRole('dialog')
  await expect(modal.getByRole('button', { name: 'キャンセル' })).toBeFocused()
  await page.keyboard.press('Tab')
  await expect(modal.getByRole('button', { name: '使用停止', exact: true })).toBeFocused()
  await page.keyboard.press('Tab')
  await expect(modal.getByRole('button', { name: 'キャンセル' })).toBeFocused()
  await page.keyboard.press('Escape')
  await expect(modal).toBeHidden(); await expect(retire).toBeFocused()
  await retire.click(); await modal.getByRole('button', { name: '使用停止', exact: true }).click()
  const before = await (await page.request.get(`/api/production-lines/${id}`)).json()
  expect(before.pairs.items[0].isActive).toBe(true)
  await page.getByRole('button', { name: '保存', exact: true }).click()
  await expect(page).toHaveURL(/\/production-lines$/)
  const after = await (await page.request.get(`/api/production-lines/${id}`)).json()
  expect(after.pairs.items[0].isActive).toBe(false)
})

test('TC-362: Draft start requires eligible selection, assignment locks and list shows current history', async ({ page }) => {
  const productId = '0197e4a0-0000-7000-8000-000000001004'
  const observation = await (await page.request.get(`/api/production-lines/eligible?productId=${productId}`)).json()
  const code = `ORDER-${Date.now()}`
  const create = await page.request.post('/api/production-lines', { data: { code, name: '指示試験ライン', workingHoursPerDay: '8', products: [{ productId, minutesPerUnit: '1.25', expectedUnit: observation.product.unit, expectedUnitRevision: observation.product.unitRevision, confirmUnit: true }] } })
  expect(create.status()).toBe(201)
  const line = await create.json()
  await page.goto('/production-orders/new')
  await productField(page).selectOption(productId)
  await page.getByLabel('数量', { exact: false }).fill('2')
  await page.getByLabel('納期').fill(futureDate())
  await page.getByRole('button', { name: '保存', exact: true }).click()
  await expect(page).toHaveURL(/\/production-orders\/[0-9a-f-]{36}$/)
  await page.getByLabel('ステータス').selectOption('InProgress')
  await page.getByRole('button', { name: '保存', exact: true }).click()
  await expect(page.getByText('生産開始には生産ラインの選択が必要です。')).toBeVisible()
  const picker = page.getByRole('region', { name: '生産ライン', exact: true })
  await picker.getByLabel('コード・名称で検索').fill(code)
  await picker.getByRole('button', { name: '検索', exact: true }).click()
  await picker.getByRole('button', { name: '選択', exact: true }).click()
  await page.getByRole('button', { name: '保存', exact: true }).click()
  await expect(page.getByRole('status')).toContainText('を保存しました。')
  await expect(picker.getByRole('button')).toHaveCount(0)
  const number = (await page.getByRole('heading', { level: 1 }).innerText()).replace('製造指示 ', '')
  await page.request.post(`/api/production-lines/${line.id}/retire`, { data: { version: line.version } })
  await page.reload(); await expect(picker).toContainText('使用停止')
  await page.goto(`/production-orders?orderNumber=${number}`)
  await expect(page.getByRole('columnheader', { name: '生産ライン', exact: true })).toBeVisible()
  await expect(page.getByRole('cell', { name: new RegExp(code) })).toBeVisible()
  expect(await page.getByRole('columnheader', { name: '生産ライン', exact: true }).getByRole('button').count()).toBe(0)
  await expectNoAxeViolations(page)
})

test('TC-364: browser Back preserves dirty values; lost committed response blocks replay and supports read verification', async ({ page }) => {
  await page.goto('/production-lines')
  await page.getByRole('link', { name: '生産ラインを登録' }).click()
  const code = `UNKNOWN-${Date.now()}`
  await fillLine(page, code)
  await page.evaluate(() => history.back())
  const dialog = page.getByRole('dialog')
  await expect(dialog).toBeVisible()
  await dialog.getByRole('button', { name: 'キャンセル' }).click()
  await expect(page.getByLabel('ラインコード')).toHaveValue(code)
  let writes = 0
  await page.route('**/api/production-lines', async route => {
    if (route.request().method() !== 'POST') { await route.continue(); return }
    writes++
    await route.fetch()
    await route.abort('failed')
  })
  await page.getByRole('button', { name: '保存', exact: true }).click()
  await expect(page.getByText('保存結果を確認できません。再送信せず、現在の状態を確認してください。')).toBeVisible()
  await expect(page.getByRole('button', { name: '保存', exact: true })).toBeDisabled()
  await expect(page.getByLabel('ラインコード')).toHaveValue(code)
  expect(writes).toBe(1)
  await page.getByRole('button', { name: '現在の状態を確認', exact: true }).click()
  await expect(page.getByRole('region', { name: '現在の状態を確認', exact: true })).toContainText(code)
  await expect(page.getByRole('button', { name: '保存', exact: true })).toBeDisabled()
  expect(writes).toBe(1)
})


test('TC-364: lost retirement response permits Cancel and read verification without replay', async ({ page }) => {
  const code = `RETIRE-UNKNOWN-${Date.now()}`
  const response = await page.request.post('/api/production-lines', { data: { code, name: '結果確認ライン', workingHoursPerDay: '8', products: [] } })
  expect(response.status()).toBe(201)
  await page.goto(`/production-lines?q=${code}&state=all`)
  let writes = 0
  await page.route('**/api/production-lines/*/retire', async route => { writes++; await route.fetch(); await route.abort('failed') })
  await page.getByRole('button', { name: `${code}を使用停止`, exact: true }).click()
  const dialog = page.getByRole('dialog')
  await dialog.getByRole('button', { name: '使用停止', exact: true }).click()
  await expect(dialog).toContainText('保存結果を確認できません。')
  await expect(dialog.getByRole('button', { name: '使用停止', exact: true })).toBeDisabled()
  await dialog.getByRole('button', { name: 'キャンセル' }).click()
  await expect(dialog).toBeHidden()
  await page.getByRole('button', { name: '現在の状態を確認', exact: true }).click()
  await expect(page.getByRole('row').filter({ has: page.getByRole('rowheader', { name: code }) })).toContainText('使用停止')
  expect(writes).toBe(1)
})

test('TC-335: browser history restores applied line filters and current rows', async ({ page }) => {
  const suffix = Date.now(); const first = `HISTORY-A-${suffix}`; const second = `HISTORY-B-${suffix}`
  for (const code of [first, second]) expect((await page.request.post('/api/production-lines', { data: { code, name: '履歴検索ライン', workingHoursPerDay: '8', products: [] } })).status()).toBe(201)
  await page.goto('/production-lines')
  const search = page.getByLabel('コード・名称で検索')
  await search.fill(first); await page.getByRole('button', { name: '検索', exact: true }).click()
  await expect(page.getByRole('rowheader', { name: first })).toBeVisible()
  await search.fill(second); await page.getByRole('button', { name: '検索', exact: true }).click()
  await expect(page.getByRole('rowheader', { name: second })).toBeVisible()
  await page.goBack()
  await expect(search).toHaveValue(first); await expect(page.getByRole('rowheader', { name: first })).toBeVisible()
  await page.goForward()
  await expect(search).toHaveValue(second); await expect(page.getByRole('rowheader', { name: second })).toBeVisible()
})
