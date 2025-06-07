/**
 * Xử lý địa điểm giao xe khi thuê xe tự lái
 */
document.addEventListener('DOMContentLoaded', function () {
    // Khai báo biến cho bản đồ giao xe
    let deliveryMap;
    let deliveryMarker;

    // Các phần tử tùy chọn giao xe
    const ownerLocationOption = document.getElementById('ownerLocationOption');
    const requestDeliveryOption = document.getElementById('requestDeliveryOption');
    const deliveryAddressSection = document.getElementById('deliveryAddressSection');

    // Xử lý chuyển đổi tùy chọn giao xe
    if (ownerLocationOption && requestDeliveryOption) {
        ownerLocationOption.addEventListener('change', toggleDeliveryOption);
        requestDeliveryOption.addEventListener('change', toggleDeliveryOption);

        // Cập nhật hiển thị ban đầu
        toggleDeliveryOption();
    }

    // Hàm xử lý chuyển đổi tùy chọn giao xe
    function toggleDeliveryOption() {
        if (requestDeliveryOption && requestDeliveryOption.checked) {
            if (deliveryAddressSection) {
                deliveryAddressSection.classList.remove('d-none');
                setTimeout(() => initializeDeliveryMap(), 300); // Khởi tạo bản đồ sau khi hiển thị
            }

            // Đặt trạng thái yêu cầu giao xe
            setDeliveryRequestedState(true);
        } else {
            if (deliveryAddressSection) {
                deliveryAddressSection.classList.add('d-none');
            }

            // Đặt trạng thái không yêu cầu giao xe
            setDeliveryRequestedState(false);
        }

        // Tính toán lại phí thuê xe
        if (window.submitCalculateForm) {
            window.submitCalculateForm();
        }
    }

    // Hàm thiết lập trạng thái yêu cầu giao xe
    function setDeliveryRequestedState(isRequested) {
        // Tìm trường ẩn IsDeliveryRequested hoặc tạo mới nếu chưa có
        let hiddenField = document.querySelector('input[name="IsDeliveryRequested"]');
        if (!hiddenField) {
            hiddenField = document.createElement('input');
            hiddenField.type = 'hidden';
            hiddenField.name = 'IsDeliveryRequested';
            document.getElementById('rentalForm')?.appendChild(hiddenField);
        }

        hiddenField.value = isRequested ? 'true' : 'false';
    }

    // Khởi tạo bản đồ giao xe
    window.initializeDeliveryMap = function () {
        const mapContainer = document.getElementById('deliveryMap');
        if (!mapContainer) return;

        if (!deliveryMap) {
            deliveryMap = L.map('deliveryMap').setView([10.7623, 106.6827], 13);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(deliveryMap);

            // Xử lý click trên bản đồ
            deliveryMap.on('click', function (e) {
                const lat = e.latlng.lat;
                const lng = e.latlng.lng;

                document.getElementById('deliveryLatitude').value = lat;
                document.getElementById('deliveryLongitude').value = lng;

                // Thêm/cập nhật marker
                if (deliveryMarker) {
                    deliveryMarker.setLatLng([lat, lng]);
                } else {
                    deliveryMarker = L.marker([lat, lng]).addTo(deliveryMap);
                }

                // Tìm địa chỉ từ vị trí
                reverseGeocode(lat, lng, 'delivery');

                // Tính toán lại phí thuê xe
                if (window.submitCalculateForm) {
                    window.submitCalculateForm();
                }
            });

            // Khôi phục marker nếu đã có tọa độ
            const latInput = document.getElementById('deliveryLatitude');
            const lngInput = document.getElementById('deliveryLongitude');

            if (latInput?.value && lngInput?.value) {
                const lat = parseFloat(latInput.value);
                const lng = parseFloat(lngInput.value);

                deliveryMap.setView([lat, lng], 15);
                deliveryMarker = L.marker([lat, lng]).addTo(deliveryMap);
            }
        }

        // Cập nhật kích thước bản đồ khi hiển thị
        setTimeout(() => deliveryMap.invalidateSize(), 100);
    }

    // Xử lý nút lấy vị trí hiện tại cho điểm giao xe
    document.getElementById('deliveryCurrentLocation')?.addEventListener('click', function () {
        getCurrentLocation('delivery');
    });

    // Xử lý tìm kiếm địa chỉ giao xe
    document.getElementById('searchDeliveryLocation')?.addEventListener('click', function () {
        const address = document.getElementById('searchDeliveryAddress')?.value;
        if (address) {
            geocodeAddress(address, 'delivery');
        } else {
            alert('Vui lòng nhập địa chỉ để tìm kiếm.');
        }
    });

    // Hàm lấy vị trí hiện tại
    function getCurrentLocation(mapType) {
        if (mapType === 'delivery') {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(
                    function (position) {
                        const lat = position.coords.latitude;
                        const lng = position.coords.longitude;

                        document.getElementById('deliveryLatitude').value = lat;
                        document.getElementById('deliveryLongitude').value = lng;

                        if (deliveryMap) {
                            deliveryMap.setView([lat, lng], 15);

                            if (deliveryMarker) {
                                deliveryMarker.setLatLng([lat, lng]);
                            } else {
                                deliveryMarker = L.marker([lat, lng]).addTo(deliveryMap);
                            }

                            reverseGeocode(lat, lng, 'delivery');

                            // Tính toán lại phí thuê xe
                            if (window.submitCalculateForm) {
                                window.submitCalculateForm();
                            }
                        }
                    },
                    function (error) {
                        alert('Không thể lấy vị trí hiện tại. Vui lòng cho phép truy cập vị trí hoặc nhập địa chỉ thủ công.');
                        console.error('Lỗi định vị:', error);
                    }
                );
            } else {
                alert('Trình duyệt của bạn không hỗ trợ định vị.');
            }
        }
    }

    // Hàm tìm tọa độ từ địa chỉ
    function geocodeAddress(address, mapType) {
        if (mapType === 'delivery') {
            fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1&accept-language=vi`)
                .then(response => response.json())
                .then(data => {
                    if (data && data.length > 0) {
                        const lat = parseFloat(data[0].lat);
                        const lng = parseFloat(data[0].lon);
                        const displayName = data[0].display_name;

                        document.getElementById('deliveryLatitude').value = lat;
                        document.getElementById('deliveryLongitude').value = lng;
                        document.getElementById('DeliveryAddress').value = displayName;

                        if (deliveryMap) {
                            deliveryMap.setView([lat, lng], 15);

                            if (deliveryMarker) {
                                deliveryMarker.setLatLng([lat, lng]);
                            } else {
                                deliveryMarker = L.marker([lat, lng]).addTo(deliveryMap);
                            }

                            // Tính toán lại phí thuê xe
                            if (window.submitCalculateForm) {
                                window.submitCalculateForm();
                            }
                        }
                    } else {
                        alert('Không tìm thấy địa điểm. Vui lòng thử lại với từ khóa khác.');
                    }
                })
                .catch(error => {
                    console.error('Lỗi tìm kiếm địa chỉ:', error);
                    alert('Có lỗi xảy ra khi tìm kiếm địa chỉ.');
                });
        }
    }

    // Hàm tìm địa chỉ từ tọa độ
    function reverseGeocode(lat, lng, mapType) {
        if (mapType === 'delivery') {
            fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&accept-language=vi`)
                .then(response => response.json())
                .then(data => {
                    if (data && data.display_name) {
                        const addressEl = document.getElementById('DeliveryAddress');
                        if (addressEl) addressEl.value = data.display_name;
                    }
                })
                .catch(error => console.error('Lỗi tìm địa chỉ từ tọa độ:', error));
        }
    }
});