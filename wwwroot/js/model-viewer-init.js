function initializeModelViewer(containerSelector, modelUrl, modelName) {
    const modelContainer = document.getElementById(containerSelector);
    
    if (modelContainer) {
        const script = document.createElement('script');
        script.type = 'module';
        script.src = 'https://unpkg.com/@google/model-viewer/dist/model-viewer.min.js';
        
        script.onload = () => {
            const modelViewer = document.createElement('model-viewer');
            modelViewer.src = modelUrl;
            modelViewer.alt = modelName;
            modelViewer.setAttribute('auto-rotate', '');
            modelViewer.setAttribute('camera-controls', '');
            modelViewer.setAttribute('shadow-intensity', '1');
            modelViewer.style.width = '100%';
            modelViewer.style.height = '100%';
            modelViewer.setAttribute('background-color', '#f8f9fa');

            // Thêm progress bar
            const progressContainer = document.createElement('div');
            progressContainer.className = 'progress position-absolute bottom-0 start-0 end-0 m-3';
            progressContainer.style.height = '5px';
            progressContainer.innerHTML = '<div class="progress-bar" role="progressbar" style="width: 0%"></div>';
            modelViewer.appendChild(progressContainer);

            // Thêm event listener cho progress
            modelViewer.addEventListener('progress', (e) => {
                const progressBar = progressContainer.querySelector('.progress-bar');
                if (progressBar) {
                    const fraction = e.detail.totalProgress * 100;
                    progressBar.style.width = `${fraction}%`;
                }
            });

            modelViewer.addEventListener('load', () => {
                progressContainer.remove();
            });

            modelContainer.innerHTML = '';
            modelContainer.appendChild(modelViewer);
        };

        script.onerror = () => {
            modelContainer.innerHTML = `
                <div class="alert alert-warning m-3 text-center">
                    <i class="bi bi-exclamation-circle fs-3 d-block mb-3"></i>
                    <h5>Không thể tải thư viện model-viewer</h5>
                    <p>Vui lòng kiểm tra kết nối mạng và thử lại.</p>
                </div>
            `;
        };

        document.head.appendChild(script);
    }
}
