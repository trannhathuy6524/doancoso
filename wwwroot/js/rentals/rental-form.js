/**
 * Xử lý chung cho form đặt thuê xe
 */
document.addEventListener('DOMContentLoaded', function() {
    // Debug: In ra thông tin ban đầu
    console.log("Form đặt thuê xe được khởi tạo");

    // Tự động ẩn thông báo sau 5 giây
    setTimeout(function () {
        $('.alert-dismissible').alert('close');
    }, 5000);

    // Thêm CSS để tạo hiệu ứng mượt mà cho phần dịch vụ tài xế
    document.head.insertAdjacentHTML('beforeend', `
    <style>
        #driverServiceOptions {
            transition: all 0.3s ease;
            max-height: 1000px;
            overflow: hidden;
            opacity: 1;
            margin-top: 1rem;
        }

        #driverServiceOptions.d-none {
            max-height: 0;
            opacity: 0;
            margin-top: 0;
            padding: 0;
            pointer-events: none;
        }
    </style>
    `);

    // Hàm tiện ích để submit form tính giá
    window.submitCalculateForm = function() {
        const calculateButton = document.querySelector('button[asp-page-handler="Calculate"]');
        if (calculateButton) {
            calculateButton.click();
        }
    }

    // Kiểm soát submit form
    document.querySelector('form')?.addEventListener('submit', function(e) {
        const agreeTerms = document.getElementById('agreeTerms');
        if (agreeTerms && !agreeTerms.checked && !e.submitter.hasAttribute('asp-page-handler')) {
            e.preventDefault();
            alert('Vui lòng đồng ý với các điều khoản thuê xe trước khi tiếp tục.');
        }
    });
});