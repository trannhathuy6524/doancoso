/**
 * Xử lý ngày tháng trong form đặt xe
 */
document.addEventListener('DOMContentLoaded', function () {
    // Đặt giá trị tối thiểu cho EndDate theo StartDate
    const startDateInput = document.getElementById('StartDate');
    if (startDateInput) {
        startDateInput.addEventListener('change', function () {
            const startDate = new Date(this.value);
            const nextDay = new Date(startDate);
            nextDay.setDate(nextDay.getDate() + 1);

            const endDateInput = document.getElementById('EndDate');
            if (endDateInput) {
                const formattedDate = nextDay.toISOString().split('T')[0];
                endDateInput.min = formattedDate;

                // Nếu EndDate nhỏ hơn StartDate + 1, cập nhật giá trị của EndDate
                if (new Date(endDateInput.value) <= startDate) {
                    endDateInput.value = formattedDate;
                }
            }
        });
    }

    // Xử lý modal xem ảnh lớn
    const imageModal = document.getElementById('imageModal');
    if (imageModal) {
        imageModal.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var imgSrc = button.getAttribute('data-bs-src');
            var modalImg = document.getElementById('modalImage');
            if (modalImg) {
                modalImg.src = imgSrc;
            }
        });
    }
});