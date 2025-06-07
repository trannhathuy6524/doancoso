/**
 * Xử lý hiển thị lịch thuê xe
 */
document.addEventListener('DOMContentLoaded', function () {
    let calendar;

    // Khởi tạo FullCalendar để hiển thị lịch đặt xe
    initializeCalendar();

    function initializeCalendar() {
        const calendarEl = document.getElementById('calendar');
        if (!calendarEl) {
            console.error("Calendar element not found");
            return;
        }

        // Lấy danh sách ngày không khả dụng từ input hidden
        const unavailableDatesInput = document.getElementById('unavailableDatesData');
        let unavailableDates = [];

        // Thêm sau dòng 20 trong hàm initializeCalendar
        console.log("Raw unavailableDatesData value:", unavailableDatesInput.value);
        if (unavailableDatesInput && unavailableDatesInput.value) {
            try {
                // Kiểm tra nếu giá trị trống hoặc không hoàn chỉnh
                const value = unavailableDatesInput.value.trim();
                if (value === "" || value === "[" || value === "[]") {
                    unavailableDates = [];
                    console.log("Empty unavailable dates array");
                } else {
                    unavailableDates = JSON.parse(value);
                    console.log("Unavailable dates parsed successfully:", unavailableDates);
                }
            } catch (e) {
                console.error("Error parsing unavailable dates:", e);
                unavailableDates = []; // Đảm bảo mảng trống khi có lỗi

                // Phần còn lại của mã xử lý lỗi...
            }
        } else {
            console.warn("No unavailableDatesData input found or empty value");
            unavailableDates = []; // Đảm bảo mảng trống khi không có dữ liệu
        }

        // Lấy ngày bắt đầu và kết thúc từ các input
        const startDateInput = document.getElementById('StartDate');
        const endDateInput = document.getElementById('EndDate');
        const rentalDateInput = document.getElementById('RentalDate');

        let selectedStart = startDateInput ? startDateInput.value : null;
        let selectedEnd = endDateInput ? endDateInput.value : null;

        // Nếu thuê theo giờ, sử dụng ngày thuê làm cả ngày bắt đầu và kết thúc
        if ((!selectedStart || !selectedEnd) && rentalDateInput) {
            selectedStart = rentalDateInput.value;
            selectedEnd = rentalDateInput.value;
        }

        console.log("Selected dates for calendar:", selectedStart, selectedEnd);

        // Tạo mảng sự kiện cho calendar
        const events = [];

        // Thêm ngày không khả dụng (cần hiển thị màu đỏ nền)
        if (Array.isArray(unavailableDates) && unavailableDates.length > 0) {
            console.log(`Adding ${unavailableDates.length} unavailable dates to calendar`);
            unavailableDates.forEach(date => {
                if (date) {
                    events.push({
                        start: date,
                        end: date,
                        display: 'background',
                        backgroundColor: 'rgba(255,0,0,0.25)',
                        classNames: ['unavailable-date']
                    });
                    console.log(`Added unavailable date to calendar: ${date}`);
                }
            });
        } else {
            console.log("No unavailable dates to add to calendar");
        }

        // Thêm ngày đã chọn (hiển thị màu xanh)
        if (selectedStart) {
            let endDate = selectedEnd || selectedStart;

            // Điều chỉnh ngày kết thúc để hiển thị đúng trên calendar (thêm 1 ngày)
            // Chỉ khi selectedEnd khác null và không phải ngày hiện tại
            if (selectedEnd && selectedEnd !== selectedStart) {
                const endDateObj = new Date(endDate);
                endDateObj.setDate(endDateObj.getDate() + 1);
                endDate = endDateObj.toISOString().split('T')[0];
            }

            events.push({
                start: selectedStart,
                end: endDate,
                backgroundColor: 'rgba(0,123,255,0.25)',
                borderColor: '#0d6efd',
                classNames: ['selected-date']
            });
            console.log(`Added selected date range to calendar: ${selectedStart} to ${endDate}`);
        }

        // Khởi tạo FullCalendar với cấu hình và sự kiện
        calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth'
            },
            events: events,
            locale: 'vi',
            firstDay: 1, // Bắt đầu từ thứ 2
            eventDidMount: function (info) {
                // Thêm tooltip cho các ngày không khả dụng
                if (info.event.display === 'background') {
                    info.el.title = 'Ngày đã có người đặt';
                }
            }
        });

        calendar.render();
        console.log("Calendar rendered with", events.length, "events");
    }

    // Hàm cập nhật lịch khi có dữ liệu mới
    window.updateCalendar = function (unavailableDates) {
        if (!calendar) {
            console.error("Calendar not initialized for update");
            return;
        }

        // Xóa tất cả các sự kiện hiện tại
        calendar.getEvents().forEach(event => event.remove());
        console.log("Cleared all calendar events for update");

        // Thêm lại các ngày không khả dụng
        if (unavailableDates && Array.isArray(unavailableDates)) {
            unavailableDates.forEach(dateStr => {
                if (dateStr) {
                    calendar.addEvent({
                        start: dateStr,
                        end: dateStr,
                        display: 'background',
                        backgroundColor: 'rgba(255,0,0,0.25)',
                        classNames: ['unavailable-date']
                    });
                    console.log(`Re-added unavailable date: ${dateStr}`);
                }
            });
        }

        // Xác định ngày đã chọn để hiển thị
        let selectedStart, selectedEnd;
        const rentalTypeDay = document.getElementById('rentalTypeDay');

        if (rentalTypeDay && rentalTypeDay.checked) {
            selectedStart = document.getElementById('StartDate')?.value;
            selectedEnd = document.getElementById('EndDate')?.value;
        } else {
            selectedStart = document.getElementById('RentalDate')?.value;
            selectedEnd = selectedStart;
        }

        // Thêm ngày đã chọn vào lịch nếu hợp lệ
        if (selectedStart) {
            let endDate = selectedEnd || selectedStart;

            // Điều chỉnh ngày kết thúc để hiển thị đúng trên calendar (thêm 1 ngày)
            // Chỉ khi selectedEnd khác null và không phải ngày hiện tại
            if (selectedEnd && selectedEnd !== selectedStart) {
                const endDateObj = new Date(endDate);
                endDateObj.setDate(endDateObj.getDate() + 1);
                endDate = endDateObj.toISOString().split('T')[0];
            }

            calendar.addEvent({
                start: selectedStart,
                end: endDate,
                backgroundColor: 'rgba(0,123,255,0.25)',
                borderColor: '#0d6efd',
                classNames: ['selected-date']
            });
            console.log(`Re-added selected date range: ${selectedStart} to ${endDate}`);
        }

        // Re-render calendar để hiển thị các sự kiện mới
        calendar.render();
        console.log("Calendar re-rendered after update");
    };

    // Đăng ký sự kiện khi thay đổi ngày để cập nhật calendar
    const dateInputs = ['StartDate', 'EndDate', 'RentalDate'];
    dateInputs.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('change', function () {
                console.log(`Date input changed: ${id} = ${this.value}`);
                const unavailableDatesInput = document.getElementById('unavailableDatesData');
                let unavailableDates = [];

                if (unavailableDatesInput && unavailableDatesInput.value) {
                    try {
                        unavailableDates = JSON.parse(unavailableDatesInput.value);
                    } catch (e) {
                        console.error("Error parsing unavailable dates on input change:", e);
                    }
                }

                window.updateCalendar(unavailableDates);
            });
        }
    });

    // Đăng ký sự kiện cho radio buttons loại thuê để cập nhật calendar khi chuyển loại thuê
    const rentalTypeInputs = ['rentalTypeDay', 'rentalTypeHour'];
    rentalTypeInputs.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('change', function () {
                console.log(`Rental type changed: ${id} = ${this.checked}`);
                const unavailableDatesInput = document.getElementById('unavailableDatesData');
                let unavailableDates = [];

                if (unavailableDatesInput && unavailableDatesInput.value) {
                    try {
                        // Kiểm tra giá trị trống hoặc không hợp lệ
                        const value = unavailableDatesInput.value.trim();
                        if (value === "" || value === "[" || value === "[]") {
                            unavailableDates = [];
                            console.log("Empty unavailable dates array in event listener");
                        } else {
                            unavailableDates = JSON.parse(value);
                            console.log("Unavailable dates parsed successfully in event listener:", unavailableDates);
                        }
                    } catch (e) {
                        console.error("Error parsing unavailable dates on rental type change:", e);
                        unavailableDates = []; // Đặt mảng rỗng khi có lỗi
                    }
                } else {
                    console.warn("No unavailableDatesData input found or empty value in event listener");
                    unavailableDates = [];
                }

                window.updateCalendar(unavailableDates);
            });
        }
    });
});