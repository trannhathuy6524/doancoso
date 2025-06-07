/**
 * Xử lý điểm đón và trả khách khi thuê xe có tài xế
 */
document.addEventListener('DOMContentLoaded', function () {
    // Khai báo biến cho bản đồ
    let pickupMap;
    let pickupMarker;
    let dropoffMap;
    let dropoffMarker;

    // Xử lý checkbox "Điểm trả giống điểm đón"
    const sameLocationCheckbox = document.getElementById('sameLocationCheckbox');
    const dropoffLocationSection = document.getElementById('dropoffLocationSection');

    if (sameLocationCheckbox && dropoffLocationSection) {
        sameLocationCheckbox.addEventListener('change', function () {
            if (this.checked) {
                dropoffLocationSection.classList.add('d-none');

                // Copy giá trị từ điểm đón sang điểm trả
                copyPickupToDropoff();
            } else {
                dropoffLocationSection.classList.remove('d-none');
                setTimeout(() => initializeDropoffMap(), 300);
            }
        });
    }

    // Khởi tạo bản đồ điểm đón
    window.initializePickupMap = function () {
        const mapContainer = document.getElementById('pickupMap');
        if (!mapContainer) return;

        if (!pickupMap) {
            pickupMap = L.map('pickupMap').setView([10.7623, 106.6827], 13);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(pickupMap);

            // Xử lý click trên bản đồ
            pickupMap.on('click', function (e) {
                const lat = e.latlng.lat;
                const lng = e.latlng.lng;

                document.getElementById('pickupLatitude').value = lat;
                document.getElementById('pickupLongitude').value = lng;

                // Thêm/cập nhật marker
                if (pickupMarker) {
                    pickupMarker.setLatLng([lat, lng]);
                } else {
                    pickupMarker = L.marker([lat, lng]).addTo(pickupMap);
                }

                // Tìm địa chỉ từ vị trí
                reverseGeocode(lat, lng, 'pickup');

                // Nếu chọn điểm trả giống điểm đón, cập nhật điểm trả
                if (sameLocationCheckbox?.checked) {
                    copyPickupToDropoff();
                }
            });

            // Khôi phục marker nếu đã có tọa độ
            const latInput = document.getElementById('pickupLatitude');
            const lngInput = document.getElementById('pickupLongitude');

            if (latInput?.value && lngInput?.value) {
                const lat = parseFloat(latInput.value);
                const lng = parseFloat(lngInput.value);

                if (!isNaN(lat) && !isNaN(lng)) {
                    pickupMap.setView([lat, lng], 15);
                    pickupMarker = L.marker([lat, lng]).addTo(pickupMap);
                }
            }
        }

        // Cập nhật kích thước bản đồ khi hiển thị
        setTimeout(() => pickupMap.invalidateSize(), 100);
    }

    // Khởi tạo bản đồ điểm trả
    window.initializeDropoffMap = function () {
        const mapContainer = document.getElementById('dropoffMap');
        if (!mapContainer) return;

        if (!dropoffMap) {
            dropoffMap = L.map('dropoffMap').setView([10.7623, 106.6827], 13);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(dropoffMap);

            // Xử lý click trên bản đồ
            dropoffMap.on('click', function (e) {
                const lat = e.latlng.lat;
                const lng = e.latlng.lng;

                document.getElementById('dropoffLatitude').value = lat;
                document.getElementById('dropoffLongitude').value = lng;

                // Thêm/cập nhật marker
                if (dropoffMarker) {
                    dropoffMarker.setLatLng([lat, lng]);
                } else {
                    dropoffMarker = L.marker([lat, lng]).addTo(dropoffMap);
                }

                // Tìm địa chỉ từ vị trí
                reverseGeocode(lat, lng, 'dropoff');
            });

            // Khôi phục marker nếu đã có tọa độ
            const latInput = document.getElementById('dropoffLatitude');
            const lngInput = document.getElementById('dropoffLongitude');

            if (latInput?.value && lngInput?.value) {
                const lat = parseFloat(latInput.value);
                const lng = parseFloat(lngInput.value);

                if (!isNaN(lat) && !isNaN(lng)) {
                    dropoffMap.setView([lat, lng], 15);
                    dropoffMarker = L.marker([lat, lng]).addTo(dropoffMap);
                }
            }
        }

        // Cập nhật kích thước bản đồ khi hiển thị
        setTimeout(() => dropoffMap.invalidateSize(), 100);
    }

    // Sao chép giá trị từ điểm đón sang điểm trả
    function copyPickupToDropoff() {
        const pickupAddress = document.getElementById('PickupAddress');
        const dropoffAddress = document.getElementById('DropoffAddress');
        const pickupLat = document.getElementById('pickupLatitude');
        const pickupLng = document.getElementById('pickupLongitude');
        const dropoffLat = document.getElementById('dropoffLatitude');
        const dropoffLng = document.getElementById('dropoffLongitude');

        if (pickupAddress && dropoffAddress) {
            dropoffAddress.value = pickupAddress.value;
        }

        if (pickupLat && pickupLng && dropoffLat && dropoffLng) {
            dropoffLat.value = pickupLat.value;
            dropoffLng.value = pickupLng.value;
        }
    }

    // Xử lý nút lấy vị trí hiện tại cho điểm đón
    document.getElementById('pickupCurrentLocation')?.addEventListener('click', function () {
        getCurrentLocation('pickup');
    });

    // Xử lý nút lấy vị trí hiện tại cho điểm trả
    document.getElementById('dropoffCurrentLocation')?.addEventListener('click', function () {
        getCurrentLocation('dropoff');
    });

    // Xử lý tìm kiếm địa chỉ đón khách
    document.getElementById('searchPickupLocation')?.addEventListener('click', function () {
        const address = document.getElementById('searchPickupAddress')?.value;
        if (address) {
            geocodeAddress(address, 'pickup');
        } else {
            alert('Vui lòng nhập địa chỉ để tìm kiếm.');
        }
    });

    // Xử lý tìm kiếm địa chỉ trả khách
    document.getElementById('searchDropoffLocation')?.addEventListener('click', function () {
        const address = document.getElementById('searchDropoffAddress')?.value;
        if (address) {
            geocodeAddress(address, 'dropoff');
        } else {
            alert('Vui lòng nhập địa chỉ để tìm kiếm.');
        }
    });

    // Hàm lấy vị trí hiện tại
    function getCurrentLocation(mapType) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                function (position) {
                    const lat = position.coords.latitude;
                    const lng = position.coords.longitude;

                    if (mapType === 'pickup') {
                        document.getElementById('pickupLatitude').value = lat;
                        document.getElementById('pickupLongitude').value = lng;

                        if (pickupMap) {
                            pickupMap.setView([lat, lng], 15);

                            if (pickupMarker) {
                                pickupMarker.setLatLng([lat, lng]);
                            } else {
                                pickupMarker = L.marker([lat, lng]).addTo(pickupMap);
                            }

                            reverseGeocode(lat, lng, 'pickup');

                            // Nếu chọn điểm trả giống điểm đón, cập nhật điểm trả
                            if (sameLocationCheckbox?.checked) {
                                copyPickupToDropoff();
                            }
                        }
                    } else if (mapType === 'dropoff') {
                        document.getElementById('dropoffLatitude').value = lat;
                        document.getElementById('dropoffLongitude').value = lng;

                        if (dropoffMap) {
                            dropoffMap.setView([lat, lng], 15);

                            if (dropoffMarker) {
                                dropoffMarker.setLatLng([lat, lng]);
                            } else {
                                dropoffMarker = L.marker([lat, lng]).addTo(dropoffMap);
                            }

                            reverseGeocode(lat, lng, 'dropoff');
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

    // Hàm tìm tọa độ từ địa chỉ
    function geocodeAddress(address, mapType) {
        fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1&accept-language=vi`)
            .then(response => response.json())
            .then(data => {
                if (data && data.length > 0) {
                    const lat = parseFloat(data[0].lat);
                    const lng = parseFloat(data[0].lon);
                    const displayName = data[0].display_name;

                    if (mapType === 'pickup') {
                        document.getElementById('pickupLatitude').value = lat;
                        document.getElementById('pickupLongitude').value = lng;
                        document.getElementById('PickupAddress').value = displayName;

                        if (pickupMap) {
                            pickupMap.setView([lat, lng], 15);

                            if (pickupMarker) {
                                pickupMarker.setLatLng([lat, lng]);
                            } else {
                                pickupMarker = L.marker([lat, lng]).addTo(pickupMap);
                            }
                        }

                        // Nếu chọn điểm trả giống điểm đón, cập nhật điểm trả
                        if (sameLocationCheckbox?.checked) {
                            copyPickupToDropoff();
                        }
                    } else if (mapType === 'dropoff') {
                        document.getElementById('dropoffLatitude').value = lat;
                        document.getElementById('dropoffLongitude').value = lng;
                        document.getElementById('DropoffAddress').value = displayName;

                        if (dropoffMap) {
                            dropoffMap.setView([lat, lng], 15);

                            if (dropoffMarker) {
                                dropoffMarker.setLatLng([lat, lng]);
                            } else {
                                dropoffMarker = L.marker([lat, lng]).addTo(dropoffMap);
                            }
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

    // Hàm tìm địa chỉ từ tọa độ
    function reverseGeocode(lat, lng, mapType) {
        fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&accept-language=vi`)
            .then(response => response.json())
            .then(data => {
                if (data && data.display_name) {
                    if (mapType === 'pickup') {
                        const addressEl = document.getElementById('PickupAddress');
                        if (addressEl) addressEl.value = data.display_name;
                    } else if (mapType === 'dropoff') {
                        const addressEl = document.getElementById('DropoffAddress');
                        if (addressEl) addressEl.value = data.display_name;
                    }
                }
            })
            .catch(error => console.error('Lỗi tìm địa chỉ từ tọa độ:', error));
    }

    // Lắng nghe sự kiện thay đổi điểm đón để cập nhật điểm trả nếu checkbox được chọn
    document.getElementById('PickupAddress')?.addEventListener('change', function () {
        if (sameLocationCheckbox?.checked) {
            copyPickupToDropoff();
        }
    });

    document.getElementById('pickupLatitude')?.addEventListener('change', function () {
        if (sameLocationCheckbox?.checked) {
            copyPickupToDropoff();
        }
    });

    document.getElementById('pickupLongitude')?.addEventListener('change', function () {
        if (sameLocationCheckbox?.checked) {
            copyPickupToDropoff();
        }
    });
});