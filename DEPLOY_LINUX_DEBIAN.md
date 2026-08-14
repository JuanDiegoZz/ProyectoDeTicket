# 📘 MANUAL DE DESPLIEGUE Y ADMINISTRACIÓN EN PRODUCCIÓN
## Sistema de Gestión de Tickets de Soporte TI - TecNM Monclova
**Entorno de Producción:** Servidor Linux (Debian 12 "Bookworm" / Debian 13 "Trixie")  
**Arquitectura:** Contenedores Docker + Docker Compose + PostgreSQL 16 + ASP.NET MVC (.NET 8)

---

### 1. REQUISITOS PREVIOS EN EL SERVIDOR DEBIAN
Asegúrate de tener acceso SSH con permisos de `sudo` en tu servidor Debian.

#### 1.1. Actualizar el sistema e instalar dependencias básicas
```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y curl ufw git fail2ban apt-transport-https ca-certificates gnupg lsb-release
```

#### 1.2. Instalar Docker Engine y Docker Compose Oficial
```bash
# Agregar repositorio oficial de Docker
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Habilitar el servicio Docker
sudo systemctl enable --now docker
```

---

### 2. CONFIGURACIÓN DE SEGURIDAD Y FIREWALL (UFW + Fail2ban)
Para cumplir con el objetivo de **entorno seguro y prevención de ataques**:

```bash
# 1. Reglas por defecto: denegar tráfico entrante, permitir saliente
sudo ufw default deny incoming
sudo ufw default allow outgoing

# 2. Permitir acceso SSH (Puerto 22)
sudo ufw allow 22/tcp comment 'Acceso SSH Seguro'

# 3. Permitir el puerto de la aplicación Web (Puerto 8080 o HTTP 80)
sudo ufw allow 8080/tcp comment 'Sistema de Tickets TecNM'

# 4. Aislar PostgreSQL: NUNCA abrir el puerto 5432 a Internet.
# Docker maneja la comunicación interna de forma aislada a través de su red bridge 'tickets_network'.

# 5. Activar el Firewall
sudo ufw enable
sudo ufw status verbose

# 6. Activar Fail2ban para mitigar ataques de fuerza bruta
sudo systemctl enable --now fail2ban
```

---

### 3. DESPLIEGUE DEL SISTEMA CON DOCKER COMPOSE

#### 3.1. Copiar los archivos del proyecto al servidor
Copia la carpeta del proyecto a `/opt/tickets-tecnm` o clónala desde tu repositorio:
```bash
sudo mkdir -p /opt/tickets-tecnm
cd /opt/tickets-tecnm
```

#### 3.2. Estructura de archivos requerida en el servidor:
```text
/opt/tickets-tecnm/
├── Controllers/
├── Data/
├── Models/
├── Properties/
├── Services/
├── ViewModels/
├── Views/
├── wwwroot/
├── appsettings.json
├── docker-compose.yml
├── Dockerfile
└── TicketsApp.csproj
```

#### 3.3. Compilar y levantar los contenedores
Ejecuta en la terminal de Debian:
```bash
docker compose up -d --build
```

#### 3.4. Verificar que los servicios estén activos y saludables:
```bash
docker compose ps
docker compose logs -f web
```

---

### 4. GESTIÓN Y MANTENIMIENTO EN DEBIAN

#### Respaldo automático de la Base de Datos PostgreSQL:
```bash
# Realizar un backup de la base de datos a un archivo .sql
docker exec -t tickets_postgres_db pg_dump -U postgres tickets_db > backup_tickets_$(date +%Y%m%d).sql
```

#### Restauración de respaldo:
```bash
cat backup_tickets_20260814.sql | docker exec -i tickets_postgres_db psql -U postgres -d tickets_db
```

#### Reiniciar o actualizar la aplicación tras cambios de código:
```bash
docker compose down
docker compose up -d --build
```

---

### 5. ACCESO AL SISTEMA Y CREDENCIALES
- **URL de acceso:** `http://<IP_DEL_SERVIDOR_LINUX>:8080`
- **Administrador TI:** `admin@monclova.tecnm.mx` | `Admin123!`
- **Técnico de Soporte:** `carlos.tecnico@monclova.tecnm.mx` | `Tecnico123!`
- **Profesor:** `ruben.rr@monclova.tecnm.mx` | `Docente123!`
