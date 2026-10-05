import { test,expect } from '@playwright/test'
import { signIn,expectNoAxeViolations } from './helpers'

test.use({ screenshot: 'off', trace: { mode: 'retain-on-failure', screenshots: false } })
test('TC-412: reviewed geometry, full dates and read-only history at breakpoint edges',async({page})=>{
 await signIn(page)
 for(const width of [1440,900,640,639,390,320]){
  await page.setViewportSize({width,height:1080})
  await page.goto('/plant-calendar?month=2026-10&date=2026-10-12')
  const table=page.locator('.calendar-month'),agenda=page.locator('.calendar-agenda'),details=page.locator('.calendar-day-panel')
  await expect(details).toContainText('2026-10-12')
  await expect(width>=640?table:agenda).toBeVisible()
  await expect((width>=640?table:agenda).getByRole('button',{name:/^2026-10-/})).toHaveCount(31)
  const calendar=await (width>=640?table:agenda).boundingBox(),day=await details.boundingBox()
  if(!calendar||!day)throw new Error('Missing calendar geometry')
  if(width>=900){expect(day.x-calendar.x-calendar.width).toBeGreaterThanOrEqual(23);expect(Math.abs(day.y-calendar.y)).toBeLessThanOrEqual(2)}
  else expect(day.y-calendar.y-calendar.height).toBeGreaterThanOrEqual(23)
  await expect(page.getByRole('button',{name:'前へ',exact:true})).toHaveCount(0)
  expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBe(true)
  await expectNoAxeViolations(page)
  await page.goto('/plant-calendar?month=2026-10&date=2026-10-12&mode=day')
  await expect(page.getByRole('radio',{name:'稼働日',exact:true})).toBeVisible()
  await expect(page.getByRole('radio',{name:'非稼働日',exact:true})).toBeVisible()
  await page.goto('/plant-calendar?month=2026-10&date=2026-10-12&mode=history')
  await expect(page.getByRole('button',{name:'戻る',exact:true})).toBeVisible()
  await expect(page.getByRole('button',{name:'保存',exact:true})).toHaveCount(0)
  await expect(page.getByRole('radio')).toHaveCount(0)
 }
})
test('TC-413: all bounded choices reachable and selected labels survive page changes',async({page})=>{
 await signIn(page)
 const created=await page.request.post('/api/production-lines',{data:{code:`PAGING-${Date.now()}`,name:'検索候補ライン',workingHoursPerDay:'8',products:[]}})
 expect(created.ok()).toBe(true)
 const seed=await created.json()
 const original=await (await page.request.get('/api/plant-calendar/line-choices?page=1')).json()
 const sample=original.items.find((line:{id:string})=>line.id===seed.id)
 expect(sample).toBeTruthy()
 let releasePage:()=>void=()=>{}
 const pageGate=new Promise<void>(resolve=>{releasePage=resolve})
 let pendingPage=false
 await page.route('**/api/plant-calendar/line-choices?*',async route=>{
  const p=Number(new URL(route.request().url()).searchParams.get('page')??1)
  if(p===2){pendingPage=true;await pageGate}
  const rows=Array.from({length:p===1?50:1},(_,n)=>({...sample,id:n===0&&p===1?sample.id:`${String(n+(p-1)*50+1).padStart(8,'0')}-1111-4111-8111-111111111111`,code:`CHOICE-${n+(p-1)*50+1}`,name:'検索候補ライン'}))
  return route.fulfill({json:{...original,page:p,pageSize:50,totalCount:51,totalPages:2,items:rows}})
 })
 await page.goto('/plant-calendar?month=2026-10&date=2026-10-12')
 const scope=page.locator('main form').first()
 await scope.getByRole('combobox',{name:'生産ライン',exact:true}).selectOption(sample.id)
 const disclosure=scope.locator('details')
 await disclosure.locator('summary').press('Enter')
 await expect(disclosure).toHaveAttribute('open','')
 await disclosure.getByRole('button',{name:'次へ',exact:true}).click()
 await expect.poll(()=>pendingPage).toBe(true)
 await expect(scope.getByRole('combobox',{name:'生産ライン',exact:true}).locator('option:checked')).toHaveText('CHOICE-1 検索候補ライン')
 releasePage()
 await expect(scope.getByRole('combobox',{name:'生産ライン',exact:true}).locator('option:checked')).toHaveText('CHOICE-1 検索候補ライン')
 await expect(scope.getByRole('combobox',{name:'生産ライン',exact:true}).locator('option')).toContainText(['工場共通','CHOICE-1 検索候補ライン','CHOICE-51 検索候補ライン'])
 await expect(disclosure.getByRole('button',{name:'次へ',exact:true})).toBeDisabled()
 await disclosure.getByRole('button',{name:'条件をクリア',exact:true}).click()
 await expect(disclosure.getByRole('button',{name:'前へ',exact:true})).toBeDisabled()
})

