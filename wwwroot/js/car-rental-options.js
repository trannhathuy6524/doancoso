/**
 * Xử lý các tùy chọn thuê xe
 */
document.addEventListener('DOMContentLoaded', function () {
    // Các selector cho phần dịch vụ tài xế
    const serviceSelfDrive = document.getElementById('serviceSelfDrive');
    const serviceWithDriver = document.getElementById('serviceWithDriver');
    const selfDriveCard = document.getElementById('selfDriveCard');
    const withDriverCard = document.getElementById('withDriverCard');
    const driverServiceOptions = document.getElementById('driverServiceOptions');

    // Các selector cho phần loại hình di chuyển
    const tripLocalTrip = document.getElementById('tripLocalTrip');
    const tripInterCityRoundTrip = document.getElementById('tripInterCityRoundTrip');
    const tripInterCityOneWay = document.getElementById('tripInterCityOneWay');
    const distanceSection = document.getElementById('distanceSection');
    const oneWayNote = document.getElementById('oneWayNote');
    const calculateButton = document.querySelector('button[type="submit"]');

    // Xử lý khi click vào card tự lái
    if (selfDriveCard) {
        selfDriveCard.addEventListener('click', function () {
            if (serviceSelfDrive) {
                serviceSelfDrive.checked = true;
                updateServiceSelection();
            }
        });
    }

    // Xử lý khi click vào card có tài xế
    if (withDriverCard) {
        withDriverCard.addEventListener('click', function () {
            if (serviceWithDriver) {
                serviceWithDriver.checked = true;
                updateServiceSelection();
            }
        });
    }

    // Xử lý khi thay đổi loại dịch vụ
    if (serviceSelfDrive) {
        serviceSelfDrive.addEventListener('change', updateServiceSelection);
    }

    if (serviceWithDriver) {
        serviceWithDriver.addEventListener('change', updateServiceSelection);
    }

    // Xử lý khi thay đổi loại chuyến đi
    if (tripLocalTrip) {
        tripLocalTrip.addEventListener('change', updateTripType);
    }

    if (tripInterCityRoundTrip) {
        tripInterCityRoundTrip.addEventListener('change', updateTripType);
    }

    if (tripInterCityOneWay) {
        tripInterCityOneWay.addEventListener('change', updateTripType);
    }

    // Xử lý khi thay đổi khoảng cách
    const estimatedDistance = document.getElementById('EstimatedDistance');
    if (estimatedDistance) {
        estimatedDistance.addEventListener('change', function () {
            if (calculateButton) {
                calculateButton.click();
            }
        });
    }

    // Xử lý chuyển đổi giữa thuê theo ngày và theo giờ
    const rentalTypeDay = document.getElementById('rentalTypeDay');
    const rentalTypeHour = document.getElementById('rentalTypeHour');
    const dayRentalSection = document.getElementById('dayRentalSection');
    const hourRentalSection = document.getElementById('hourRentalSection');

    if (rentalTypeDay && dayRentalSection && hourRentalSection) {
        rentalTypeDay.addEventListener('change', function () {
            if (this.checked) {
                dayRentalSection.style.display = '';
                hourRentalSection.style.display = 'none';
            }
        });
    }

    if (rentalTypeHour && dayRentalSection && hourRentalSection) {
        rentalTypeHour.addEventListener('change', function () {
            if (this.checked) {
                dayRentalSection.style.display = 'none';
                hourRentalSection.style.display = '';
            }
        });
    }

    /**
     * Hàm cập nhật hiển thị theo loại dịch vụ
     */
    function updateServiceSelection() {
        const isWithDriver = serviceWithDriver && serviceWithDriver.checked;

        // Cập nhật style cho card
        if (selfDriveCard && withDriverCard) {
            if (isWithDriver) {
                selfDriveCard.classList.remove('border-primary', 'bg-light');
                withDriverCard.classList.add('border-primary', 'bg-light');
            } else {
                selfDriveCard.classList.add('border-primary', 'bg-light');
                withDriverCard.classList.remove('border-primary', 'bg-light');
            }
        }

        // Hiển thị/ẩn tùy chọn dịch vụ tài xế
        if (driverServiceOptions) {
            if (isWithDriver) {
                driverServiceOptions.classList.remove('d-none');
            } else {
                driverServiceOptions.classList.add('d-none');
            }
        }

        // Tự động tính lại giá
        if (calculateButton) {
            calculateButton.click();
        }
    }

    /**
     * Hàm cập nhật hiển thị theo loại chuyến đi
     */
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

        // Tự động tính lại giá
        if (calculateButton && !isLocalTrip) {
            calculateButton.click();
        }
    }

    // Khởi tạo hiển thị ban đầu
    updateServiceSelection();
    updateTripType();
});