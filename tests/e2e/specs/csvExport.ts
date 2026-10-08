import { readFile } from 'node:fs/promises'
import { expect, type Page } from '@playwright/test'

// Shared by the WI-016 CSV export specs (PC and SP); a plain module, because Playwright forbids importing a spec file.

export const fileName = /^製造指示一覧_\d{8}-\d{4}\.csv$/
export const header = '指示番号,製品コード,製品名,生産ライン,数量,単位,納期,ステータス,納期遅れ,備考,作成日時,更新日時,完了日時'

/** A minimal RFC 4180 reader, so a quoted multi-line note cannot skew the row count. */
export function parseCsv(text: string): string[][] {
  const records: string[][] = []
  let record: string[] = []
  let field = ''
  let quoted = false
  for (let i = 0; i < text.length; i++) {
    const c = text[i]
    if (quoted) {
      if (c === '"' && text[i + 1] === '"') { field += '"'; i++ }
      else if (c === '"') quoted = false
      else field += c
    } else if (c === '"') quoted = true
    else if (c === ',') { record.push(field); field = '' }
    else if (c === '\r' && text[i + 1] === '\n') { record.push(field); field = ''; records.push(record); record = []; i++ }
    else field += c
  }
  return records
}

/** Opens the list with a query, returns the total its summary reports. */
export async function openList(page: Page, query: string): Promise<number> {
  await page.goto(`/production-orders${query}`)
  await expect(page.getByRole('heading', { level: 1, name: '製造指示一覧' })).toBeVisible()
  const summary = page.getByRole('status').first()
  await expect(summary).toHaveText(/^\d+件中 \d+〜\d+件$/)
  return Number((await summary.textContent())!.match(/^(\d+)件中/)![1])
}

/** Clicks 「CSV出力」 and returns the downloaded file's name and bytes. */
export async function exportCsv(page: Page) {
  const downloadPromise = page.waitForEvent('download')
  await page.getByRole('button', { name: 'CSV出力' }).click()
  const download = await downloadPromise
  const bytes = await readFile((await download.path())!)
  return { name: download.suggestedFilename(), bytes }
}
