# DaycareAPI2026 — Complete local learning demo

## Live Demo

The Daycare Management System is deployed on Microsoft Azure.

Live website:
https://daycare-samah-2026-a3b8bpdwd9cfgdd0.canadacentral-01.azurewebsites.net

GitHub repository:
https://github.com/samahalmsadde/DaycareManagementSystem

This project is an educational demonstration and uses fictional data only.

An ASP.NET Core .NET 10 backend and browser interface in **one Visual Studio project**. No extra NuGet packages are needed. **Use fictional data only.**

## Run in Visual Studio 2026

1. **Do not copy files into your old broken project.** Extract this ZIP to a new folder.
2. Open **Visual Studio 2026 → File → Open → Project/Solution**.
3. Select **DaycareAPI2026.csproj** from the extracted folder.
4. Build → Build Solution (Ctrl+Shift+B).
5. Click the green **https** Run button.
6. Open the URL shown in the running terminal (normally `https://localhost:7284/`).
7. The home page is the complete interface. You do not need `/swagger`.
8. Test `https://localhost:7284/api/health` (returns JSON) and `/api/children` (initially `[]`).

If HTTPS certificate warning occurs on your local computer, use the **http** profile (`http://localhost:5284/`) instead for local testing. Do not use HTTP for a public deployment.

## Features

- Dashboard: child counts, gender, attendance, staff, payment total.
- Children: add, edit, search, delete.
- Attendance: save Present/Absent by date; existing marks can be changed.
- Daily care: add, edit, delete records and search history.
- Staff: add, edit, delete.
- Payments: add and delete; total shown on dashboard.
- Backup: export JSON.
- Health endpoint, Dockerfile, GitHub Actions build + smoke test, architecture and AI log.

## Main API endpoints

| Method | URL | Purpose |
|---|---|---|
| GET | /api/health | Service status |
| GET/POST | /api/children | List/create children |
| PUT/DELETE | /api/children/{id} | Edit/delete child |
| GET/POST | /api/staff | List/create staff |
| PUT/DELETE | /api/staff/{id} | Edit/delete staff |
| GET/PUT/DELETE | /api/attendance | List/upsert/clear attendance |
| GET/POST | /api/daily-care | List/create daily record |
| PUT/DELETE | /api/daily-care/{id} | Edit/delete daily record |
| GET/POST | /api/payments | List/create payment |
| DELETE | /api/payments/{id} | Delete payment |
| GET | /api/dashboard | Summary |
| GET | /api/export | Export JSON |

## Example request

POST /api/children
Content-Type: application/json

```json
{"firstName":"Test","lastName":"Child","gender":"Female","group":"Toddlers"}
```

## Where data is saved

The app writes local data to `App_Data/daycare.json`. It persists after restarting the application. The folder is excluded from Git. Keep backups secure. Deleting a child also deletes their attendance, daily care and payments.

## Important limitations

This is a **local learning prototype, not a production system**. It has no login, permissions, encryption at rest, audit trail, or real cloud services. It uses a JSON file instead of a relational/cloud database and is intended for a single local instance. Do not deploy publicly or store real children's confidential information. The course's cloud hosting, monitoring, ETL, automated deployment, and security deliverables still need real configuration and evidence.

## GitHub / CI

Commit this folder to a GitHub repository. `.github/workflows/build.yml` builds and smoke-tests the app on push. A cloud deployment pipeline is **not** included; a hosting provider and credentials are required. Never commit secrets.

## Docker (optional)

```bash
docker build -t daycareapi2026 .
docker run --rm -p 8080:8080 daycareapi2026
```

Visit `http://localhost:8080/`. Without a mounted persistent volume, Docker container data is temporary.

## Suggested manual test sequence

1. Create two fictional children; dashboard total should be 2.
2. Edit one child; total stays 2.
3. Mark one Present and one Absent today; dashboard shows 1 and 1.
4. Create a daily record and find it in History.
5. Add a staff member and a payment; check dashboard totals.
6. Restart the server and verify records remain.
7. Download a JSON backup.

## Files

- `DaycareAPI2026.csproj` — .NET project configuration
- `Program.cs` — complete API endpoints and web hosting
- `Models/DaycareModels.cs` — data models
- `Data/DataStore.cs` — JSON storage
- `wwwroot/index.html` — all interface sections
- `wwwroot/style.css` — styling
- `wwwroot/script.js` — browser actions and API integration
- `appsettings.json` — logging configuration
- `Properties/launchSettings.json` — local launch URLs
- `.github/workflows/build.yml` — CI build and smoke test
- `Dockerfile` — container image
- `docs/ARCHITECTURE.md` — data flow
- `docs/AI_PROMPT_LOG.md` — AI assistance log
