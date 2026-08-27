export default {
  name: 'ToastContainer',
  data() {
    return {
      toasts: []
    };
  },
  created() {
    window.addEventListener('show-toast', (e) => {
      if (e.detail) {
        this.addToast(e.detail.mensaje, e.detail.tipo || 'success');
      }
    });
  },
  methods: {
    addToast(mensaje, tipo = 'success') {
      const id = Date.now();
      this.toasts.push({ id, mensaje, tipo });
      setTimeout(() => {
        this.removeToast(id);
      }, 4000);
    },
    removeToast(id) {
      this.toasts = this.toasts.filter(t => t.id !== id);
    }
  },
  template: `
    <div class="toast-container position-fixed bottom-0 end-0 p-3" style="z-index: 1090;">
      <div v-for="t in toasts" :key="t.id" class="toast show align-items-center border-0 mb-2 shadow-lg" :class="t.tipo === 'success' ? 'bg-success text-white' : 'bg-danger text-white'" role="alert">
        <div class="d-flex">
          <div class="toast-body fw-bold">
            <i class="bi me-2" :class="t.tipo === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'"></i>
            {{ t.mensaje }}
          </div>
          <button type="button" @click="removeToast(t.id)" class="btn-close btn-close-white me-2 m-auto"></button>
        </div>
      </div>
    </div>
  `
};

export function showToast(mensaje, tipo = 'success') {
  window.dispatchEvent(new CustomEvent('show-toast', { detail: { mensaje, tipo } }));
}
