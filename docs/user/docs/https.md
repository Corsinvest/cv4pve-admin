# HTTPS with Reverse Proxy

cv4pve-admin runs on HTTP by default. For HTTPS, place a reverse proxy in front of it.

!!! info "How it works"
    The Docker Compose setup uses two networks:

    - `appnet` — internal network (postgres, watchtower, cv4pve-admin)
    - `publicnet` — network exposed to the outside (cv4pve-admin, reverse proxy)

    The reverse proxy joins `publicnet` and reaches cv4pve-admin via the internal hostname `cv4pve-admin:8080`.
    `8080` is the fixed internal container port — not the `CV4PVE_ADMIN_PORT` from `.env` (which is the host port and is no longer needed once the proxy is in place).
    Internal services (Watchtower, postgres) continue to communicate via `appnet` and are not affected.

!!! info "Forwarded headers"
    The Docker Compose files already include `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, so the application correctly handles HTTPS termination at the proxy level. No extra configuration needed.

---

## Caddy

The simplest option — automatic HTTPS with Let's Encrypt, minimal configuration.

Add a `caddy` service to your `docker-compose.yaml`:

```yaml
services:
  caddy:
    image: caddy:2-alpine
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile
      - caddy_data:/data
      - caddy_config:/config
    networks:
      - publicnet   # same network as cv4pve-admin

volumes:
  caddy_data:
  caddy_config:
```

Create a `Caddyfile` in the same directory:

```
admin.example.com {
    reverse_proxy cv4pve-admin:8080
}
```

Replace `admin.example.com` with your domain. Caddy handles certificates automatically.

---

## Traefik

For environments already using Traefik, add it to `publicnet` and add labels to the `cv4pve-admin` service:

```yaml
services:
  traefik:
    image: traefik:v3
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - ./traefik.yml:/traefik.yml
    networks:
      - publicnet

  cv4pve-admin:
    # ... existing config ...
    labels:
      traefik.enable: "true"
      traefik.http.routers.cv4pve-admin.entrypoints: "websecure"
      traefik.http.routers.cv4pve-admin.rule: "Host(`admin.example.com`)"
      traefik.http.routers.cv4pve-admin.tls: "true"
      traefik.http.routers.cv4pve-admin.tls.certresolver: "letsencrypt"
      traefik.http.services.cv4pve-admin.loadbalancer.server.port: "8080"
      traefik.docker.network: "publicnet"
```

---

## Nginx (manual, external)

If Nginx runs outside Docker (directly on the host), it cannot use the Docker network name — point it to `localhost` on the host port. The host port mapping must remain in `docker-compose.yaml`.

```nginx
server {
    listen 443 ssl;
    server_name admin.example.com;

    ssl_certificate     /etc/ssl/certs/admin.example.com.crt;
    ssl_certificate_key /etc/ssl/private/admin.example.com.key;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name admin.example.com;
    return 301 https://$host$request_uri;
}
```

!!! warning "WebSocket headers required"
    The `Upgrade` and `Connection` headers are required for Blazor Server (WebSocket). Without them the UI will not work correctly.
