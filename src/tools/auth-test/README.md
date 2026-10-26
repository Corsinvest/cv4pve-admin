# Identity providers for tests

Two containers to try External Authentication (Enterprise Edition) without real accounts.

```bash
docker compose up -d
```

| Service | Address | Content |
|---------|---------|---------|
| OpenID Connect (Keycloak) | `http://127.0.0.1:18080/realms/test` | client `cv4pve`, secret `cv4pve-secret`, redirect `https://localhost:7251/*`; users `existing`, `newuser`, `attacker`, `groupuser`, `nogroup` with password `Test-1234!`; groups `pve-admins`, `pve-helpdesk` in the `groups` claim |
| LDAP (OpenLDAP) | `127.0.0.1:18389`, no TLS | base `dc=test,dc=local`, service account `cn=admin,dc=test,dc=local` / `admin`; users `uid=jdoe` (group `pve-admins`) and `uid=jsmith` (group `pve-helpdesk`) with password `Test-1234!`; user filter `(uid={0})` |

The Keycloak console is at `http://127.0.0.1:18080` (user `admin`, password `admin`). The realm and the directory are recreated from the files in `keycloak/` and `ldap/` when the containers are created again (`docker compose down` then `up -d`).

Keycloak takes about a minute to start.
