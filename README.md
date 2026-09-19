# Portfolio / MSA Gold & Jewellery Trading L.L.C.

ASP.NET Core MVC (.NET 8) company portfolio and admin CMS using a 3-layer architecture:

- `Portfolio.Web` — MVC, Areas/Admin, public UI and static assets
- `Portfolio.Business` — DTOs, interfaces and application services
- `Portfolio.Data` — EF Core, SQLite, entities, Identity and seeding

## Stack

- .NET 8 / ASP.NET Core MVC
- Entity Framework Core 8
- SQLite (`Portfolio.Web/portfolio.db`)
- ASP.NET Core Identity with Guid keys
- Local image/file storage under `wwwroot/uploads`
- Responsive dark navy + gold premium UI
- CSS + IntersectionObserver animations

## MSA Content Included

The initial website content is configured for **MSA GOLD & JEWELLERY TRADING L.L.C**:

- Office 302, Hind Plaza-6A, Gold Souq, Deira, Dubai, UAE
- Landline: +971 4 345 0109
- Mobile / WhatsApp: +971 56 418 4546
- Email: msagold0@gmail.com
- Domain: www.msagold.ae
- Established: 2025

Social profile links are seeded as editable placeholders because no official MSA profile URLs were provided.

Products:

- Fine Gold Bullion
- Commercial Trade Gold
- Silver Bullion

Services:

- Gold Jewelry Solutions
- Technical Refining & Assaying
- Trade Settlement Desk

## Admin Login (development)

- URL: `/Admin/Account/Login`
- Email: `admin@portfolio.com`
- Password: `Admin@123456`

Change the bootstrap credentials before production.

## Audit Logging

All requests under `/Admin` are captured in the `AuditLogs` table with:

- user / anonymous actor
- controller + action
- HTTP method
- requested path
- entity id when available
- status code
- IP address
- user agent
- UTC timestamp

View the logs at `/Admin/AuditLogs` using the `AuditLogs.View` permission.

## Run

1. Open `Portfolio.sln` in Visual Studio.
2. Set `Portfolio.Web` as startup project.
3. Run with HTTPS.
4. On first startup, EF migrations run and the seed creates roles, permissions, admin user and initial MSA content.

The database is a single SQLite file and can be backed up by copying `Portfolio.Web/portfolio.db`.

## UI Theme & Localization
- Light / Dark / System theme modes are supported across the public website and Admin.
- Appearance supports fine-grained shared, light-palette, and dark/night-palette color control.
- Theme preferences are stored in WebsiteSetting records under the `Theme.*` namespace so existing SQLite databases do not require a destructive schema change.
- Browser-level light/night preference can be toggled locally from the header.
- English (`en`) and Arabic (`ar`) are supported for the UI, including RTL layout switching.
- A persistent culture cookie is set through `/Language/Set`.
- Static UI phrases have an Arabic dictionary and are applied server-side for layouts plus client-side for the rest of the existing views, while business content remains exactly as entered by the administrator.
- Localization files are copied on build/publish from `Portfolio.Web/Localization`.
