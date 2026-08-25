# Railway backend setup

## 1. Services
1. Add a **PostgreSQL** plugin to the same Railway project.
2. Deploy this API (`OnlineShop.Api` as Root Directory).

## 2. Variables (API service)
Link Postgres (or set manually):

| Variable | Value |
|----------|--------|
| `DATABASE_URL` | From Postgres service (Railway usually injects this when linked) |
| `ASPNETCORE_URLS` | `http://0.0.0.0:${PORT}` |
| `Admin__Password` | your admin password |
| `Cors__AllowedOrigins__0` | `http://localhost:3000` |
| `Cors__AllowedOrigins__1` | your frontend URL later (Vercel/Railway) |

Optional:
| `ASPNETCORE_ENVIRONMENT` | `Production` (Swagger still enabled in this build) |

## 3. Networking
Settings → Networking → **Generate Domain**

Then open:
- `https://YOUR-DOMAIN/` → health JSON
- `https://YOUR-DOMAIN/swagger`
- `https://YOUR-DOMAIN/api/categories`

## 4. Why the old URL failed
- App used **LocalDB** (Windows only) — crash/loop on Linux
- **HTTPS redirect** inside the container breaks Railway’s proxy
- No automatic schema create — now `Database.Migrate()` runs on startup

## 5. After code push
Redeploy the API. First boot creates tables + seed data on Postgres.
