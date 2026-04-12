# Plan de 12 Fases — Reconstrucción

Nota breve: No se encontró un documento único y oficial con las "12 fases" en el repositorio. Esta reconstrucción se basa en evidencias dispersas (README, `memories/repo/project-conventions.md`, `docs/DAILY_CONTEXT.md`, `docs/Arthur_Checkpoint_Exploration.md`) y en las decisiones de arquitectura y pasos técnicos ya presentes en el repositorio.

Fuentes consultadas:
- `README.md`
- `memories/repo/project-conventions.md`
- `docs/DAILY_CONTEXT.md`
- `docs/Arthur_Checkpoint_Exploration.md`

Resumen del estado actual (evidencia):
- En `memories/repo/project-conventions.md` se indica: "Phases 0–6 completed; Phase 7 (containerization) scaffolded." — por tanto, Fases 0 a 6 están marcadas como cumplidas y la Fase 7 está iniciada.
- `README.md` muestra el scaffold inicial (Phase 0) y referencia continuación hacia Phase 1.
- `docs/DAILY_CONTEXT.md` y el historial de checkpoints contienen las tareas recientes (Dockerfiles, `docker-compose`, EOD docs).

Reconstrucción propuesta — Fases (0..11)

0. Scaffold del repositorio y estructura de proyectos (Scaffold inicial)
   - Estado: Completada.
   - Qué incluye: proyectos Domain / Application / Infrastructure / API / Tests.

1. Capa Domain — modelos, entidades, reglas de negocio
   - Estado: Completada.

2. Capa Application — casos de uso, DTOs, puertos (interfaces)
   - Estado: Completada.

3. Capa Infrastructure — adaptadores, repositorios ADO.NET, migraciones
   - Estado: Completada (migrator/seeder presentes según memorias).

4. API — controladores, endpoints (Auth, Tasks, Users), middlewares
   - Estado: Completada.

5. Frontend — SPA React + Vite, UI CRUD para tareas
   - Estado: Completada.

6. Tests y validación local — xUnit + Moq, pruebas unitarias e integración
   - Estado: Completada (tests locales mencionados como "green").

7. Containerization — Dockerfiles y `docker-compose` para db → api → web
   - Estado: **Completada** (April 11, 2026).
   - Fixes aplicados: `nginx.conf` (SPA routing + proxy `/api`), `node:22-alpine` (Vite 8 requiere Node ≥ 20.19), healthcheck SQL Server 2022 (`mssql-tools18`), `EnsureDatabaseExistsAsync` en `DbMigrator`, removida duplicación de `__Migrations` en `0001_init.sql`.
   - Validado: `docker compose up --build -d` levanta los 3 servicios; migraciones y seed aplicadas; JWT login + Tasks GET funcionan; nginx proxy `/api` hacia el API container verificado.

8. Validación de entorno y logs — ejecutar contenedores, verificar migraciones y runtime
   - Estado: **Completada** (April 11, 2026 — validada junto con Phase 7).

9. CI básica — GitHub Actions para `dotnet build` y `dotnet test` en PRs
   - Estado: Pendiente (próximo paso).

10. Hardening y configuración — variables de entorno, secretos, ajuste de JWT, hashing
   - Estado: Parcial (buenas prácticas documentadas; verificar envs y secretos en despliegue).

11. Documentación y Release — README actualizado, instrucciones Run Locally, demo credentials, release tag
   - Estado: Parcial (README y DAILY_CONTEXT existen; completar sección "Run Locally" con env vars y credenciales de demo).


Por qué pudo "perderse" el documento original
- No se halló un único archivo que contenga las 12 fases: la información del plan está fragmentada entre `README.md`, `memories/repo/project-conventions.md` y los documentos de checkpoint (`docs/Arthur_Checkpoint_Exploration.md` / `docs/DAILY_CONTEXT.md`).
- Posibles causas:
  - El roadmap original nunca fue creado como un único artefacto sino como notas dispersas. 
  - Se creó localmente y no fue commiteado/pusheado.
  - Se dividió intencionalmente en memorias y checkpoints (diseño deliberado) y por eso no hay un solo archivo monolítico.

Recomendaciones inmediatas
- Añadir este documento (`docs/12_PHASE_ROADMAP.md`) al repo (ya creado) para evitar pérdida futura.
- Actualizar `memories/repo/project-conventions.md` con una línea que haga referencia a este roadmap y su versión.
- Añadir una tarea en `docs/DAILY_CONTEXT.md` o en el proceso EOD para verificar que los artefactos clave (roadmap, conventions, DAILY_CONTEXT) estén commiteados antes del cierre del día.

Siguientes pasos sugeridos (rápidos)
1. Reiniciar Docker Desktop y ejecutar:

```powershell
docker-compose up --build -d
docker-compose ps
docker-compose logs -f
```

2. Verificar que las migraciones se aplican y que las APIs responden; corregir env vars si hace falta.
3. Scaffoldear `.github/workflows/ci.yml` mínimo que ejecute `dotnet build` y `dotnet test`.

---
Documento reconstruido automáticamente por el asistente a partir de las fuentes del repo.
