# AI Operations Guide — GitHub Copilot Pro+

**Author:** Arturo Martínez
**Date:** April 11, 2026
**Plan:** GitHub Copilot Pro+
**Monthly premium requests (approx.):** 1,500
**Sources:** Official GitHub Copilot documentation (April 2026)

---

## Index

1. [Full inventory of available models](#1-full-inventory-of-available-models)
2. [Free vs premium — the golden rule](#2-free-vs-premium--the-golden-rule)
3. [Operation modes in VS Code](#3-operation-modes-in-vs-code)
4. [Daily onboarding — how to start a session with full context](#4-daily-onboarding)
5. [Skills — what they are and how to use them](#5-skills)
6. [Database operations guidance](#6-database-operations)
7. [Which model to use while coding](#7-which-model-to-use-while-coding)
8. [Implementation planning before coding](#8-implementation-planning)
9. [Git operations optimization](#9-git-operations-optimization)
10. [Documentation and Markdown guidance](#10-documentation-and-markdown)
11. [Testing and TDD guidance](#11-testing-and-tdd)
12. [Frontend (React/Angular) guidance](#12-frontend-guidance)
13. [Azure DevOps and CI/CD guidance](#13-azure-devops-and-cicd)
14. [Master matrix — model by scenario](#14-master-matrix--model-by-scenario)
15. [Monthly budget simulation](#15-monthly-budget-simulation)
16. [Golden rules to maximize efficiency](#16-golden-rules-to-maximize-efficiency)
17. [How to create and use memories](#17-how-to-create-and-use-memories)
18. [How to create `.github/copilot-instructions.md`](#18-how-to-create-githubcopilot-instructionsmd)
19. [Next recommended steps](#19-next-recommended-steps)

---

## 1. Full inventory of available models

### Included models (do NOT consume premium requests on paid plan)

These models are effectively unlimited on Pro+. Interactions with them count as 0 premium requests.

| Model | Multiplier | Provider | Strength |
|-------|:---:|---------|---------|
| **GPT-4.1** | **0×** | OpenAI | General-purpose, good for code and prose — balanced quality/speed. |
| **GPT-4o** | **0×** | OpenAI | Multimodal (text + images). Fast. |
| **GPT-5 mini** | **0×** | OpenAI | Copilot default. Fast, accurate across languages; good for debugging and reasoning. |
| **Raptor mini** | **0×** | GitHub | Specialized for inline suggestions and quick explanations. |

> Rule #1: Do what you can with these four models first — they are free (0×) on Pro+.

---

### Low-cost premium models (0.25× – 0.33×)

| Model | Multiplier | Provider | Strength |
|-------|:---:|---------|---------|
| **Grok Code Fast 1** | **0.25×** | xAI | Fast code generation and debugging. 4 interactions ≈ 1 premium request. |
| **Claude Haiku 4.5** | **0.33×** | Anthropic | Lightweight tasks and short answers. 3 interactions ≈ 1 premium request. |
| **Gemini 3 Flash** | **0.33×** | Google | Fast responses for light queries. 3 interactions ≈ 1 premium request. |
| **GPT-5.4 mini** | **0.33×** | OpenAI | Codebase exploration and grep-style tooling. 3 interactions ≈ 1 premium request. |

> Rule #2: Use these for simple tasks that need a step above GPT-5 mini — low cost.

---

### Standard-cost premium models (1×)

| Model | Multiplier | Provider | Strength |
|-------|:---:|---------|---------|
| **Claude Sonnet 4.0** | **1×** | Anthropic | Balanced performance for code workflows. |
| **Claude Sonnet 4.5** | **1×** | Anthropic | Complex problem solving and advanced reasoning. |
| **Claude Sonnet 4.6** | **1×** | Anthropic | Improved completions and robust reasoning. Recommended for deep reasoning. |
| **Gemini 2.5 Pro** | **1×** | Google | Complex code generation, debugging, research workflows. |
| **Gemini 3.1 Pro** | **1×** | Google | Advanced reasoning in long contexts; reliable edit-test loops. |
| **GPT-5.1** | **1×** | OpenAI | Multi-step problem solving and architectural analysis. |
| **GPT-5.2** | **1×** | OpenAI | Multi-step problem solving and architectural analysis. |
| **GPT-5.2-Codex** | **1×** | OpenAI | Agentic development tasks. |
| **GPT-5.3-Codex** | **1×** | OpenAI | High-quality code for complex tasks (features, tests, refactors, reviews). |
| **GPT-5.4** | **1×** | OpenAI | Complex reasoning, code analysis, technical decisions. |

> Rule #3: These models are your primary tools for complex work — use them intentionally.

---

### High-cost premium models (3× – 30×)

| Model | Multiplier | Provider | Strength |
|-------|:---:|---------|---------|
| **Claude Opus 4.5** | **3×** | Anthropic | Deep problem solving and advanced reasoning. |
| **Claude Opus 4.6** | **3×** | Anthropic | Anthropic's most capable model; deep reasoning and complex debugging. Each interaction = 3 premium requests. |
| **Claude Opus 4.6 (fast mode)** | **30×** | Anthropic | Accelerated version of Opus 4.6 (Pro+ exclusive). Each interaction = 30 premium requests. Use only for critical emergencies. |

> Rule #4: Think of Opus as a scalpel, not a hammer. Use it for precision surgery, not routine tasks. 10 prompts with Opus 4.6 = 30 premium requests; Opus fast mode is 10× more expensive.

---

### 10% discount — Auto Model Selection

If you use **Auto** (automatic model selection) in VS Code, you get a **10% discount** on the multiplier. Example:
- Claude Sonnet 4.0 goes from 1× to **0.9×**
- Claude Opus 4.6 goes from 3× to **2.7×**

> Tip: When you don't have a strong model preference, enable Auto for the discount.

---

## 2. Free vs premium — the golden rule

Sometimes you must choose between speed/cost and deeper reasoning. Follow this decision flow:

- If the task is simple or repetitive: use GPT-5 mini / GPT-4.1 (0×) or low-cost models (0.25–0.33×).
- If the task requires deep reasoning: use Sonnet/Gemini/GPT-5.x (1×).
- If the task is critical and absolutely needs the best answer: use Opus (3×) or Opus fast (30×) only in emergencies.

**Monthly budget (Pro+): ~1,500 premium requests**

---

## 3. Operation modes in VS Code

### Ask Mode
What it does: answers questions, explains code, suggests solutions. Does NOT modify files.
When to use: quick queries, exploration, planning.
Cost: 1 prompt = 1 premium request × model multiplier.

### Edit Mode
What it does: proposes edits to files you select. You accept or reject changes.
When to use: targeted refactors, controlled edits.
Cost: 1 prompt = 1 premium request × model multiplier.

### Agent Mode
What it does: acts autonomously — explores files, runs commands, iterates until the task is complete.
When to use: multi-file tasks, feature generation, deep debugging.
Cost: 1 premium request per user prompt × multiplier. The agent's internal tool calls (file reads, executing tests) do NOT count as premium requests.

> Critical discovery: In Agent Mode, only YOUR prompts count as premium requests. The agent's internal tool calls (file reads, tests) do not. This makes Agent Mode highly efficient for well-structured prompts.

### Copilot Cloud Agent (GitHub.com)
An autonomous agent that runs in a Codespace and can create PRs, implement features, etc. It charges per session/steer. Use when you want a fully autonomous run.

---

## 4. Daily onboarding

Problem: each day you need the AI to know the project's architecture, recent changes, and current work state. Re-supplying full context every day wastes tokens.

Optimal strategy — Context Pyramid:

- Memories (persistent, 0 tokens)
- Short daily summary (`DAILY_CONTEXT.md`) — 1 quick prompt to summarize (free with GPT-5 mini)
- Agent-mode diff exploration (free tool calls)
- Full checkpoint only when necessary

Steps:
1. Use persistent memory files (`/memories/`) for conventions and decisions.
2. Create `DAILY_CONTEXT.md` at the end of each day (prompt GPT-5 mini to summarize today’s changes).
3. Start the next day using Agent Mode with GPT-5 mini: ask it to read `/memories/` and `DAILY_CONTEXT.md`, then explore only the files mentioned.

Recommended model: GPT-5 mini or GPT-4.1 (0×). Recommended mode: Agent Mode. Estimated daily cost: 0 premium requests.

---

## 5. Skills

Skills are reusable `SKILL.md` modules with workflows and best practices (testing, profiling, docs, agent customization). Copilot can auto-load skills when relevant.

To provide project-level guidance, add `.github/copilot-instructions.md` with a short conventions summary. That file is loaded automatically and gives Copilot repo context for free.

---

## 6. Database operations

Covered scenarios: DDL/DML scripts, schema design, complex queries, ADO.NET patterns, migrations, performance troubleshooting.

Rule of thumb (80/20):
- 80% of DB work (DDL, simple queries, ADO.NET patterns) → GPT-5 mini / GPT-4.1 (0×)
- 20% (schema design, optimization, deep debugging) → Claude Sonnet 4.6 or Gemini 3.1 Pro (1×)

Never use Opus for routine DB work; the cost is rarely justified.

---

## 7. Which model to use while coding

Follow the escalation pattern: Autopilot → Assisted → Neurosurgeon.

1. Start with GPT-5 mini for autocompletion, boilerplate, simple refactors, DTOs, and basic tests.
2. If results are insufficient, escalate to Claude Sonnet 4.6 or GPT-5.x (1×) for complex logic, design, or debugging.
3. If still unresolved and the problem is critical, use Claude Opus 4.6 (3×).

Always try cheaper models first.

---

## 8. Implementation planning

Planning before coding saves requests and reduces iterations. Typical plan: 1–3 prompts (free with GPT-5 mini).

Pattern: Explore code (Agent, GPT-5 mini) → Draft plan (GPT-5 mini) → Optionally validate plan with a higher-cost model (Sonnet 4.6) → Execute (Agent)

Context persists in the same conversation; save plans to files for new sessions.

---

## 9. Git operations optimization

Do basic Git commands manually (`git add`, `commit`, `push`, etc.). Use Copilot for value-add tasks:
- Diff analysis and PR reviews (Agent + GPT-5 mini for diffs; Copilot Review for PRs)
- Commit message generation via GitHub Desktop (integrated, free)

---

## 10. Documentation and Markdown

Docs generation and README writing: GPT-5 mini (0×). Diagram generation (Mermaid): GPT-4.1. Summaries and long doc reviews: Agent Mode with GPT-5 mini (free tool calls).

---

## 11. Testing and TDD

Unit tests and Moq-based tests: GPT-5 mini (Agent). Integration tests requiring complex setup: GPT-5.3-Codex or Sonnet (1×). Use TDD pattern: write port interfaces, generate failing tests, implement, refactor.

---

## 12. Frontend guidance

React + TypeScript + Vite: GPT-5 mini covers most tasks (components, styles, API integration). Escalate only for complex debugging.

---

## 13. Azure DevOps and CI/CD

YAML pipelines: GPT-4.1. Docker multi-stage builds: GPT-5 mini. Troubleshooting pipeline logs: Agent Mode (free tool calls).

---

## 14. Master matrix — model by scenario

Use GPT-5 mini for onboarding, general coding, frontend, docs, and most tests. Escalate to Sonnet/Gemini/GPT-5.x for complex reasoning and to Opus only for emergencies.

---

## 15. Monthly budget simulation

With disciplined use (mostly 0× models and occasional 1× escalations) you can consume a small fraction of a 1,500-request monthly budget. Discipline avoids depleting premium capacity.

---

## 16. Golden rules to maximize efficiency

1. Start with GPT-5 mini for everything you can.
2. Use Agent Mode for file reads (tool calls are free).
3. Keep long documents in memories and create short daily summaries.
4. Do Git commands manually; use AI for analysis and reviews.
5. Escalate models only when necessary.
6. Reserve Opus for high-value emergencies.
7. Store context in files, not in long prompts.
8. Use `.github/copilot-instructions.md` for repo-level context.
9. Enable Auto model selection when appropriate (10% discount).
10. Frontend work is generally free with GPT-5 mini.

---

## 17. How to create and use memories

What they are: Markdown files that store facts, conventions, and shortcuts Copilot loads automatically. Paths:
- `/memories/` — user-level (persistent)
- `/memories/session/` — session-level (temporary)
- `/memories/repo/` — repo-level (project facts)

What to store: architecture decisions, coding rules, CI commands, constraints (no secrets), and short references.

Format: short bullets, one fact per line, keep files small (<200 lines).

Example: `/memories/repo/project-conventions.md` should include the project's architecture, data access rule (ADO.NET only), test policy, common commands, and CI notes.

How Copilot uses them: files under `/memories/` and `/memories/repo/` are auto-loaded. Edit them when conventions change and tell the agent to re-read.

---

## 18. How to create `.github/copilot-instructions.md`

Place this file at `.github/copilot-instructions.md` and include a concise set of conventions, common commands, testing policy, code review rules, and model escalation guidance. Keep it short (50–200 lines). Example content is already added to the repo file.

Steps to create locally (do not commit until you say so):

```powershell
mkdir .github
echo "# copilot-instructions" > .github/copilot-instructions.md
```

---

## 19. Next recommended steps

- Keep `memories/repo/project-conventions.md` updated as decisions are made.
- Fill `docs/DAILY_CONTEXT.md` at end of day using the EOD template.
- When you're ready, I can commit these files or open a PR — I will not commit without your explicit instruction.

*End of guide.*
