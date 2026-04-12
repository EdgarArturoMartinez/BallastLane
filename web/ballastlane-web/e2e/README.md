Run Playwright E2E (dev-only)

1. Install dependencies for E2E tests:

```bash
cd web/ballastlane-web/e2e
npm install
npm run install-browsers
```

2. Run tests against a running stack (run `docker compose up --build -d` from repo root first):

```bash
npm test
```

Note: Playwright and browser binaries are kept out of the main frontend package to keep the standard frontend Docker build small for interviewers.
