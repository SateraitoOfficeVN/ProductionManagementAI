import { expect, test, type Locator, type Page } from '@playwright/test'
import { expectNoAxeViolations, signIn } from './helpers'

// WI-015 (005_DD-SPD-REDESIGN version 3): TC-421, TC-422, TC-425, TC-426, TC-430–TC-433. Lists that need many rows
// or long products are served from mocked responses, so no assertion depends on the shared database contents.

const id = (n: number) => `0197e4a0-0000-7000-8000-${n.toString(16).padStart(12, '0')}`
const at = '2026-10-06T00:00:00Z'
const summary = (n: number, extra: Record<string, unknown> = {}) => ({ id: id(0x15000 + n), code: `L-${String(n).padStart(4, '0')}`, name: `組立ライン ${n}`, workingHoursPerDay: '8', isActive: n % 7 !== 0, updatedAt: at, version: '1', ...extra })
const choice = (n: number, sku: string, name: string, unit = 'kg') => ({ id: id(0x16000 + n), sku, name, unit, unitRevision: '1', isActive: true })
const pairOf = (product: ReturnType<typeof choice>, extra: Record<string, unknown> = {}) => ({ product, minutesPerUnit: '1.5', confirmedUnit: product.unit, confirmedUnitRevision: '1', isActive: true, requiresUnitConfirmation: false, updatedAt: at, ...extra })
const longJapanese = 'ブレーキキャリパー組立ライン '.repeat(15).slice(0, 200)
const noSpaces = 'W'.repeat(200)
const height = async (locator: Locator) => (await locator.boundingBox())!.height
const noPageScroll = (page: Page) => page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)
const noBoxScroll = (locator: Locator) => locator.evaluate(element => element.scrollWidth <= element.clientWidth + 1)
async function inViewport(page: Page, locator: Locator) {
  const box = (await locator.boundingBox())!
  const viewport = page.viewportSize()!
  expect(box.x).toBeGreaterThanOrEqual(15); expect(box.y).toBeGreaterThanOrEqual(15)
  expect(box.x + box.width).toBeLessThanOrEqual(viewport.width - 15); expect(box.y + box.height).toBeLessThanOrEqual(viewport.height - 15)
  expect(await noBoxScroll(locator)).toBe(true)
}
async function singleLine(locator: Locator) {
  return locator.evaluate(element => {
    const style = getComputedStyle(element)
    return element.clientHeight - Number.parseFloat(style.paddingTop) - Number.parseFloat(style.paddingBottom) <= Number.parseFloat(style.lineHeight) + 1
  })
}
async function serveLines(page: Page, total: number, make = summary) {
  const sizes: string[] = []
  await page.route(url => url.pathname === '/api/production-lines', async route => {
    if (route.request().method() !== 'GET') { await route.continue(); return }
    const params = new URL(route.request().url()).searchParams
    const size = Number(params.get('pageSize') ?? '50'); const current = Number(params.get('page') ?? '1')
    sizes.push(String(size))
    const first = (current - 1) * size
    const items = Array.from({ length: Math.max(0, Math.min(size, total - first)) }, (_, i) => make(first + i + 1))
    await route.fulfill({ json: { items, total, page: current, pageSize: size } })
  })
  return sizes
}

test.beforeEach(async ({ page }) => { await signIn(page) })

test('TC-421/422/430: 「表示件数」 in the URL, scroll box with sticky header, pager outside the box', async ({ page }) => {
  const sizes = await serveLines(page, 240)
  await page.goto('/production-lines?state=all')
  await expect(page).toHaveURL(/pageSize=20/)
  const region = page.getByRole('region', { name: '生産ライン一覧' })
  await expect(region.locator('tbody tr')).toHaveCount(20)
  await expect(page.getByText('240 件', { exact: true })).toBeVisible()
  await expect(page.getByRole('navigation', { name: '生産ラインのページ切替' }).getByText('1 / 12')).toBeVisible()
  await page.getByLabel('表示件数').selectOption('100')
  await expect(page).toHaveURL(/page=1.*pageSize=100|pageSize=100.*page=1/)
  await expect(region.locator('tbody tr')).toHaveCount(100)
  expect(sizes).toContain('100')
  // DEC-009: bounded box, inner scrolling, sticky header, no horizontal page scroll, pager outside the box.
  const box = (await region.boundingBox())!
  expect(box.height).toBeLessThanOrEqual(Math.min(page.viewportSize()!.height * 0.6, 640) + 2)
  expect(await region.evaluate(element => element.scrollHeight > element.clientHeight)).toBe(true)
  await region.focus(); await page.keyboard.press('End')
  await expect.poll(() => region.evaluate(element => element.scrollTop)).toBeGreaterThan(0)
  // Focusing the region may scroll the page, so the header is measured against the box's current position.
  await expect.poll(() => region.evaluate(element => Math.abs(element.querySelector('thead th')!.getBoundingClientRect().top - element.getBoundingClientRect().top))).toBeLessThanOrEqual(2)
  expect(await region.getByRole('navigation').count()).toBe(0)
  expect(await noPageScroll(page)).toBe(true)
  await page.getByRole('button', { name: '次へ', exact: true }).click()
  await expect(page).toHaveURL(/page=2/); await expect(page).toHaveURL(/pageSize=100/)
  await page.getByRole('button', { name: 'クリア', exact: true }).click()
  await expect(page).toHaveURL(/\/production-lines\?pageSize=100$/)
  await expectNoAxeViolations(page)
})

