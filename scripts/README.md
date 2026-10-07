# scripts

Repeatable project scripts.

- `docs-pdf.py`: renders a Markdown document under `docs/` to PDF, English or Japanese, with Mermaid diagrams and embedded images (RFC 0008). Usage and requirements are in its docstring.
- `harness-deck/`: builds the English and Japanese ai/ harness presentation (`docs/en/presentations/`, `docs/ja/presentations/`) with ReportLab. See its README.
- `start-app.ps1`: starts the whole app locally with one command, entirely in Docker (no .NET SDK, Node or psql on the host; Windows PowerShell 5.1 or PowerShell 7): creates `deploy/.env` with random passwords if missing, starts the database, applies migrations as the owner in the one-shot `migrate` container, then builds and starts backend and frontend and waits until they answer. `-ActivateCalendar` also activates the plant calendar on this local database (today, plant timezone; skipped if already active), `-SkipMigrations` skips migrations, `-Open` opens the browser. Double-click `start-app.cmd` in the repo root to run it with `-ActivateCalendar -Open`, or from the repo root: `powershell -ExecutionPolicy Bypass -File scripts\start-app.ps1`.
