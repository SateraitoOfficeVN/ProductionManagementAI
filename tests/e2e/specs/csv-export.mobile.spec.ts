import { expect, test } from '@playwright/test'
import { exportCsv, fileName, header, openList, parseCsv } from './csvExport'
import { expectNoAxeViolations, signIn } from './helpers'

// WI-016 TC-468 (SP, 390 px): 「CSV出力」 is a full-width, touch-sized button on its own row and downloads the same file.

test('SP export: full-width 44 px button, download, no horizontal scroll (TC-468)', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await signIn(page)
  const total = await openList(page, '?status=Draft')

  const button = page.getByRole('button', { name: 'CSV出力' })
  const box = (await button.boundingBox())!
  expect(box.height).toBeGreaterThanOrEqual(44)
  expect(box.width).toBeGreaterThanOrEqual(390 - 2 * 16 - 1) // full content width inside the 16 px page gutter
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)

  const { name, bytes } = await exportCsv(page)

  expect(name).toMatch(fileName)
  const records = parseCsv(bytes.subarray(3).toString('utf8'))
  expect(records[0].join(',')).toBe(header)
  expect(records).toHaveLength(total + 1)
  await expect(page.getByText(`CSVファイルを出力しました（${total}件）。`)).toBeVisible()
  await expectNoAxeViolations(page)
})
