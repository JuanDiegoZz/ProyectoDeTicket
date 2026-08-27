# BarChart.js — Componente de Gráfica de Barras (vue-chartjs)

## 1. Qué Representa
Representa comparativas cuantitativas mediante barras verticales u horizontales. En el sistema se utiliza para comparar:
1. **Fallas por Edificio / Ubicación** (Barras verticales).
2. **Top Fallas por Categoría** (Barras horizontales).

---

## 2. Dependencias
- **Vue 3**: `h` render function.
- **vue-chartjs**: `Bar` component wrapper.
- **Chart.js**: Registrador de controladores y elementos de barra.

---

## 3. Props
| Prop | Tipo | Requerido | Descripción |
| :--- | :--- | :--- | :--- |
| `chartData` | `Object` | Sí | Objeto con `labels` y `datasets` formateados según la especificación de Chart.js. |
| `chartOptions` | `Object` | No | Configuración de orientaciones (`indexAxis: 'y'`), leyendas y ejes. |

---

## 4. Ejemplo de Uso
```javascript
import BarChart from './components/charts/BarChart.js';

// En template Vue 3:
// <bar-chart :chart-data="datosUbicaciones" />
```

---

## 5. Endpoint Utilizado
- **GET `/api/tickets/dashboard-admin`**: Proporciona los listados agrupados `fallasPorUbicacion` y `fallasPorCategoria`.
