import { h } from 'vue';
import { Bar } from 'vue-chartjs';

export default {
  name: 'BarChart',
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
          legend: { display: false }
        }
      })
    }
  },
  render() {
    return h(Bar, {
      data: this.chartData,
      options: this.chartOptions
    });
  }
};
