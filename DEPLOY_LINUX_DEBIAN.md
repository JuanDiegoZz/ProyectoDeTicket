# 📘 MANUAL DE DESPLIEGUE EN PRODUCCIÓN (LINUX DEBIAN 12 / 13)
## Sistema de Gestión de Tickets de Soporte TI — TecNM Campus Monclova
**Entorno de Producción:** Servidor Linux Debian 12 / 13  
**Arquitectura Desacoplada:** Backend API REST .NET 10 + Frontend Single Page Application (Vue 3 / Nginx) + PostgreSQL 18  

---

### 1. REQUISITOS PREVIOS EN EL SERVIDOR DEBIAN

#### 1.1. Actualizar el sistema e instalar dependencias básicas
```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y curl ufw git fail2ban apt-transport-https ca-certificates gnupg lsb-release nginx
```

#### 1.2. Instalar Docker Engine y Docker Compose Oficial
```bash
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
sudo systemctl enable --now docker
```

---

### 2. CONFIGURACIÓN DE SEGURIDAD Y FIREWALL (UFW + FAIL2BAN)

```bash
# 1. Reglas UFW de firewall
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow 22/tcp comment 'Acceso SSH'
sudo ufw allow 80/tcp comment 'HTTP Nginx Frontend SPA'
sudo ufw allow 443/tcp comment 'HTTPS SSL'
sudo ufw allow 5000/tcp comment 'Backend API REST .NET 10 (Interno/Reverse Proxy)'

# 2. Activar UFW y Fail2ban
sudo ufw enable
sudo systemctl enable --now fail2ban
```

---

### 3. DESPLIEGUE DEL BACKEND Y POSTGRESQL 18 (DOCKER COMPOSE)

#### 3.1. Clonar repositorio y navegar a la API
```bash
git clone https://github.com/JuanDiegoZz/ProyectoDeTicket.git /var/www/tickets
cd /var/www/tickets/Backend/API
```

#### 3.2. Estructura de `docker-compose.yml` para .NET 10 y PostgreSQL 18
```yaml
version: '3.8'

services:
  postgres_db:
    image: postgres:18-alpine
    container_name: tecnm_postgres_db
    restart: always
    environment:
      POSTGRES_DB: tickets_tecnm_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: TuPasswordSeguro2026!
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - tickets_network

  api_backend:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: tecnm_api_backend
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres_db;Database=tickets_tecnm_db;Username=postgres;Password=TuPasswordSeguro2026!
    ports:
      - "5000:5000"
    depends_on:
      - postgres_db
    networks:
      - tickets_network

volumes:
  postgres_data:

networks:
  tickets_network:
    driver: bridge
```

#### 3.3. Iniciar Contenedores Backend
```bash
docker compose up -d --build
docker compose ps
```

---

### 4. DESPLIEGUE DEL FRONTEND VUE 3 SPA (NGINX)

#### 4.1. Configuración de Nginx (`/etc/nginx/sites-available/tickets-frontend`)
```nginx
server {
    listen 80;
    server_name tickets.monclova.tecnm.mx;

    root /var/www/tickets/Frontend;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # Proxy inverso para la API REST .NET 10
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'keep-alive';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### 4.2. Activar sitio en Nginx y reiniciar
```bash
sudo ln -s /etc/nginx/sites-available/tickets-frontend /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

### 5. RESPALDOS AUTOMÁTICOS CRONTAB (POSTGRESQL 18)

Crear script de respaldo `/var/www/tickets/backup_db.sh`:
```bash
#!/bin/bash
FECHA=$(date +%Y%m%d_%H%M%S)
DIR_BACKUP="/var/backups/tickets_db"
mkdir -p $DIR_BACKUP
docker exec -t tecnm_postgres_db pg_dump -U postgres tickets_tecnm_db | gzip > $DIR_BACKUP/backup_$FECHA.sql.gz
find $DIR_BACKUP -type f -mtime +30 -delete
```

Hacer ejecutable y programar a las 2:00 AM diariamente:
```bash
chmod +x /var/www/tickets/backup_db.sh
(crontab -l 2>/dev/null; echo "0 2 * * * /var/var/tickets/backup_db.sh") | crontab -
```
