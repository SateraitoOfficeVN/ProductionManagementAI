import { expect, test } from '@playwright/test'
import { exportCsv, fileName, header, openList, parseCsv } from './csvExport'
import { expectNoAxeViolations, signIn } from './helpers'

// WI-016 TC-467 (PC): 「CSV出力」 on Screen B downloads every order matching the applied filters, as UTF-8 with BOM.
// Read-only against the demo stack; assertions use the totals the list itself reports.

test.beforeEach(async ({ page }) => {
  await signIn(page)
})

test('exports every Draft order across pages with BOM and header, and stays accessible (TC-467)', async ({ page }) => {
  const total = await openList(page, '?status=Draft&pageSize=10')
  expect(total).toBeGreaterThan(10) // the export must span more than the visible page
  await expect(page.getByRole('button', { name: 'CSV出力' })).toHaveAccessibleDescription(
    `検索した絞り込み条件の${total}件を出力します。`,
  )

  const { name, bytes } = await exportCsv(page)

  expect(name).toMatch(fileName)
  expect([...bytes.subarray(0, 3)]).toEqual([0xef, 0xbb, 0xbf])
  const records = parseCsv(bytes.subarray(3).toString('utf8'))
  expect(records[0].join(',')).toBe(header)
  expect(records).toHaveLength(total + 1)
  for (const record of records.slice(1)) {
    expect(record).toHaveLength(13)
    expect(record[7]).toBe('下書き')
  }
  await expect(page.getByText(`CSVファイルを出力しました（${total}件）。`)).toBeVisible()
  await expectNoAxeViolations(page)
})

test('without filters the hint and the file cover every order (TC-467)', async ({ page }) => {
  const total = await openList(page, '')
  await expect(page.getByText(`すべての製造指示${total}件を出力します。`)).toBeVisible()

  const { bytes } = await exportCsv(page)

  expect(parseCsv(bytes.subarray(3).toString('utf8'))).toHaveLength(total + 1)
})