test('TC-431: controls match the production-order heights (DEC-002)', async ({ page }) => {
  await page.goto('/production-orders')
  const orders = page.getByRole('main')
  await expect(orders.getByRole('button', { name: '前のページ' })).toBeVisible()
  const orderFilled = await height(orders.getByRole('link', { name: '+ 新規製造指示' }))
  const orderSearch = await height(orders.locator('form button[type=submit]'))
  const orderOutlined = await height(orders.getByRole('button', { name: 'クリア', exact: true }))
  const orderField = await height(orders.locator('#orderNumber'))
  await page.goto('/production-orders/new')
  const orderSave = await height(page.getByRole('button', { name: '保存', exact: true }))

  await serveLines(page, 3)
  await page.goto('/production-lines')
  const main = page.getByRole('main')
  await expect(main.getByRole('rowheader', { name: 'L-0001' })).toBeVisible()
  expect(await height(main.getByRole('link', { name: '新規ライン' }))).toBeCloseTo(orderFilled, 0)
  expect(await height(main.getByRole('button', { name: '検索', exact: true }))).toBeCloseTo(orderSearch, 0)
  expect(await height(main.getByRole('button', { name: 'クリア', exact: true }))).toBeCloseTo(orderOutlined, 0)
  expect(await height(main.getByLabel('コード・名称で検索'))).toBeCloseTo(orderField, 0)
  expect(await height(main.getByRole('link', { name: 'L-0001を編集' }))).toBeCloseTo(orderOutlined, 0)
  // A filled row action stretches to its outlined neighbour, as on 製品マスタ, so both row buttons line up.
  expect(await height(main.getByRole('button', { name: 'L-0001を使用停止' }))).toBeCloseTo(orderOutlined, 0)

  await page.goto('/production-lines/new')
  expect(await height(page.getByRole('button', { name: '保存', exact: true }))).toBeCloseTo(orderSave, 0)
  expect(await height(page.getByRole('link', { name: 'キャンセル', exact: true }))).toBeCloseTo(orderOutlined, 0)
  expect(await height(page.getByLabel('ラインコード'))).toBeCloseTo(orderField, 0)
  expect(await height(page.getByRole('button', { name: '製品を追加', exact: true }))).toBeCloseTo(orderOutlined, 0)
})

