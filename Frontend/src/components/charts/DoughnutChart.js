import { ref, onMounted, onUnmounted, watch } from 'vue';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

export default {
  name: 'DoughnutChart',
  props: {
    chartData: {
      type: Object,
      required: true
    },
    chartOptions: {
      type: Object,
      default: () => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom' }
        },
        cutout: '70%'
      })
    }
  },
  setup(props) {
    const canvasRef = ref(null);
    let chartInstance = null;

    const renderChart = () => {
      if (chartInstance) {
        chartInstance.destroy();
      }
      if (canvasRef.value && props.chartData) {
        chartInstance = new Chart(canvasRef.value, {
          type: 'doughnut',
          data: props.chartData,
          options: props.chartOptions
        });
      }
    };

    onMounted(() => {
      renderChart();
    });

    onUnmounted(() => {
      if (chartInstance) {
        chartInstance.destroy();
      }
    });

    watch(
      () => props.chartData,
      () => {
        renderChart();
      },
      { deep: true }
    );

    return {
      canvasRef
    };
  },
  template: `<canvas ref="canvasRef"></canvas>`
};
