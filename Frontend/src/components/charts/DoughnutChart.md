# DoughnutChart.js — Componente de Gráfica de Dona (vue-chartjs)

## 1. Qué Representa
Representa proporciones y distribuciones porcentuales en forma de dona. En el sistema se utiliza para visualizar la proporción de tickets por estado (**Abiertos, En Progreso, Resueltos**).

---

## 2. Dependencias
- **Vue 3**: `h` render function.
- **vue-chartjs**: `Doughnut` component wrapper.
- **Chart.js**: Registrador de controladores y elementos de dona.

---

## 3. Props
| Prop | Tipo | Requerido | Descripción |
| :--- | :--- | :--- | :--- |
| `chartData` | `Object` | Sí | Objeto con `labels` y `datasets` formateados según la especificación de Chart.js. |
| `chartOptions` | `Object` | No | Objeto opcional para personalizar leyenda, recortes (`cutout`), tooltips y responsividad. |

---

## 4. Ejemplo de Uso
```javascript
import DoughnutChart from './components/charts/DoughnutChart.js';

// En template Vue 3:
// <doughnut-chart :chart-data="datosEstado" />
```

---

## 5. Endpoint Utilizado
- **GET `/api/tickets/dashboard-admin`**: Proporciona los totales de tickets abiertos, en progreso y resueltos.
