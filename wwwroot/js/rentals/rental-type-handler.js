/**
 * Xử lý lựa chọn loại hình thuê xe (theo ngày hoặc theo giờ)
 */
document.addEventListener('DOMContentLoaded', function () {
    console.log("Rental type handler initialized");

    const rentalTypeDay = document.getElementById('rentalTypeDay');
    const rentalTypeHour = document.getElementById('rentalTypeHour');
    const dayRentalSection = document.getElementById('dayRentalSection');
    const hourRentalSection = document.getElementById('hourRentalSection');

    function toggleRentalSections() {
        console.log("Toggle rental sections. Day rental checked:", rentalTypeDay?.checked);

        if (rentalTypeDay?.checked) {
            // Hiện phần thuê theo ngày, ẩn phần thuê theo giờ
            if (dayRentalSection) dayRentalSection.style.display = 'block';
            if (hourRentalSection) hourRentalSection.style.display = 'none';
        } else {
            // Hiện phần thuê theo giờ, ẩn phần thuê theo ngày
            if (dayRentalSection) dayRentalSection.style.display = 'none';
            if (hourRentalSection) hourRentalSection.style.display = 'block';

            // Đặt giá trị mặc định cho giờ thuê
            setDefaultTimes();
        }

        // Cập nhật hiển thị phí tài xế
        if (window.updateDriverFeeDisplay) {
            window.updateDriverFeeDisplay();
        }

        // Tính lại giá
        if (window.submitCalculateForm) {
            window.submitCalculateForm();
        }
    }

    // Đặt giá trị mặc định cho giờ bắt đầu và kết thúc
    function setDefaultTimes() {
        const startTimeInput = document.getElementById('StartTime');
        const endTimeInput = document.getElementById('EndTime');

        if (startTimeInput && !startTimeInput.value) {
            startTimeInput.value = '08:00';
        }
        if (endTimeInput && !endTimeInput.value) {
            endTimeInput.value = '22:00';
        }
    }

    // Xử lý khi ngày bắt đầu thay đổi
    document.getElementById('StartDate')?.addEventListener('change', function () {
        const startDate = new Date(this.value);
        const endDateInput = document.getElementById('EndDate');

        // Đảm bảo ngày kết thúc ít nhất là 1 ngày sau ngày bắt đầu
        const nextDay = new Date(startDate);
        nextDay.setDate(nextDay.getDate() + 1);

        const formattedDate = nextDay.toISOString().split('T')[0];
        endDateInput.min = formattedDate;

        // Nếu ngày kết thúc hiện tại nhỏ hơn hoặc bằng ngày bắt đầu mới, cập nhật nó
        if (new Date(endDateInput.value) <= startDate) {
            endDateInput.value = formattedDate;
        }

        // Tự động submit form để tính lại giá
        if (window.submitCalculateForm) {
            window.submitCalculateForm();
        }
    });

    // Xử lý khi ngày kết thúc thay đổi
    document.getElementById('EndDate')?.addEventListener('change', function () {
        // Tự động submit form để tính lại giá
        if (window.submitCalculateForm) {
            window.submitCalculateForm();
        }
    });

    // Xử lý khi giờ bắt đầu thay đổi
    document.getElementById('StartTime')?.addEventListener('change', function () {
        // Tự động submit form để tính lại giá
        if (window.submitCalculateForm) {
            window.submitCalculateForm();
        }
    });

    // Xử lý khi ngày thuê theo giờ thay đổi
    document.getElementById('RentalDate')?.addEventListener('change', function () {
        // Tự động submit form để tính lại giá
        if (window.submitCalculateForm) {
            window.submitCalculateForm();
        }
    });

    // Xử lý khi giờ kết thúc thay đổi
    document.getElementById('EndTime')?.addEventListener('change', function () {
        // Tự động submit form để tính lại giá
        if (window.submitCalculateForm) {
            window.submitCalculateForm();
        }
    });

    // Gắn sự kiện cho các radio button loại thuê
    if (rentalTypeDay) {
        rentalTypeDay.addEventListener('change', function () {
            console.log("Day rental type selected");
            toggleRentalSections();
        });
    }

    if (rentalTypeHour) {
        rentalTypeHour.addEventListener('change', function () {
            console.log("Hour rental type selected");
            toggleRentalSections();
        });
    }

    // Khởi tạo hiển thị lần đầu
    toggleRentalSections();
    setDefaultTimes();
});