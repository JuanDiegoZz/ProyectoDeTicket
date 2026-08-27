# Filtros de Reportes, Gráficas y Exportación en Excel (CSV)

## 1. Descripción General
El sistema cuenta con un motor unificado de filtrado procesado directamente en el **Backend (.NET 10 + PostgreSQL 18)**. Esto garantiza que:
1. Las **Gráficas (`vue-chartjs` + `Chart.js`)** se actualicen dinámicamente según el conjunto de datos filtrado.
2. La **Tabla Paginada** muestre exactamente los registros coincidentes.
3. La **Exportación a Excel / CSV** descargue exclusivamente las filas resultantes de los mismos filtros activos.

---

## 2. Parámetros de Filtro Soportados

| Parámetro | Tipo | Ejemplo / Formato | Descripción |
| :--- | :--- | :--- | :--- |
| `busqueda` | `string` | `"E1"` o `"Hardware"` | Búsqueda por texto libre en folio, asunto, descripción, solicitante o aula. |
| `fechaInicio` | `string (ISO Date)` | `"2026-08-01"` | Filtra tickets creados desde las 00:00:00 UTC de la fecha elegida. |
| `fechaFin` | `string (ISO Date)` | `"2026-08-27"` | Filtra tickets creados hasta las 23:59:59 UTC de la fecha elegida. |
| `ubicacionId` | `int` | `1` (Edificio E1) | Filtra por el identificador del edificio / ubicación. |
| `categoriaId` | `int` | `2` (Redes) | Filtra por la categoría del problema o falla de TI. |
| `estado` | `Enum / String` | `"Abierto"`, `"Resuelto"` | Filtra por el estado actual del ticket. |
| `prioridad` | `Enum / String` | `"Alta"`, `"Urgente"` | Filtra por el nivel de prioridad asignado. |
| `orden` | `string` | `"prioridad_desc"` | Define el criterio de ordenamiento (`recientes`, `antiguos`, `folio_asc`, `prioridad_desc`). |

---

## 3. Endpoints de la API REST Consumidos

### 3.1 `GET /api/tickets/dashboard-admin`
Devuelve el desglose de métricas e indicadores de las gráficas ajustado a los filtros.
- **Query Params**: `busqueda`, `estado`, `prioridad`, `categoriaId`, `ubicacionId`, `fechaInicio`, `fechaFin`.
- **Respuesta JSON**:
```json
{
  "totalTickets": 42,
  "ticketsAbiertos": 10,
  "ticketsEnProgreso": 12,
  "ticketsResueltos": 20,
  "fallasPorUbicacion": [
    { "ubicacion": "Edificio E1", "cantidad": 15 }
  ],
  "fallasPorCategoria": [
    { "categoria": "Hardware", "cantidad": 27 }
  ]
}
```

### 3.2 `GET /api/tickets/exportar-csv`
Genera un archivo `.csv` codificado en **UTF-8 con BOM** para compatibilidad con Microsoft Excel.
- **Query Params**: `busqueda`, `estado`, `prioridad`, `categoriaId`, `ubicacionId`, `fechaInicio`, `fechaFin`, `orden`.

---

## 4. Limpieza de Filtros
El sistema ofrece la función `limpiarFiltros()` que restablece todos los controles a su valor por defecto e invoca de nuevo la APIREST para desplegar el panorama general del campus.
