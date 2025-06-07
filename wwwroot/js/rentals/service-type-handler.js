/**
 * Xử lý lựa chọn loại dịch vụ thuê xe (tự lái hoặc có tài xế)
 */
document.addEventListener('DOMContentLoaded', function () {
    // Lấy các phần tử DOM cần thiết
    const selfDriveCard = document.getElementById('selfDriveCard');
    const withDriverCard = document.getElementById('withDriverCard');
    const serviceSelfDrive = document.getElementById('serviceSelfDrive');
    const serviceWithDriver = document.getElementById('serviceWithDriver');
    const driverServiceOptions = document.getElementById('driverServiceOptions');
    const driverFeeInfo = document.getElementById('driverFeeInfo');
    const driverFeeText = document.getElementById('driverFeeText');

    // Xử lý khi click vào card "Tự lái xe"
    if (selfDriveCard) {
        selfDriveCard.addEventListener('click', function () {
            if (serviceSelfDrive) {
                // Đánh dấu radio button và thay đổi style card
                serviceSelfDrive.checked = true;
                selfDriveCard.classList.add('selected');
                withDriverCard.classList.remove('selected');

                // Ẩn tùy chọn tài xế
                if (driverServiceOptions) {
                    driverServiceOptions.classList.add('d-none');
                }

                // Ẩn thông tin phí tài xế
                if (driverFeeInfo) {
                    driverFeeInfo.style.display = 'none';
                }

                // Tính lại giá
                if (window.submitCalculateForm) {
                    window.submitCalculateForm();
                }

                // Cập nhật hiển thị các tùy chọn địa điểm
                updateLocationOptions();
            }
        });
    }

    // Xử lý khi click vào card "Có tài xế"
    if (withDriverCard) {
        withDriverCard.addEventListener('click', function () {
            if (serviceWithDriver) {
                // Đánh dấu radio button và thay đổi style card
                serviceWithDriver.checked = true;
                withDriverCard.classList.add('selected');
                selfDriveCard.classList.remove('selected');

                // Hiện tùy chọn tài xế
                if (driverServiceOptions) {
                    driverServiceOptions.classList.remove('d-none');
                }

                // Cập nhật hiển thị phí tài xế
                updateDriverFeeDisplay();

                // Tính lại giá
                if (window.submitCalculateForm) {
                    window.submitCalculateForm();
                }

                // Cập nhật hiển thị các tùy chọn địa điểm
                updateLocationOptions();
            }
        });
    }

    // Xử lý khi radio button "Tự lái xe" thay đổi
    if (serviceSelfDrive) {
        serviceSelfDrive.addEventListener('change', function () {
            if (this.checked) {
                // Ẩn tùy chọn tài xế
                if (driverServiceOptions) {
                    driverServiceOptions.classList.add('d-none');
                }

                // Cập nhật style card
                if (selfDriveCard) selfDriveCard.classList.add('selected');
                if (withDriverCard) withDriverCard.classList.remove('selected');

                // Ẩn thông tin phí tài xế
                if (driverFeeInfo) {
                    driverFeeInfo.style.display = 'none';
                }

                // Tính lại giá
                if (window.submitCalculateForm) {
                    window.submitCalculateForm();
                }

                // Cập nhật hiển thị các tùy chọn địa điểm
                updateLocationOptions();
            }
        });
    }

    // Xử lý khi radio button "Có tài xế" thay đổi
    if (serviceWithDriver) {
        serviceWithDriver.addEventListener('change', function () {
            if (this.checked) {
                // Hiện tùy chọn tài xế
                if (driverServiceOptions) {
                    driverServiceOptions.classList.remove('d-none');
                }

                // Cập nhật style card
                if (withDriverCard) withDriverCard.classList.add('selected');
                if (selfDriveCard) selfDriveCard.classList.remove('selected');

                // Cập nhật hiển thị phí tài xế
                updateDriverFeeDisplay();

                // Tính lại giá
                if (window.submitCalculateForm) {
                    window.submitCalculateForm();
                }

                // Cập nhật hiển thị các tùy chọn địa điểm
                updateLocationOptions();
            }
        });
    }

    // Hàm cập nhật hiển thị phí tài xế dựa trên loại thuê
    window.updateDriverFeeDisplay = function () {
        const serviceWithDriver = document.getElementById('serviceWithDriver');
        const driverFeeInfo = document.getElementById('driverFeeInfo');
        const driverFeeText = document.getElementById('driverFeeText');
        const estimatedDistance = document.getElementById('estimatedDistance');
        const tripInterCityOneWay = document.getElementById('tripInterCityOneWay');
        const tripInterCityRoundTrip = document.getElementById('tripInterCityRoundTrip');

        // Nếu không có dịch vụ tài xế hoặc không được chọn, ẩn phí
        if (!serviceWithDriver || !serviceWithDriver.checked) {
            if (driverFeeInfo) driverFeeInfo.style.display = 'none';
            return;
        }

        // Hiển thị thông tin phí tài xế
        if (driverFeeInfo) driverFeeInfo.style.display = 'block';
        if (!driverFeeText) return;

        // Xác định loại thuê và tính phí tương ứng
        let feeText = '';
        const rentalTypeDay = document.getElementById('rentalTypeDay');

        if (rentalTypeDay && rentalTypeDay.checked) {
            // Thuê theo ngày
            const days = parseInt(document.querySelector('.price-item:nth-child(2) span:last-child')?.textContent) || 1;
            const driverFeePerDay = parseFloat(document.getElementById('driverFeePerDay')?.value) || 0;
            const totalDriverFee = driverFeePerDay * days;
            feeText = `Phí tài xế: <span class="fw-bold">${totalDriverFee.toLocaleString()} VNĐ</span> (${driverFeePerDay.toLocaleString()} VNĐ/ngày × ${days} ngày)`;
        } else {
            // Thuê theo giờ
            const hours = parseInt(document.querySelector('.price-item:nth-child(2) span:last-child')?.textContent) || 1;
            const driverFeePerHour = parseFloat(document.getElementById('driverFeePerHour')?.value) || 0;
            const totalDriverFee = driverFeePerHour * hours;
            feeText = `Phí tài xế: <span class="fw-bold">${totalDriverFee.toLocaleString()} VNĐ</span> (${driverFeePerHour.toLocaleString()} VNĐ/giờ × ${hours} giờ)`;
        }

        // Thêm phí khoảng cách nếu có
        if (estimatedDistance &&
            ((tripInterCityOneWay && tripInterCityOneWay.checked) ||
                (tripInterCityRoundTrip && tripInterCityRoundTrip.checked))) {
            const distance = parseFloat(estimatedDistance.value) || 0;
            if (distance > 0) {
                // Phí 5.000 VNĐ/km
                const kmFee = 5000;
                const distanceFee = distance * kmFee;
                const multiplier = (tripInterCityOneWay && tripInterCityOneWay.checked) ? 2 : 1;
                const totalDistanceFee = distanceFee * multiplier;

                feeText += `<br>Phí khoảng cách: <span class="fw-bold">${totalDistanceFee.toLocaleString()} VNĐ</span>`;
                feeText += ` (${kmFee.toLocaleString()} VNĐ/km × ${distance} km`;
                if (multiplier > 1) {
                    feeText += ` × 2 chiều`;
                }
                feeText += `)`;
            }
        }

        // Cập nhật nội dung hiển thị
        if (driverFeeText) {
            driverFeeText.innerHTML = feeText;
        }
    }

    // Hàm cập nhật hiển thị cho các tùy chọn địa điểm
    function updateLocationOptions() {
        const isWithDriver = serviceWithDriver && serviceWithDriver.checked;
        const selfDriveLocationOptions = document.getElementById('selfDriveLocationOptions');
        const withDriverLocationOptions = document.getElementById('withDriverLocationOptions');

        if (selfDriveLocationOptions && withDriverLocationOptions) {
            if (isWithDriver) {
                selfDriveLocationOptions.classList.add('d-none');
                withDriverLocationOptions.classList.remove('d-none');

                // Khởi tạo bản đồ điểm đón sau khi hiển thị
                setTimeout(function () {
                    if (window.initializePickupMap) {
                        window.initializePickupMap();
                    }

                    // Khởi tạo bản đồ điểm trả nếu hiển thị
                    if (!document.getElementById('sameLocationCheckbox')?.checked && window.initializeDropoffMap) {
                        window.initializeDropoffMap();
                    }
                }, 300);
            } else {
                selfDriveLocationOptions.classList.remove('d-none');
                withDriverLocationOptions.classList.add('d-none');

                // Chỉ khởi tạo bản đồ giao xe nếu tùy chọn giao xe được chọn
                if (document.getElementById('requestDeliveryOption')?.checked) {
                    setTimeout(function () {
                        if (window.initializeDeliveryMap) {
                            window.initializeDeliveryMap();
                        }
                    }, 300);
                }
            }
        }
    }

    // Thực hiện cập nhật hiển thị ban đầu
    updateLocationOptions();
    if (serviceWithDriver && serviceWithDriver.checked && driverServiceOptions) {
        driverServiceOptions.classList.remove('d-none');
        updateDriverFeeDisplay();
    }
});