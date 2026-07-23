# Cloud Assignment System V3 — Specification Pack

**Status:** Frozen implementation contract  
**Architecture:** Pragmatic Clean Architecture + Modular Monolith  
**Frontend:** Modern Magical Academy  
**Deployment direction:** Google Cloud–centered, cloud-agnostic code boundaries

This pack is the source of truth before implementation. Each module must be delivered as one integrated vertical slice: database, backend, authorization, validation, frontend, tests, and manual verification.

## Locked rules

1. No role-specific all-in-one dashboard.
2. No business logic inside controllers or React page components.
3. No direct cloud SDK usage outside Infrastructure adapters.
4. No UI control without a real backend flow.
5. Search, sort, filter, pagination, loading, empty, and error states are part of the feature.
6. Excel import always uses preview → confirm → result.
7. Secrets never enter Git.
8. V2 remains online as a rollback/demo backup until V3 passes regression.
