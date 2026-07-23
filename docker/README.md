# Docker Development Services

Phase 1 containers only PostgreSQL. API and frontend run directly on the host for fast development.

```powershell
docker compose up -d database
docker compose ps
docker compose logs -f database
```

Do not commit `.env`.
