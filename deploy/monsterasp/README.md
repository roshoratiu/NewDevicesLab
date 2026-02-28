# MonsterASP deployment notes

This folder documents deployment specifics for `ndl.runasp.net`.

## Website

- Hostname: `site57027.siteasp.net`
- Site root: `/wwwroot`
- Domain: `ndl.runasp.net`

## Database

- SQL Server: `db42967.databaseasp.net`
- Database: `db42967`
- Port: `1433`

## Safe secret handling

- Keep credentials out of git.
- Prefer setting `ConnectionStrings__DefaultConnection` in hosting environment.
- If panel has no env-var support, edit server-side `appsettings.Production.json` directly after upload.