test('TC-425/426: the add dialog pages 20 at a time, confirms the shown unit and returns focus on Escape', async ({ page }) => {
  const code = `ADD-${Date.now()}`
  await page.goto('/production-lines/new')
  await page.getByLabel('ラインコード').fill(code)
  await page.getByLabel('ライン名').fill('追加ダイアログ試験')
  await page.getByLabel('稼働時間／日').fill('8')
  const opener = page.getByRole('button', { name: '製品を追加', exact: true })
  await opener.click()
  const dialog = page.getByRole('dialog', { name: '製品を追加' })
  await expect(dialog.getByLabel('製品を検索')).toBeFocused()
  const choices = dialog.getByRole('region', { name: '製品の候補' })
  await expect(choices.getByRole('radio').first()).toBeVisible()
  expect(await choices.getByRole('radio').count()).toBeLessThanOrEqual(20)
  const total = Number((await dialog.getByText(/^\d+ 件$/).innerText()).replace(/\D/g, ''))
  if (total > 20) await expect(dialog.getByRole('navigation', { name: '製品候補のページ切替' })).toBeVisible()
  await expect(dialog.getByRole('button', { name: '追加', exact: true })).toBeDisabled()
  await page.keyboard.press('Escape')
  await expect(dialog).toBeHidden(); await expect(opener).toBeFocused()

  await opener.click()
  await dialog.getByLabel('製品を検索').fill('P-1001')
  await dialog.getByRole('button', { name: '検索', exact: true }).click()
  await dialog.getByRole('radio', { name: /^P-1001\s/ }).check()
  await expect(dialog).toContainText('追加すると、表示されている単位で 1 単位あたりの製造時間を登録します。')
  await dialog.getByRole('button', { name: '追加', exact: true }).click()
  const minutes = page.getByRole('textbox', { name: /^P-1001 の製造時間/ })
  await expect(minutes).toBeFocused()
  await expect(page.getByText('追加予定', { exact: true })).toBeVisible()
  await expect(page.getByRole('checkbox')).toHaveCount(0)
  await opener.click()
  await dialog.getByLabel('製品を検索').fill('P-1001')
  await dialog.getByRole('button', { name: '検索', exact: true }).click()
  await expect(dialog.getByText('選択できる製品はありません。')).toBeVisible()
  await dialog.getByRole('button', { name: 'キャンセル', exact: true }).click()
  await minutes.fill('0.5')
  await expectNoAxeViolations(page)
  await page.getByRole('button', { name: '保存', exact: true }).click()
  await expect(page.getByText('生産ラインを保存しました。')).toBeVisible()
  const listed = await (await page.request.get(`/api/production-lines?q=${code}&state=all`)).json()
  const saved = await (await page.request.get(`/api/production-lines/${listed.items[0].id}`)).json()
  expect(saved.pairs.items[0]).toMatchObject({ minutesPerUnit: '0.5', requiresUnitConfirmation: false })
})

// DEC-011: maximum-length codes and names, Japanese with spaces and ASCII without spaces, on PC, SP 390 and 320 px.
test('TC-432: long line codes and names never break the list, cards or retire dialog', async ({ page }) => {
  const stamp = Date.now()
  const lines = [{ code: `LONG${stamp}`.padEnd(50, 'X'), name: longJapanese }, { code: `NOSP${stamp}`.padEnd(50, 'Y'), name: noSpaces }]
  for (const line of lines) expect((await page.request.post('/api/production-lines', { data: { ...line, workingHoursPerDay: '23.125', products: [] } })).status()).toBe(201)
  for (const width of [1280, 390, 320]) {
    await page.setViewportSize({ width, height: 900 })
    await page.goto(`/production-lines?q=${stamp}&state=all`)
    const region = page.getByRole('region', { name: '生産ライン一覧' })
    await expect(page.getByText('2 件', { exact: true })).toBeVisible()
    expect(await noPageScroll(page)).toBe(true)
    expect(await noBoxScroll(region)).toBe(true)
    if (width >= 640) {
      for (const header of await region.locator('thead th').all()) expect(await singleLine(header)).toBe(true)
      for (const line of lines) {
        const row = region.getByRole('row').filter({ has: page.getByRole('rowheader', { name: line.code }) })
        expect(await singleLine(row.getByText('使用中', { exact: true }))).toBe(true)
        const edit = (await row.getByRole('link', { name: `${line.code}を編集` }).boundingBox())!
        const retire = (await row.getByRole('button', { name: `${line.code}を使用停止` }).boundingBox())!
        expect(retire.y).toBeCloseTo(edit.y, 0); expect(retire.height).toBeCloseTo(edit.height, 0)
      }
    } else {
      for (const card of await region.getByRole('listitem').all()) {
        const cardBox = (await card.boundingBox())!; const regionBox = (await region.boundingBox())!
        expect(cardBox.x + cardBox.width).toBeLessThanOrEqual(regionBox.x + regionBox.width + 1)
        expect(await noBoxScroll(card)).toBe(true)
      }
    }
    await page.getByRole('button', { name: `${lines[1].code}を使用停止` }).click()
    const dialog = page.getByRole('dialog', { name: 'このラインを使用停止にしますか？' })
    await expect(dialog).toContainText(lines[1].code)
    await inViewport(page, dialog)
    await expectNoAxeViolations(page)
    await page.keyboard.press('Escape')
    await expect(dialog).toBeHidden()
  }
})

