# Azure CAF Decisions (v0) — Order Project

This document captures the initial Cloud Adoption Framework (CAF) decisions for the **Order** project.
It is intentionally minimal (MVP) and can be refined as the project grows.

------

## 1. Scope & Goals
- **Project type:** Reference / portfolio project (DDD + Clean Architecture)
- **Primary goal:** Demonstrate professional cloud foundation + deployment readiness (not just code)
- **Non-goals (for v0):**
  - Enterprise-grade hub-spoke networking
  - Full private endpoints everywhere
  - Multi-subscription production landing zone

---

## 2. Environments
- **Environments:** `dev`, `prod`
- **Reasoning:** Keep the project lean while still demonstrating separation of concerns.

---

## 3. Region & Naming
- **Primary region:** `westeurope` (WEU)
- **Naming convention (pattern):**
  - `<org>-<project>-<env>-<region>-<resource>-<suffix>`
- **Example:**
  - `sj-order-dev-weu-rg-01`
  - `sj-order-dev-weu-kv-01`
  - `sj-order-dev-weu-law-01`

---

## 4. Subscription & Resource Organization
- **Subscription model (v0):** Single subscription
- **Separation mechanism:** Resource Groups + naming + tagging
- **Resource groups (suggested):**
  - `rg-order-dev-weu-platform`
  - `rg-order-dev-weu-workload`
  - `rg-order-prod-weu-platform`
  - `rg-order-prod-weu-workload`

---

## 5. Identity & Access (RBAC)
- **Identity provider:** Microsoft Entra ID (Azure AD)
- **App-to-Azure auth:** Managed Identity (preferred)
- **RBAC principle:** Least privilege, scope as low as possible (RG/resource)
- **Human access:** Use Entra ID groups instead of direct user assignments where possible.

---

## 6. Governance (Minimum Baseline)
### 6.1 Tagging (mandatory)
- `project=order`
- `env=dev|prod`
- `owner=sajad`
- `managedBy=iac`
- `costCenter=personal`

### 6.2 Policies (v0)
- Allowed locations: `westeurope`
- Require tags: `project`, `env`, `managedBy`
- Enforce HTTPS where supported
- Audit/deny public access for sensitive resources where feasible (to be refined)

---

## 7. Security Baseline (v0)
- **Secrets:** Azure Key Vault
- **TLS:** HTTPS only
- **Public network access:** Allowed for v0 (with controlled access)
- **Private endpoints:** Not in v0 (to reduce complexity/cost); can be introduced later as an “enterprise mode”.

---

## 8. Observability
- **Central logging:** Log Analytics Workspace
- **App telemetry:** Application Insights
- **Baseline:** Collect platform + workload logs, enable diagnostics where possible.

---

## 9. Infrastructure as Code & CI/CD
- **IaC tool:** Bicep
- **CI tool:** GitHub Actions
- **Day-0 CI:** Build + Test + Bicep syntax validation
- **CD approach (later):** Deploy Landing Zone first, then workload.

---

## 10. Next Steps
1. Create a minimal Landing Zone in `/infra/landing-zone` using Bicep
2. Add reusable modules in `/infra/modules`
3. Introduce parameter files per environment
4. Extend policies and security controls iteratively
