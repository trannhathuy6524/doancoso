# 🚗 ASP.NET Core MVC - Website Thuê Xe

Dự án xây dựng hệ thống cho thuê xe, nơi người dùng có thể đăng xe cho thuê, tìm kiếm, đặt xe, đánh giá,... được phát triển bằng ASP.NET Core MVC, Entity Framework Core và Bootstrap.

---


## 🔧 Cấu trúc làm việc nhóm với Git

### 1. **Không push trực tiếp lên `main`**

Mỗi người làm việc trên **nhánh riêng**, ví dụ:

```
git checkout -b huy/tim-kiem-xe

Làm việc xong thì:
git add .
git commit -m "Thêm chức năng tìm kiếm xe"
git push origin huy/tim-kiem-xe
