import { test, expect, type Page } from '@playwright/test';

const DEMO_USER = { username: 'demo', password: 'Demo@12345' };

async function uiLogin(page: Page) {
  await page.goto('/');
  await page.fill('#username', DEMO_USER.username);
  await page.fill('#password', DEMO_USER.password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await page.waitForURL(/\/tasks/);
  await expect(page.getByRole('heading', { name: 'My Tasks' })).toBeVisible();
}

test.describe('Auth + Tasks E2E', () => {
  test('login and create task', async ({ page }) => {
    await uiLogin(page);
    const title = `E2E Task ${Date.now()}`;

    await page.getByRole('button', { name: 'New task' }).click();
    await expect(page.getByRole('dialog', { name: /New task|Edit task/ })).toBeVisible();

    await page.fill('#m-title', title);
    await page.fill('#m-description', 'Created by Playwright E2E test');
    await page.getByRole('button', { name: 'Create task' }).click();

    // Wait for the created card to appear
    await expect(page.locator(`h3:has-text("${title}")`)).toBeVisible({ timeout: 5000 });
  });

  test('login, create and delete task', async ({ page }) => {
    await uiLogin(page);
    const title = `E2E Delete ${Date.now()}`;

    await page.getByRole('button', { name: 'New task' }).click();
    await page.fill('#m-title', title);
    await page.fill('#m-description', 'To be deleted by E2E');
    await page.getByRole('button', { name: 'Create task' }).click();

    await expect(page.locator(`h3:has-text("${title}")`)).toBeVisible({ timeout: 5000 });

    // Hover the card area then click the delete button related to the title
    const cardAncestorXPath = `//h3[normalize-space(.)="${title}"]/ancestor::div[contains(@class, 'group')][1]`;
    const deleteButtonXPath = `//h3[normalize-space(.)="${title}"]/../button[@aria-label="Delete task"]`;

    const card = page.locator(`xpath=${cardAncestorXPath}`);
    await card.hover();

    page.once('dialog', dialog => dialog.accept());
    await page.locator(`xpath=${deleteButtonXPath}`).first().click();

    // Ensure the card is removed
    await expect(page.locator(`h3:has-text("${title}")`)).toHaveCount(0, { timeout: 5000 });
  });

  test('login, create and edit task', async ({ page }) => {
    await uiLogin(page);
    const title = `E2E Edit ${Date.now()}`;

    // Create a task to edit
    await page.getByRole('button', { name: 'New task' }).click();
    await page.fill('#m-title', title);
    await page.fill('#m-description', 'Created for edit test');
    await page.getByRole('button', { name: 'Create task' }).click();
    await expect(page.locator(`h3:has-text("${title}")`)).toBeVisible({ timeout: 5000 });

    // Open card to edit
    await page.locator(`h3:has-text("${title}")`).first().click();
    const dlg = page.getByRole('dialog', { name: 'Edit task' });
    await expect(dlg).toBeVisible();

    const newTitle = `${title} (edited)`;
    await dlg.locator('#m-title').fill(newTitle);
    await dlg.locator('#m-description').fill('Edited by Playwright E2E test');

    // Change status to 'In Progress' via the label inside the dialog
    await dlg.getByText('In Progress').first().click();

    await dlg.getByRole('button', { name: 'Save changes' }).click();

    // Verify edited card appears with new title and status badge
    await expect(page.locator(`h3:has-text("${newTitle}")`)).toBeVisible({ timeout: 5000 });
    const cardXPath = `//h3[normalize-space(.)="${newTitle}"]/ancestor::div[contains(@class, 'group')][1]`;
    const card = page.locator(`xpath=${cardXPath}`);
    await expect(card.getByText('In Progress')).toBeVisible();
  });

  test('create task validation: title required', async ({ page }) => {
    await uiLogin(page);

    await page.getByRole('button', { name: 'New task' }).click();
    const dlg = page.getByRole('dialog', { name: /New task|Edit task/ });
    await expect(dlg).toBeVisible();

    // Clear title and attempt to submit
    await dlg.locator('#m-title').fill('');
    await dlg.getByRole('button', { name: 'Create task' }).click();

    // Expect client-side validation message
    await expect(dlg.getByText('Title is required')).toBeVisible();
  });
});
