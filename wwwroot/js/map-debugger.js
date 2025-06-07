/**
 * Map Debugger - Khắc phục vấn đề hiển thị bản đồ
 */
document.addEventListener('DOMContentLoaded', function () {
    console.log('Map debugger loaded');

    // Kiểm tra xem Leaflet đã được tải hay chưa
    if (typeof L === 'undefined') {
        console.error('Leaflet is not loaded!');
        return;
    }

    console.log('Leaflet is available', L.version);

    // Kiểm tra phần tử bản đồ
    const mapElement = document.getElementById('locationMap');
    if (!mapElement) {
        console.error('Map container #locationMap not found in the DOM');
        return;
    }

    console.log('Map container found:', mapElement);

    // Kiểm tra car handler
    if (!window.carMapHandler) {
        console.error('carMapHandler not defined. Initializing direct map rendering');

        // Đọc dữ liệu từ data attributes nếu có
        const lat = mapElement.dataset.lat;
        const lng = mapElement.dataset.lng;
        const name = mapElement.dataset.name;
        const address = mapElement.dataset.address;

        if (lat && lng) {
            try {
                // Khởi tạo bản đồ trực tiếp
                const map = L.map('locationMap').setView([parseFloat(lat), parseFloat(lng)], 15);

                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                }).addTo(map);

                L.marker([parseFloat(lat), parseFloat(lng)]).addTo(map)
                    .bindPopup(name + '<br>' + address)
                    .openPopup();

                console.log('Emergency map initialization successful');
            } catch (e) {
                console.error('Error during emergency map initialization:', e);
            }
        } else {
            console.error('Missing map coordinates in data attributes');
        }
    }
});