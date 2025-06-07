/**
 * Xử lý hiển thị bản đồ vị trí xe
 */
document.addEventListener('DOMContentLoaded', function () {
    // Khởi tạo bản đồ nếu có phần tử locationMap và có tọa độ
    function initMap(lat, lng, carName, address) {
        if (!document.getElementById('locationMap')) return;

        var map = L.map('locationMap').setView([lat, lng], 15);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);

        L.marker([lat, lng]).addTo(map)
            .bindPopup(`${carName}<br>${address}`)
            .openPopup();

        // Xử lý nút Xem đường đi
        const directionsBtn = document.getElementById('getDirectionsBtn');
        if (directionsBtn) {
            directionsBtn.addEventListener('click', function () {
                getDirections(lat, lng);
            });
        }
    }

    /**
     * Lấy chỉ đường từ vị trí hiện tại đến xe
     */
    function getDirections(lat, lng) {
        // Hỏi người dùng cho phép truy cập vị trí
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                // Success
                function (position) {
                    var userLat = position.coords.latitude;
                    var userLng = position.coords.longitude;

                    // Tạo URL Google Maps chỉ đường
                    var googleMapsUrl = `https://www.google.com/maps/dir/?api=1&origin=${userLat},${userLng}&destination=${lat},${lng}&travelmode=driving`;

                    // Mở liên kết trong tab mới
                    window.open(googleMapsUrl, '_blank');
                },
                // Error
                function (error) {
                    let message;
                    switch (error.code) {
                        case error.PERMISSION_DENIED:
                            message = "Bạn đã từ chối cho phép truy cập vị trí.";
                            break;
                        case error.POSITION_UNAVAILABLE:
                            message = "Không thể xác định vị trí của bạn.";
                            break;
                        case error.TIMEOUT:
                            message = "Quá thời gian xác định vị trí.";
                            break;
                        default:
                            message = "Đã xảy ra lỗi khi xác định vị trí.";
                            break;
                    }
                    alert(message);

                    // Nếu không lấy được vị trí, vẫn mở Google Maps với điểm đến
                    var googleMapsUrl = `https://www.google.com/maps/search/?api=1&query=${lat},${lng}`;
                    window.open(googleMapsUrl, '_blank');
                }
            );
        } else {
            alert("Trình duyệt của bạn không hỗ trợ định vị.");

            // Mở Google Maps với điểm đến
            var googleMapsUrl = `https://www.google.com/maps/search/?api=1&query=${lat},${lng}`;
            window.open(googleMapsUrl, '_blank');
        }
    }

    // Khởi tạo biến toàn cục để trang Razor có thể gọi
    window.carMapHandler = {
        initMap: initMap
    };
});