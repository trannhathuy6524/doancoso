/**
 * Quản lý phiên đăng nhập
 */
document.addEventListener('DOMContentLoaded', function () {
    // Kiểm tra biến toàn cục isUserLoggedIn được truyền từ trang Razor
    if (typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn) {
        startKeepAliveTimer();
    }

    /**
     * Bắt đầu timer để giữ phiên hoạt động
     */
    function startKeepAliveTimer() {
        console.log('Session manager initialized for logged in user');
        setInterval(pingKeepAlive, 240000); // Ping mỗi 4 phút
    }

    /**
     * Gửi yêu cầu đến máy chủ để giữ phiên đăng nhập
     */
    function pingKeepAlive() {
        fetch('/api/Session/KeepAlive', {
            method: 'GET',
            credentials: 'same-origin',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                if (response.ok) {
                    console.log('Phiên đăng nhập được gia hạn thành công');
                } else {
                    console.warn('Không thể gia hạn phiên đăng nhập');
                }
            })
            .catch(error => {
                console.error('Lỗi khi gia hạn phiên đăng nhập:', error);
            });
    }
});