test('TC-414: product paging, independent queries and complete long labels',async({page})=>{
 await signIn(page);await page.setViewportSize({width:390,height:900})
 const made=await page.request.post('/api/product-master',{data:{sku:`PG-${Date.now()}`,name:'長い製品名称'.repeat(12),unit:'kg'}})
 expect(made.ok()).toBe(true)
 const product=await made.json()
 try {
 const eligible=await (await page.request.get(`/api/production-lines/eligible?productId=${product.id}`)).json()
 const lineResponse=await page.request.post('/api/production-lines',{data:{code:`PG-L-${Date.now()}`,name:'候補確認ライン',workingHoursPerDay:'8',products:[{productId:product.id,minutesPerUnit:'0.125',expectedUnit:'kg',expectedUnitRevision:eligible.product.unitRevision,confirmUnit:true}]}})
 expect(lineResponse.ok()).toBe(true)
 const line=await lineResponse.json()
 const original=await (await page.request.get(`/api/plant-calendar/product-choices?lineId=${line.id}&page=1`)).json()
 const sample=original.items[0]
 await page.route('**/api/plant-calendar/product-choices?*',route=>{
  const url=new URL(route.request().url()),p=Number(url.searchParams.get('page')??1),q=url.searchParams.get('q')
  const rows=q?[]:Array.from({length:p===1?50:1},(_,n)=>({...sample,id:n===0&&p===1?sample.id:`${String(n+(p-1)*50+1).padStart(8,'0')}-2222-4222-8222-222222222222`,sku:`PG-${n+(p-1)*50+1}`}))
  return route.fulfill({json:{...original,page:p,pageSize:50,totalCount:q?0:51,totalPages:q?0:2,items:rows}})
 })
 await page.goto(`/plant-calendar?month=2026-10&date=2026-10-12&lineId=${line.id}`)
 const capacity=page.getByRole('region',{name:'参考能力',exact:true})
 const select=capacity.getByRole('combobox',{name:'製品',exact:true})
 await select.selectOption(sample.id)
 const fullLabel=`PG-1 ${sample.name} (kg)`
 await expect(capacity.locator('p').filter({hasText:fullLabel})).toBeVisible()
 const disclosure=capacity.locator('details').filter({has:page.getByText('製品候補を検索',{exact:true})})
 await disclosure.locator('summary').click()
 await disclosure.getByRole('button',{name:'次へ',exact:true}).click()
 await expect(select.locator('option:checked')).toHaveText(fullLabel)
 await disclosure.getByRole('textbox',{name:'検索',exact:true}).fill('no-matches')
 await disclosure.getByRole('button',{name:'検索',exact:true}).click()
 await expect(disclosure.getByRole('button',{name:'次へ',exact:true})).toHaveCount(0)
 await expect(select.locator('option:checked')).toHaveText(fullLabel)
 await expect(capacity.getByRole('combobox',{name:'生産ライン',exact:true})).toHaveValue(line.id)
 expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBe(true)
 await disclosure.getByRole('button',{name:'条件をクリア',exact:true}).click()
 await expect(disclosure.getByRole('button',{name:'前へ',exact:true})).toBeDisabled()
 } finally {
  // Keep this synthetic long-name fixture from affecting subsequent screen journeys.
  const currentResponse=await page.request.get(`/api/product-master/${product.id}`)
  expect(currentResponse.ok()).toBe(true)
  const current=await currentResponse.json()
  const cleaned=await page.request.put(`/api/product-master/${product.id}`,{data:{sku:current.sku,name:'候補検証済み製品',unit:current.unit,drawingNumber:current.drawingNumber,version:current.version}})
  expect(cleaned.ok()).toBe(true)
  expect((await cleaned.json()).name).toBe('候補検証済み製品')
 }
})
