/**
 * Tổng hợp các module JS cho trang đặt thuê xe
 * Chịu trách nhiệm khởi tạo và thực hiện các tác vụ cần thiết sau khi tất cả modules đã được tải
 */
document.addEventListener('DOMContentLoaded', function () {
    console.log('Rental bundle loaded. Initializing all rental modules...');

    // Kiểm tra xem các module đã được tải đầy đủ chưa
    const requiredModules = [
        'submitCalculateForm',
        'updateDriverFeeDisplay',
        'initializePickupMap',
        'initializeDropoffMap',
        'initializeDeliveryMap'
    ];

    setTimeout(() => {
        // Kiểm tra các module cần thiết đã được tải
        let missingModules = requiredModules.filter(module => !window[module]);
        if (missingModules.length > 0) {
            console.warn('Missing modules:', missingModules.join(', '));
        } else {
            console.log('All required modules loaded successfully');
        }

        // Thiết lập ban đầu - kích hoạt các chức năng phù hợp với trạng thái hiện tại
        const serviceSelfDrive = document.getElementById('serviceSelfDrive');
        const serviceWithDriver = document.getElementById('serviceWithDriver');
        const rentalTypeDay = document.getElementById('rentalTypeDay');
        const rentalTypeHour = document.getElementById('rentalTypeHour');

        // Kích hoạt sự kiện để cập nhật UI
        if (serviceSelfDrive && serviceSelfDrive.checked) {
            serviceSelfDrive.dispatchEvent(new Event('change'));
        } else if (serviceWithDriver && serviceWithDriver.checked) {
            serviceWithDriver.dispatchEvent(new Event('change'));
        }

        if (rentalTypeDay && rentalTypeDay.checked) {
            rentalTypeDay.dispatchEvent(new Event('change'));
        } else if (rentalTypeHour && rentalTypeHour.checked) {
            rentalTypeHour.dispatchEvent(new Event('change'));
        }
    }, 500); // Đợi 500ms để các module khác được tải
});