/**
 * Xử lý lựa chọn loại hình di chuyển khi thuê xe có tài xế
 */
document.addEventListener('DOMContentLoaded', function () {
    // Xử lý click vào các lựa chọn loại hình di chuyển
    const tripLocalTrip = document.getElementById('tripLocalTrip');
    const tripInterCityRoundTrip = document.getElementById('tripInterCityRoundTrip');
    const tripInterCityOneWay = document.getElementById('tripInterCityOneWay');
    const distanceSection = document.getElementById('distanceSection');
    const oneWayNote = document.getElementById('oneWayNote');

    function updateTripType() {
        const isLocalTrip = tripLocalTrip && tripLocalTrip.checked;
        const isInterCityOneWay = tripInterCityOneWay && tripInterCityOneWay.checked;

        // Hiển thị/ẩn phần khoảng cách
        if (distanceSection) {
            if (isLocalTrip) {
                distanceSection.classList.add('d-none');
            } else {
                distanceSection.classList.remove('d-none');
            }
        }

        // Hiển thị/ẩn ghi chú chuyến đi một chiều
        if (oneWayNote) {
            if (isInterCityOneWay) {
                oneWayNote.classList.remove('d-none');
            } else {
                oneWayNote.classList.add('d-none');
            }
        }

        // Cập nhật hiển thị phí tài xế
        if (window.updateDriverFeeDisplay) {
            window.updateDriverFeeDisplay();
        }

        // Tự động tính lại giá
        if (window.submitCalculateForm && !isLocalTrip) {
            window.submitCalculateForm();
        }
    }

    // Xử lý khi thay đổi loại chuyến đi
    if (tripLocalTrip) tripLocalTrip.addEventListener('change', updateTripType);
    if (tripInterCityRoundTrip) tripInterCityRoundTrip.addEventListener('change', updateTripType);
    if (tripInterCityOneWay) tripInterCityOneWay.addEventListener('change', updateTripType);

    // Xử lý khi thay đổi khoảng cách ước tính
    const estimatedDistance = document.getElementById('estimatedDistance');
    if (estimatedDistance) {
        estimatedDistance.addEventListener('input', function () {
            // Cập nhật hiển thị phí tài xế
            if (window.updateDriverFeeDisplay) {
                window.updateDriverFeeDisplay();
            }

            // Tính lại giá
            if (window.submitCalculateForm) {
                window.submitCalculateForm();
            }
        });
    }

    // Khởi tạo hiển thị ban đầu
    updateTripType();
});