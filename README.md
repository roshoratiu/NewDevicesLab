# NewDevicesLab

Layered .NET 8 solution for the course project.

## Architecture

- `src/frontend` → ASP.NET Core Web API host + Swagger UI
- `src/Application` → use-cases/services/contracts
- `src/Domain` → domain entities
- `src/Persistance` → EF Core + SQL Server repositories
- `src/Infrastructure` → reserved for integrations (mail, storage, devices, etc.)
- `test/tests` → unit/integration tests

## Local database (Docker)

1. Ensure your `.env` exists (it is created from `.env.example`):
   - `MSSQL_SA_PASSWORD=Your_strong_password_123!`
2. Start SQL Server:
   - `docker compose up -d --wait`
3. SQL Server is exposed on `localhost,1434` (host port), container internal port is `1433`.

## Run the API

```bash
docker compose up -d --wait
dotnet run --project src/frontend/NewDevicesLab.Frontend.csproj
```

## Authentication and admin access

- The current secure pass uses local username/email + password login with a server-side cookie session.
- Admin APIs and the devices API now require authenticated access plus permission claims.
- The app now retries database startup for about 60 seconds so first-run SQL warm-up does not immediately crash the web host.
- The seeded bootstrap administrator is:
  - Username: `admin`
  - Email: `admin@ru.nl`
  - Password: `ChangeMeNow!2026`
- Change that password immediately in real deployments.

### Current admin capabilities

- Sign in to the protected admin interface at `/`
- Create users
- Assign users to multiple groups
- Change the permissions granted to each group
- Review the permission catalog that will also drive future project and order-sheet access

## Development configuration

- Local development uses `src/frontend/appsettings.json` (Docker SQL on `localhost,1434`).
- Production deployment uses `src/frontend/appsettings.Production.json`.
- Best practice: do not keep real production DB password in source files; set it in server environment and override with:
  - `ConnectionStrings__DefaultConnection`

## Swagger documentation

When the API is running:

- Swagger UI: `http://localhost:5253/swagger` (or the port shown in terminal)
- OpenAPI JSON: `http://localhost:5253/swagger/v1/swagger.json`

### What is documented

- API metadata: title, version, description, contact
- Endpoint summaries from XML comments
- Response types for each endpoint

### Current endpoints

- `GET /api/auth/me` → current signed-in user
- `POST /api/auth/login` → create auth cookie session
- `POST /api/auth/logout` → end auth cookie session
- `GET /api/admin/overview` → admin dashboard data
- `POST /api/admin/users` → create a user
- `PUT /api/admin/users/{userId}/groups` → update a user's group memberships
- `PUT /api/admin/groups/{groupId}/permissions` → update group permissions
- `GET /api/devices` → list all devices
- `POST /api/devices` → create a new device
  - Body example:
    ```json
    {
      "name": "Raspberry Pi 5"
    }
    ```

## Notes

- On startup, the API ensures the DB exists via EF Core `EnsureCreated()`.
- On startup, the app seeds the system permissions, default groups (`Student`, `Teaching Assistant`, `Teacher`, `Order`, `Administrator`), and the bootstrap admin account if missing.
- Connection string is in `src/frontend/appsettings.json` and defaults to local SQL Server on port `1434`.

## Permission model

Permissions are assigned to groups, and users can belong to multiple groups.
This is the proposed first permission set based on the current admin scope plus the project/order-sheet workflow you described.

### Core and admin permissions

- `devices.read` → view the device catalog
- `devices.create` → create device entries
- `devices.update` → edit device entries
- `devices.delete` → delete device entries
- `admin.access` → open the protected admin interface
- `admin.users.manage` → create users and change user group memberships
- `admin.groups.manage` → change group permission assignments
- `system.full_access` → bypass all permission checks and access everything

### Project permissions

- `projects.create` → create a new project
- `projects.view.own` → view projects the user created or participates in
- `projects.view.group` → view projects across student groups or across the wider lab
- `projects.status.manage` → update project statuses

### Order-sheet permissions

- `ordersheets.create` → open a new order sheet
- `ordersheets.view.own` → view the user’s own order sheets
- `ordersheets.view.group` → view submitted order sheets across the wider group
- `ordersheets.submit` → submit an order sheet after totals are prepared

## Default group mapping

- `Student`
  - `devices.read`
  - `projects.create`
  - `projects.view.own`
  - `ordersheets.create`
  - `ordersheets.view.own`
  - `ordersheets.submit`
- `Teaching Assistant`
  - `devices.read`
  - `devices.create`
  - `devices.update`
  - `projects.view.group`
  - `projects.status.manage`
  - `ordersheets.view.group`
- `Teacher`
  - `devices.read`
  - `devices.create`
  - `devices.update`
  - `devices.delete`
  - `projects.view.group`
  - `projects.status.manage`
  - `ordersheets.view.group`
- `Order`
  - `devices.read`
  - `ordersheets.view.group`
- `Administrator`
  - all current permissions, plus `system.full_access`

## Planned next domain features

These are not implemented yet, but the permission model already anticipates them:

- `Projects`
  - A student creates a project with a name and description
  - The student adds collaborators by email from existing users in the system
  - Students only see projects they created or participate in
  - Teaching Assistants and Teachers can see broader project listings and status
- `Order sheets`
  - Fields: site name, component name, brand, link, and price in euro
  - The sheet calculates a running total before submission
  - Group-visible review depends on `ordersheets.view.group`

## MonsterASP deployment (ndl.runasp.net)

### 1) Build publish package

```bash
chmod +x scripts/publish-monsterasp.sh
./scripts/publish-monsterasp.sh
```

Published output is generated in `publish/monsterasp`.

### 2) Upload to hosting

- Hostname: `site57027.siteasp.net`
- Protocol: SFTP (port `22`) or FTP (port `21`)
- Target directory: `/wwwroot`
- Upload all files from `publish/monsterasp` into `/wwwroot`

### 3) Configure production database

- SQL host: `db42967.databaseasp.net`
- SQL database: `db42967`
- SQL port: `1433`
- Configure production connection string in hosting settings (preferred as environment variable):

```text
ConnectionStrings__DefaultConnection=Server=db42967.databaseasp.net;Database=db42967;User Id=db42967;Password=<YOUR_PASSWORD>;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True
```

If environment variables are not available in the panel, set the same value in `appsettings.Production.json` on the server copy only.

### 4) Verify deployment

- Open `https://ndl.runasp.net/swagger`
- Open `https://ndl.runasp.net/swagger/v1/swagger.json`
- Test `GET /api/devices`

## CI/CD (auto deploy on main)

GitHub Actions workflow: `.github/workflows/deploy-monsterasp.yml`

Trigger:

- Push to `main`
- Manual run via `workflow_dispatch`

### Setup GitHub Secrets (one-time, required)

The FTP deployment requires three repository secrets on GitHub. Follow these steps:

1. Go to your GitHub repository
2. Click **Settings** (top menu)
3. Click **Secrets and variables** > **Actions** (left sidebar)
4. Click **New repository secret** (green button)
5. Create each secret:

| Name | Value |
|------|-------|
| `MONSTERASP_FTP_SERVER` | `site57027.siteasp.net` |
| `MONSTERASP_FTP_USERNAME` | `site57027` |
| `MONSTERASP_FTP_PASSWORD` | (paste your FTP password from MonsterASP panel) |

Once all three are saved, each push to `main` will automatically build, publish, and deploy to `/wwwroot` on MonsterASP.
