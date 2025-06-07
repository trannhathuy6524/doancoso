/**
 * Tập hợp và khởi tạo các module cho trang chi tiết xe
 */
document.addEventListener('DOMContentLoaded', function () {
    console.log('Car details bundle loaded');

    // Tải model viewer nếu có phần tử model3d-container
    if (document.getElementById('model3d-container') && typeof initializeModelViewer === 'function') {
        // initializeModelViewer được gọi từ trang Razor
    }
});