test('TC-430/432: long products in the 20-row pair table, add dialog and retire dialog stay inside their boxes', async ({ page }) => {
  const stamp = Date.now()
  const created = await page.request.post('/api/production-lines', { data: { code: `PAIRS-${stamp}`, name: '長い製品の試験', workingHoursPerDay: '8', products: [] } })
  expect(created.status()).toBe(201)
  const line = await created.json()
  const longSku = (n: number) => `SKU${n}-${'Z'.repeat(50)}`.slice(0, 50)
  const products = Array.from({ length: 25 }, (_, i) => choice(i, longSku(i), i % 2 ? noSpaces : longJapanese, i % 3 ? 'kg' : 'セット'))
  const pairs = products.map((product, i) => pairOf(product, i === 1 ? { confirmedUnit: '個', requiresUnitConfirmation: true } : {}))
  await page.route(url => url.pathname === `/api/production-lines/${line.id}`, async route => {
    const params = new URL(route.request().url()).searchParams
    const size = Number(params.get('pairsPageSize') ?? '50'); const current = Number(params.get('pairsPage') ?? '1')
    await route.fulfill({ json: { ...line, pairs: { items: pairs.slice((current - 1) * size, current * size), total: pairs.length, page: current, pageSize: size } } })
  })
  await page.route(url => url.pathname === '/api/production-lines/product-choices', async route => {
    const params = new URL(route.request().url()).searchParams
    const size = Number(params.get('pageSize') ?? '50'); const current = Number(params.get('page') ?? '1')
    const offered = products.map((product, i) => ({ ...product, id: id(0x17000 + i) }))
    await route.fulfill({ json: { items: offered.slice((current - 1) * size, current * size), total: offered.length, page: current, pageSize: size } })
  })
  for (const width of [1280, 390, 320]) {
    await page.setViewportSize({ width, height: 900 })
    await page.goto(`/production-lines/${line.id}/edit`)
    const region = page.getByRole('region', { name: '生産可能な製品' })
    await expect(region.getByRole('textbox', { name: new RegExp(`^${products[0].sku} の製造時間`) })).toBeVisible()
    expect(await region.getByRole('textbox', { name: /の製造時間/ }).count()).toBe(20)
    await expect(page.getByRole('navigation', { name: '製品のページ切替' }).getByText('1 / 2')).toBeVisible()
    expect(await noPageScroll(page)).toBe(true)
    expect(await noBoxScroll(region)).toBe(true)
    expect(await region.evaluate(element => element.scrollHeight > element.clientHeight)).toBe(true)
    expect((await region.boundingBox())!.height).toBeLessThanOrEqual(Math.min(900 * 0.5, 560) + 2)
    await expect(region.getByText('単位の再確認が必要です。')).toBeVisible()
    for (const input of (await region.getByRole('textbox').all()).slice(0, 3)) {
      const inputBox = (await input.boundingBox())!; const regionBox = (await region.boundingBox())!
      expect(inputBox.x + inputBox.width).toBeLessThanOrEqual(regionBox.x + regionBox.width)
    }
    if (width >= 640) {
      for (const header of await region.locator('thead th').all()) expect(await singleLine(header)).toBe(true)
      await region.evaluate(element => { element.scrollTop = element.scrollHeight })
      const headerBox = (await region.locator('thead th').first().boundingBox())!
      expect(Math.abs(headerBox.y - (await region.boundingBox())!.y)).toBeLessThanOrEqual(2)
    }
    await page.getByRole('button', { name: '製品を追加', exact: true }).click()
    const dialog = page.getByRole('dialog', { name: '製品を追加' })
    const choices = dialog.getByRole('region', { name: '製品の候補' })
    await expect(choices.getByRole('radio')).toHaveCount(20)
    await expect(dialog.getByRole('navigation', { name: '製品候補のページ切替' })).toBeVisible()
    await inViewport(page, dialog)
    expect(await noBoxScroll(choices)).toBe(true)
    expect(await choices.evaluate(element => element.scrollHeight > element.clientHeight)).toBe(true)
    await expectNoAxeViolations(page)
    await page.keyboard.press('Escape')
    await page.getByRole('button', { name: `${products[0].sku}を使用停止` }).click()
    const retire = page.getByRole('dialog', { name: 'この製品を使用停止にしますか？' })
    await expect(retire).toContainText(products[0].sku)
    await inViewport(page, retire)
    await page.keyboard.press('Escape')
    await expectNoAxeViolations(page)
  }
})
