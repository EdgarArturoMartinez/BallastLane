import { test, expect } from '@playwright/test';

const DEMO = { username: 'demo', password: 'Demo@12345' };

test('drag & drop moves card between columns and persists', async ({ page }) => {
  // login
  await page.goto('/');
  await page.fill('#username', DEMO.username);
  await page.fill('#password', DEMO.password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await page.waitForURL(/\/tasks/);

  const title = `DND E2E ${Date.now()}`;

  // create a task and capture the created id from the POST response
  await page.getByRole('button', { name: 'New task' }).click();
  await page.fill('#m-title', title);
  await page.fill('#m-description', 'drag and drop test');
  const postRespPromise = page.waitForResponse(resp => resp.url().includes('/api/tasks') && resp.request().method() === 'POST', { timeout: 5000 });
  await page.getByRole('button', { name: /Create task|Save changes/ }).click();
  const postResp = await postRespPromise;
  const created = await postResp.json();
  const createdId = created?.id;
  // ensure card created
  const cardXpath = `xpath=//h3[normalize-space(.)="${title}"]/ancestor::div[contains(@class,'group')][1]`;
  await expect(page.locator(`h3:has-text("${title}")`)).toBeVisible({ timeout: 5000 });

  // target column = In Progress
  const targetXpath = `xpath=//span[normalize-space(.)="In Progress"]/ancestor::div[contains(@class,'rounded-2xl')][1]`;

  // perform drag-and-drop and wait for the PUT request that persists the change
  const putRespPromise = page.waitForResponse(resp => resp.url().endsWith(`/api/tasks/${createdId}`) && resp.request().method() === 'PUT', { timeout: 5000 });
  await page.dragAndDrop(cardXpath, targetXpath);
  const putResp = await putRespPromise;
  // ensure backend returned success
  if (putResp.status() >= 400) {
    throw new Error(`PUT /api/tasks failed with status ${putResp.status()}`);
  }

  // verify card exists in target column
  const targetCard = page.locator(`${targetXpath} >> h3:has-text("${title}")`);
  await expect(targetCard).toBeVisible({ timeout: 3000 });

  // reload to ensure persisted status
  await page.reload();
  await expect(page.locator(`${targetXpath} >> h3:has-text("${title}")`)).toBeVisible({ timeout: 3000 });
});
