import { h } from 'vue';
import { Doughnut } from 'vue-chartjs';

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
  render() {
    return h(Doughnut, {
      data: this.chartData,
      options: this.chartOptions
    });
  }
};
