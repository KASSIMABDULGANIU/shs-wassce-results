# Installer & Auto-Update Setup Guide

This guide takes you from "app runs with `dotnet run`" → "school gets a Setup.exe,
and your app can notify them of new versions."

---

## PART A — Build the Installer (Inno Setup)

### A1. Install Inno Setup
Download free from: https://jrsoftware.org/isinfo.php (just click "Download")

### A2. Publish a self-contained build
In your project terminal:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

This creates a folder:
```
bin\Release\net9.0-windows\win-x64\publish\
```
containing `SHSWassceResultsMgt.exe` and everything needed — no .NET install
required on the school's PC.

### A3. Compile the installer
1. Open `setup.iss` (provided) in Inno Setup
2. Check the `#define PublishDir` path matches step A2's folder — it should by default
3. Click **Build → Compile** (or press F9)
4. Output appears in: `Output\SHSWassceResultsMgt-Setup.exe`

That `Setup.exe` is what you give to the school. It installs the app with a
Start Menu entry, optional desktop icon, and uninstaller — and creates
`shs_wassce.db` on first run.

### A4. Each time you release a new version
1. Bump `<Version>1.0.1</Version>` in `SHSWassceResultsMgt.csproj`
2. Also update `#define MyAppVersion "1.0.1"` and `OutputBaseFilename` in `setup.iss`
3. Repeat A2 and A3

---

## PART B — Auto-Update via GitHub (free)

### B1. Create a GitHub repository
1. Go to https://github.com/new
2. Name it e.g. `shs-wassce-results` — **Public** repo is simplest (no auth needed to read files)
3. Push your project code to it (or just the `version.json` file — your source
   code doesn't *have* to be public if you don't want, only `version.json` and the
   release `.exe` need to be reachable)

### B2. Add version.json to the repo root
Use the provided `version.json` template:
```json
{
  "version": "1.0.0",
  "downloadUrl": "https://github.com/YOUR_USERNAME/shs-wassce-results/releases/download/v1.0.0/SHSWassceResultsMgt-Setup.exe",
  "notes": "Initial release."
}
```
Replace `YOUR_USERNAME` with your actual GitHub username/org.

### B3. Point the app at your version.json
Open `Services/UpdateService.cs`, replace:
```csharp
private const string VersionUrl =
    "https://raw.githubusercontent.com/YOUR_USERNAME/shs-wassce-results/main/version.json";
```
with your real username/repo. Rebuild + reinstall once with this change.

### B4. How updates actually get triggered
**Nothing happens automatically on GitHub's side** — YOU control it:

1. You build a new version (Part A4)
2. Go to your GitHub repo → **Releases → Draft a new release**
3. Tag it `v1.0.1`, upload `SHSWassceResultsMgt-Setup.exe` as a release asset
4. Edit `version.json` in the repo: change `"version": "1.0.1"` and update `downloadUrl`
   to point to that new release asset
5. Commit/push `version.json`

From this point on, **every user's app** — next time they open it (with internet) —
will see `1.0.1 > 1.0.0` and prompt: *"A new version is available, download now?"*
Clicking "Yes" opens the download link in their browser, they run the new Setup.exe
(which installs over the old one, keeping their `shs_wassce.db` data).

### B5. (Optional) Automate the build with GitHub Actions
The provided `.github/workflows/release.yml` — if placed in your repo — will
automatically build and attach a zip whenever you push a tag like `v1.0.1`:
```powershell
git tag v1.0.1
git push origin v1.0.1
```
You'd still manually update `version.json` with the new download link (or extend
the workflow to do it — let me know if you want that).

---

## Quick Summary / Mental Model

| Question | Answer |
|---|---|
| Does GitHub "push" updates to users? | No — your app **checks** `version.json` on startup |
| Does the user need a GitHub account? | No (public repo = no login needed) |
| What if user is offline? | Check fails silently, app works 100% normally |
| Where is user data stored? | `shs_wassce.db` next to the .exe — installer doesn't touch it on update |
| How do I push a new version? | Build → Inno Setup → GitHub Release → edit `version.json` |

---

## Files in this package

| File | Where it goes |
|---|---|
| `setup.iss` | Project root — open with Inno Setup |
| `version.json` | Root of your GitHub repo |
| `.github/workflows/release.yml` | GitHub repo workflow file |
| `Services/UpdateService.cs` | App service file — replace the URL with your repo |
