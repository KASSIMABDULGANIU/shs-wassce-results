# SHS WASSCE Results Management

A Windows desktop app (WPF, .NET 9) for managing WASSCE-style exam results in a
Senior High School. Works **fully offline** with a local SQLite database, and
checks for **updates online** when internet is available.

---

## 1. First-time setup (in VS Code terminal)

```powershell
# Restore NuGet packages
dotnet restore

# Create the database (one-time, generates Migrations folder)
dotnet tool install --global dotnet-ef   # only if not already installed
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates `shs_wassce.db` next to the executable with:
- Default WASSCE grading scale (Grades 1-9)
- Pre-seeded core + elective subjects (General Arts, Business, Science)

## 2. Run the app

```powershell
dotnet run
```

## 3. Build a release (.exe you can give to the school)

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output: `bin\Release\net9.0-windows\win-x64\publish\SHSWassceResultsMgt.exe`

This single .exe runs on any Windows PC — no .NET install needed on the target machine.

---

## App structure

| Folder | Purpose |
|---|---|
| `Models/` | Data classes: Student, Subject, Score, GradingScale, Staff |
| `Data/` | `AppDbContext` — SQLite database via EF Core |
| `Services/` | `GradingService` (auto grade/remark), `PdfReportService` (result slips), `UpdateService` (auto-update check) |
| `Views/` | All pages: Dashboard, Students, Subjects, Score Entry, Rankings, Reports, Staff, Settings |

## How scoring works

1. **Students** page — add your class list (name, index no., class, program, year, term)
2. **Subjects** page — confirm/add subjects per program (pre-seeded with WASSCE core + electives)
3. **Score Entry** — pick class + subject + year/term → grid loads all students →
   enter Class Score (0-50) and Exam Score (0-50) → Total/Grade/Remark auto-compute → Save
4. **Rankings** — pick class + year/term → computes position by total score,
   average, and WASSCE "Best 6" aggregate
5. **Reports** — pick a student → Preview → Export as PDF result slip
6. **Settings** — adjust the grading scale bands (e.g. if your school uses different cutoffs)

## Auto-Updater

On startup, the app silently checks `version.json` (hosted on GitHub or any URL) for a
newer version. To enable this:

1. Host `version.json` somewhere public (e.g. a GitHub repo)
2. Edit `Services/UpdateService.cs` → change `VersionUrl` to your hosted file's raw URL
3. When you release a new version: update `version.json`'s `version` and `downloadUrl`,
   and bump `<Version>` in `SHSWassceResultsMgt.csproj`

If the user is offline, the check fails silently and the app works normally —
**fully functional offline at all times.**

---

## Tech stack

- **.NET 9 / WPF** — UI framework
- **SQLite + EF Core** — local database (file: `shs_wassce.db`)
- **MaterialDesignInXAML** — modern UI theme
- **QuestPDF** — PDF result slip generation
- **CommunityToolkit.Mvvm** — MVVM helpers (for future expansion)
