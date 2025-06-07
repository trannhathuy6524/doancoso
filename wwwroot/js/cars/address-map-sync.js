/**
 * Module đồng bộ hóa giữa địa chỉ và tọa độ trên bản đồ
 * - Tự động geocode địa chỉ khi người dùng nhập
 * - Tự động reverse geocode khi người dùng chọn vị trí trên bản đồ
 * - Xác thực tính nhất quán trước khi gửi form
 * - Hỗ trợ phân biệt tỉnh và thành phố
 */
document.addEventListener('DOMContentLoaded', function () {
    // Các selector
    const provinceSelect = document.getElementById('Car_ProvinceId');
    const detailedAddressInput = document.getElementById('Car_DetailedAddress');
    const latInput = document.getElementById('latitude');
    const lngInput = document.getElementById('longitude');
    const searchAddressBtn = document.getElementById('searchAddress');
    const getCurrentLocationBtn = document.getElementById('getCurrentLocation');
    const form = document.querySelector('form');

    // Danh sách thành phố trực thuộc trung ương
    const centralCities = ['Hà Nội', 'Hồ Chí Minh', 'Đà Nẵng', 'Hải Phòng', 'Cần Thơ'];

    // Danh sách mapping giữa thành phố và tỉnh
    const cityProvinceMap = {
        'Hà Nội': 'Hà Nội',
        'Hồ Chí Minh': 'Hồ Chí Minh',
        'Thành phố Hồ Chí Minh': 'Hồ Chí Minh',
        'TP Hồ Chí Minh': 'Hồ Chí Minh',
        'TP. Hồ Chí Minh': 'Hồ Chí Minh',
        'Sài Gòn': 'Hồ Chí Minh',
        'Đà Nẵng': 'Đà Nẵng',
        'Hải Phòng': 'Hải Phòng',
        'Cần Thơ': 'Cần Thơ'
    };

    // Biến theo dõi trạng thái
    const state = {
        addressChanged: false,
        coordsChanged: false,
        lastSearchedAddress: '',
        confirmedSync: false
    };

    // Khởi tạo bản đồ và marker (đã được khai báo bên ngoài)
    if (typeof map === 'undefined') {
        console.error("Bản đồ chưa được khởi tạo");
        return;
    }

    // 1. Đặt sự kiện cho form submit
    if (form) {
        form.addEventListener('submit', function (event) {
            if (!validateAddressCoordinateSync()) {
                event.preventDefault();
            }
        });
    }

    // 2. Đặt sự kiện theo dõi thay đổi địa chỉ
    if (provinceSelect) {
        provinceSelect.addEventListener('change', function () {
            state.addressChanged = true;
            state.confirmedSync = false;
        });
    }

    if (detailedAddressInput) {
        detailedAddressInput.addEventListener('input', function () {
            state.addressChanged = true;
            state.confirmedSync = false;
        });

        // Thêm sự kiện khi blur (người dùng rời khỏi input)
        detailedAddressInput.addEventListener('blur', function () {
            const province = provinceSelect ? provinceSelect.options[provinceSelect.selectedIndex].text : '';
            if (province !== "-- Chọn Tỉnh/Thành phố --" && detailedAddressInput.value.trim() !== '' &&
                state.addressChanged && state.lastSearchedAddress !== (detailedAddressInput.value + province)) {

                // Hiển thị đề xuất tìm kiếm địa chỉ
                showAddressSearchSuggestion();
            }
        });
    }

    // 3. Đặt sự kiện click trên bản đồ
    map.on('click', function (e) {
        updateCoordinates(e.latlng.lat, e.latlng.lng);
        state.coordsChanged = true;
        state.confirmedSync = false;

        // Hiển thị thông báo đang phân giải địa chỉ
        showResolvingMessage();

        // Phân giải địa chỉ từ tọa độ và cập nhật form
        reverseGeocode(e.latlng.lat, e.latlng.lng);
    });

    // 4. Đặt sự kiện cho nút tìm kiếm địa chỉ
    if (searchAddressBtn) {
        searchAddressBtn.addEventListener('click', function () {
            const province = provinceSelect ? provinceSelect.options[provinceSelect.selectedIndex].text : '';
            const address = detailedAddressInput ? detailedAddressInput.value : '';

            if (!validateAddressInput(province, address)) return;

            // Lưu địa chỉ đã tìm kiếm
            state.lastSearchedAddress = address + province;

            // Thực hiện tìm kiếm địa chỉ
            searchAddress(address, province);
        });
    }

    // 5. Đặt sự kiện cho nút lấy vị trí hiện tại
    if (getCurrentLocationBtn) {
        getCurrentLocationBtn.addEventListener('click', function () {
            if (navigator.geolocation) {
                showMessage('info', 'Đang xác định vị trí của bạn...');

                navigator.geolocation.getCurrentPosition(
                    // Success
                    function (position) {
                        const lat = position.coords.latitude;
                        const lng = position.coords.longitude;

                        // Cập nhật tọa độ
                        updateCoordinates(lat, lng);
                        state.coordsChanged = true;

                        // Phân giải địa chỉ từ tọa độ
                        reverseGeocode(lat, lng);
                    },
                    // Error
                    function (error) {
                        let message = "Không thể xác định vị trí: ";
                        switch (error.code) {
                            case error.PERMISSION_DENIED:
                                message += "Quyền truy cập vị trí bị từ chối.";
                                break;
                            case error.POSITION_UNAVAILABLE:
                                message += "Thông tin vị trí không khả dụng.";
                                break;
                            case error.TIMEOUT:
                                message += "Yêu cầu vị trí đã hết thời gian chờ.";
                                break;
                            default:
                                message += error.message;
                        }
                        showMessage('danger', message);
                    }
                );
            } else {
                showMessage('danger', "Trình duyệt của bạn không hỗ trợ định vị.");
            }
        });
    }

    // UTILITY FUNCTIONS

    /**
     * Tìm kiếm và cập nhật vị trí từ địa chỉ
     */
    function searchAddress(address, province) {
        if (!address || !province || province === "-- Chọn Tỉnh/Thành phố --") {
            showMessage('danger', "Vui lòng nhập địa chỉ chi tiết và chọn tỉnh.");
            return;
        }

        const searchQuery = address + ', ' + province + ', Vietnam';
        showMessage('info', `Đang tìm kiếm địa chỉ: ${searchQuery}`, 'search-status');

        // Sử dụng Nominatim API (OpenStreetMap) để tìm địa chỉ
        fetch(`https://nominatim.openstreetmap.org/search?format=json&limit=5&q=${encodeURIComponent(searchQuery)}`)
            .then(response => response.json())
            .then(data => {
                // Xóa thông báo tìm kiếm
                removeMessage('search-status');

                if (data && data.length > 0) {
                    // Nếu có nhiều kết quả, hiển thị modal để chọn
                    if (data.length > 1) {
                        showAddressSelectionModal(data);
                    } else {
                        // Nếu chỉ có 1 kết quả, sử dụng kết quả đó
                        const result = data[0];
                        updateCoordinates(result.lat, result.lon);
                        updateMapView(result.lat, result.lon, result.display_name);

                        // Gửi yêu cầu reverseGeocode để lấy chi tiết địa chỉ
                        reverseGeocode(result.lat, result.lon);

                        state.confirmedSync = true;
                        showMessage('success', `Đã tìm thấy địa chỉ: ${result.display_name}`);
                    }
                } else {
                    showMessage('danger', "Không tìm thấy địa chỉ. Vui lòng thử lại với địa chỉ chi tiết hơn.");
                }
            })
            .catch(error => {
                removeMessage('search-status');
                console.error("Lỗi khi tìm kiếm địa chỉ:", error);
                showMessage('danger', "Có lỗi xảy ra khi tìm kiếm địa chỉ. Vui lòng thử lại sau.");
            });
    }

    /**
     * Phân giải địa chỉ từ tọa độ (reverse geocoding)
     */
    function reverseGeocode(lat, lng) {
        fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`)
            .then(response => response.json())
            .then(data => {
                removeMessage('resolving-address');

                if (data && data.address) {
                    // Cập nhật marker popup
                    if (marker) {
                        marker.bindPopup(data.display_name).openPopup();
                    }

                    // Tự động cập nhật hoặc hỏi người dùng
                    const address = formatAddress(data.address);
                    confirmAddressUpdate(address, data);
                } else {
                    showMessage('warning', "Không thể phân giải địa chỉ từ vị trí này.");
                }
            })
            .catch(error => {
                removeMessage('resolving-address');
                console.error("Lỗi khi phân giải địa chỉ:", error);
                showMessage('danger', "Có lỗi xảy ra khi phân giải địa chỉ.");
            });
    }

    /**
     * Định dạng địa chỉ từ dữ liệu OpenStreetMap
     */
    function formatAddress(address) {
        const components = [];

        // Thứ tự ưu tiên các thành phần địa chỉ
        if (address.house_number) components.push(address.house_number);
        if (address.road) components.push(address.road);
        if (address.hamlet) components.push(address.hamlet);
        if (address.suburb) components.push(address.suburb);
        if (address.quarter) components.push(address.quarter);
        if (address.neighbourhood) components.push(address.neighbourhood);
        if (address.city_district) components.push(address.city_district);
        if (address.district) components.push(address.district);
        if (address.town) components.push(address.town);

        return components.join(', ');
    }

    /**
     * Hàm kiểm tra xem một địa điểm có phải là thành phố trực thuộc TW không
     */
    function isCentralCity(provinceName) {
        return centralCities.some(city =>
            provinceName.toLowerCase().includes(city.toLowerCase())
        );
    }

    /**
     * Hàm xác nhận cập nhật địa chỉ
     */
    function confirmAddressUpdate(formattedAddress, data) {
        // Phân tích địa chỉ từ OpenStreetMap
        let provinceName = data.address.state || data.address.province;
        const city = data.address.city;

        // Kiểm tra xem đây có phải là thành phố trực thuộc trung ương không
        let isCityLocation = false;

        // Nếu không tìm thấy tỉnh nhưng có thành phố
        if (!provinceName && city) {
            provinceName = city;
            isCityLocation = isCentralCity(city);
        }
        // Nếu có cả tỉnh và thành phố, kiểm tra xem thành phố có phải trực thuộc TW
        else if (provinceName && city && isCentralCity(city)) {
            provinceName = city;
            isCityLocation = true;
        }

        if (!provinceName) {
            showMessage('warning', "Không thể xác định tỉnh từ vị trí này. Vui lòng chọn tỉnh thủ công.");
            return;
        }

        // Hiện hộp thoại xác nhận
        const confirmDialog = document.createElement('div');
        confirmDialog.className = 'card mt-3 mb-3 address-confirm-dialog';
        confirmDialog.id = 'addressConfirmDialog';

        // Thêm cảnh báo nếu là thành phố trực thuộc TW
        const cityWarning = isCityLocation ? `
            <div class="alert alert-warning">
                <i class="bi bi-exclamation-triangle me-2"></i>
                Vị trí này thuộc thành phố ${provinceName}. Vui lòng xác nhận tỉnh phù hợp nếu cần.
            </div>
        ` : '';

        confirmDialog.innerHTML = `
            <div class="card-header bg-primary text-white">
                <h5 class="m-0">Cập nhật thông tin địa chỉ</h5>
            </div>
            <div class="card-body">
                ${cityWarning}
                <p>Bạn đã chọn vị trí mới trên bản đồ. Cập nhật thông tin địa chỉ?</p>
                <p><strong>Địa chỉ chi tiết:</strong> ${formattedAddress}</p>
                <p><strong>Tỉnh/Thành phố:</strong> ${provinceName}</p>
                <div class="d-flex justify-content-end gap-2">
                    <button type="button" id="cancelAddressUpdate" class="btn btn-secondary">Không</button>
                    <button type="button" id="confirmAddressUpdate" class="btn btn-primary">Đồng ý</button>
                </div>
            </div>
        `;

        // Thêm dialog vào DOM
        const mapContainer = document.getElementById('map').parentElement;
        mapContainer.insertAdjacentElement('afterend', confirmDialog);

        // Xử lý sự kiện cho các nút
        document.getElementById('confirmAddressUpdate').addEventListener('click', function () {
            // Cập nhật địa chỉ chi tiết
            if (detailedAddressInput) {
                detailedAddressInput.value = formattedAddress;
            }

            // Cập nhật province select
            if (provinceSelect) {
                let found = false;
                let lookupName = provinceName;

                // Nếu là thành phố trực thuộc TW, tìm trong mapping
                if (isCityLocation) {
                    lookupName = cityProvinceMap[provinceName] || provinceName;
                }

                // Tìm trong dropdown
                for (let i = 0; i < provinceSelect.options.length; i++) {
                    if (provinceSelect.options[i].text.toLowerCase() === lookupName.toLowerCase() ||
                        provinceSelect.options[i].text.toLowerCase().includes(lookupName.toLowerCase())) {
                        provinceSelect.selectedIndex = i;
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    showMessage('warning',
                        `Không tìm thấy tỉnh "${lookupName}" trong danh sách. Vui lòng chọn tỉnh thủ công.`);
                }
            }

            // Cập nhật trạng thái
            state.confirmedSync = true;
            state.addressChanged = false;
            state.coordsChanged = false;

            // Hiện thông báo thành công
            showMessage('success', 'Đã cập nhật thông tin địa chỉ từ bản đồ.');

            // Xóa dialog
            document.getElementById('addressConfirmDialog').remove();
        });

        document.getElementById('cancelAddressUpdate').addEventListener('click', function () {
            // Xóa dialog
            document.getElementById('addressConfirmDialog').remove();

            // Hiện thông báo
            showMessage('warning', 'Thông tin địa chỉ không được cập nhật.');
        });
    }

    /**
     * Hiển thị modal chọn địa chỉ khi có nhiều kết quả
     */
    function showAddressSelectionModal(addresses) {
        // Tạo modal
        const modalHTML = `
            <div class="modal fade" id="addressSelectionModal" tabindex="-1" aria-labelledby="addressSelectionModalLabel">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="addressSelectionModalLabel">Chọn địa chỉ chính xác</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <p>Có nhiều địa chỉ phù hợp với thông tin bạn nhập. Vui lòng chọn địa chỉ chính xác:</p>
                            <div class="list-group">
                                ${addresses.map((addr, index) => `
                                    <button type="button" class="list-group-item list-group-item-action address-item"
                                            data-lat="${addr.lat}" data-lon="${addr.lon}" data-name="${addr.display_name}">
                                        <div class="d-flex w-100 justify-content-between">
                                            <h6 class="mb-1">Địa chỉ ${index + 1}</h6>
                                            <small>${Math.round(addr.importance * 100)}% phù hợp</small>
                                        </div>
                                        <p class="mb-1">${addr.display_name}</p>
                                    </button>
                                `).join('')}
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Thêm modal vào body
        document.body.insertAdjacentHTML('beforeend', modalHTML);

        // Khởi tạo modal Bootstrap
        const modal = new bootstrap.Modal(document.getElementById('addressSelectionModal'));
        modal.show();

        // Xử lý sự kiện chọn địa chỉ
        document.querySelectorAll('.address-item').forEach(item => {
            item.addEventListener('click', function () {
                const lat = this.getAttribute('data-lat');
                const lon = this.getAttribute('data-lon');
                const name = this.getAttribute('data-name');

                updateCoordinates(lat, lon);
                updateMapView(lat, lon, name);

                // Gọi reverseGeocode để lấy thông tin chi tiết về địa chỉ
                reverseGeocode(lat, lon);

                modal.hide();

                // Thông báo thành công
                showMessage('success', `Đã chọn địa chỉ: ${name}`);

                // Xóa modal sau khi ẩn
                document.getElementById('addressSelectionModal').addEventListener('hidden.bs.modal', function () {
                    this.remove();
                });
            });
        });

        // Xóa modal khi đóng
        document.getElementById('addressSelectionModal').addEventListener('hidden.bs.modal', function () {
            this.remove();
        });
    }

    /**
     * Cập nhật tọa độ vào form
     */
    function updateCoordinates(lat, lng) {
        if (latInput) latInput.value = parseFloat(lat).toFixed(6);
        if (lngInput) lngInput.value = parseFloat(lng).toFixed(6);

        // Thêm hoặc di chuyển marker
        if (marker) {
            marker.setLatLng([lat, lng]);
        } else if (map) {
            marker = L.marker([lat, lng]).addTo(map);
        }
    }

    /**
     * Cập nhật góc nhìn bản đồ
     */
    function updateMapView(lat, lng, popupContent) {
        if (!map) return;

        map.setView([lat, lng], 16);

        if (marker) {
            if (popupContent) {
                marker.bindPopup(popupContent).openPopup();
            }
        } else {
            marker = L.marker([lat, lng]).addTo(map);
            if (popupContent) {
                marker.bindPopup(popupContent).openPopup();
            }
        }
    }

    /**
     * Xác thực đầu vào địa chỉ
     */
    function validateAddressInput(province, address) {
        if (!address || address.trim() === '') {
            showMessage('danger', 'Vui lòng nhập địa chỉ chi tiết.');
            return false;
        }

        if (!province || province === '-- Chọn Tỉnh/Thành phố --') {
            showMessage('danger', 'Vui lòng chọn tỉnh.');
            return false;
        }

        return true;
    }

    /**
     * Xác thực tính nhất quán giữa địa chỉ và tọa độ trước khi gửi form
     */
    function validateAddressCoordinateSync() {
        // Kiểm tra nếu không có địa chỉ hoặc tọa độ
        if (!latInput?.value || !lngInput?.value) {
            showMessage('danger', 'Vui lòng chọn vị trí xe trên bản đồ.');
            return false;
        }

        // Nếu đã xác nhận đồng bộ, hoặc không có thay đổi, tiếp tục submit
        if (state.confirmedSync || (!state.addressChanged && !state.coordsChanged)) {
            return true;
        }

        // Nếu có sự thay đổi địa chỉ hoặc tọa độ mà chưa đồng bộ
        const message = state.addressChanged
            ? 'Địa chỉ đã thay đổi nhưng chưa được xác nhận trên bản đồ. Bạn có muốn tìm kiếm địa chỉ trước khi tiếp tục?'
            : 'Vị trí trên bản đồ đã thay đổi nhưng chưa cập nhật địa chỉ. Bạn có muốn tiếp tục?';

        if (confirm(message)) {
            if (state.addressChanged && searchAddressBtn) {
                // Nếu địa chỉ thay đổi, tự động tìm kiếm
                searchAddressBtn.click();
                return false; // Ngăn form submit
            } else {
                // Cho phép tiếp tục nếu người dùng đồng ý
                return true;
            }
        } else {
            return false; // Ngăn form submit nếu người dùng không đồng ý
        }
    }

    /**
     * Hiển thị đề xuất tìm kiếm địa chỉ
     */
    function showAddressSearchSuggestion() {
        // Tạo nút gợi ý
        const suggestionBtn = document.createElement('button');
        suggestionBtn.type = 'button';
        suggestionBtn.className = 'btn btn-outline-info btn-sm mt-2 address-search-suggestion';
        suggestionBtn.innerHTML = '<i class="bi bi-search me-1"></i> Tìm địa chỉ này trên bản đồ?';
        suggestionBtn.onclick = function () {
            searchAddressBtn.click();
            this.remove();
        };

        // Xóa gợi ý cũ nếu có
        const oldSuggestion = document.querySelector('.address-search-suggestion');
        if (oldSuggestion) oldSuggestion.remove();

        // Thêm gợi ý vào sau input địa chỉ
        detailedAddressInput.parentNode.insertAdjacentElement('afterend', suggestionBtn);

        // Tự động xóa sau 10 giây
        setTimeout(() => suggestionBtn.remove(), 10000);
    }

    /**
     * Hiển thị thông báo đang phân giải địa chỉ
     */
    function showResolvingMessage() {
        // Xóa thông báo cũ
        removeMessage('resolving-address');

        // Tạo thông báo mới
        const message = document.createElement('div');
        message.className = 'alert alert-info mt-2';
        message.id = 'resolving-address';
        message.innerHTML = '<i class="bi bi-arrow-repeat me-2"></i>Đang phân giải địa chỉ từ tọa độ...';

        // Thêm vào sau bản đồ
        document.getElementById('map').parentNode.insertAdjacentElement('afterend', message);
    }

    /**
     * Hiển thị thông báo
     */
    function showMessage(type, content, id = null) {
        // Xóa thông báo cũ có cùng ID nếu có
        if (id) removeMessage(id);

        // Tạo thông báo mới
        const message = document.createElement('div');
        message.className = `alert alert-${type} alert-dismissible fade show mt-2`;
        if (id) message.id = id;

        message.innerHTML = `
            ${content}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        // Thêm vào sau bản đồ
        document.getElementById('map').parentNode.insertAdjacentElement('afterend', message);

        // Tự động xóa sau 5 giây
        if (!id) {
            setTimeout(() => {
                if (message && message.parentNode) {
                    message.classList.remove('show');
                    setTimeout(() => message.remove(), 150);
                }
            }, 5000);
        }
    }

    /**
     * Xóa thông báo theo ID
     */
    function removeMessage(id) {
        const message = document.getElementById(id);
        if (message) message.remove();
    }

    // Thêm CSS cho module
    const style = document.createElement('style');
    style.textContent = `
        .address-confirm-dialog {
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
        }
        .address-search-suggestion {
            animation: pulse 2s infinite;
        }
        @keyframes pulse {
            0% { opacity: 1; }
            50% { opacity: 0.8; }
            100% { opacity: 1; }
        }
    `;
    document.head.appendChild(style);

    // Cập nhật nhãn cho select box tỉnh/thành phố thành "Tỉnh"
    const provinceLabel = provinceSelect?.closest('.col-md-6')?.querySelector('label');
    if (provinceLabel) {
        provinceLabel.textContent = 'Tỉnh';
    }

    // Cập nhật placeholder cho select box tỉnh/thành phố
    if (provinceSelect) {
        provinceSelect.querySelector('option[value=""]').textContent = '-- Chọn Tỉnh --';
    }
});