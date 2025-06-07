using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Data
{
    public static class ProvinceSeeder
    {
        public static void SeedProvinces(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Province>().HasData(
                new Province { Id = 1, Name = "Hà Nội", Code = "HN" },
                new Province { Id = 2, Name = "Hồ Chí Minh", Code = "HCM" },
                new Province { Id = 3, Name = "Đà Nẵng", Code = "DN" },
                new Province { Id = 4, Name = "Hải Phòng", Code = "HP" },
                new Province { Id = 5, Name = "Cần Thơ", Code = "CT" },
                new Province { Id = 6, Name = "An Giang", Code = "AG" },
                new Province { Id = 7, Name = "Bà Rịa - Vũng Tàu", Code = "VT" },
                new Province { Id = 8, Name = "Bắc Giang", Code = "BG" },
                new Province { Id = 9, Name = "Bắc Kạn", Code = "BK" },
                new Province { Id = 10, Name = "Bạc Liêu", Code = "BL" },
                new Province { Id = 11, Name = "Bắc Ninh", Code = "BN" },
                new Province { Id = 12, Name = "Bến Tre", Code = "BT" },
                new Province { Id = 13, Name = "Bình Định", Code = "BD" },
                new Province { Id = 14, Name = "Bình Dương", Code = "BDG" },
                new Province { Id = 15, Name = "Bình Phước", Code = "BP" },
                new Province { Id = 16, Name = "Bình Thuận", Code = "BTH" },
                new Province { Id = 17, Name = "Cà Mau", Code = "CM" },
                new Province { Id = 18, Name = "Cao Bằng", Code = "CB" },
                new Province { Id = 19, Name = "Đắk Lắk", Code = "DL" },
                new Province { Id = 20, Name = "Đắk Nông", Code = "DNO" },
                new Province { Id = 21, Name = "Điện Biên", Code = "DB" },
                new Province { Id = 22, Name = "Đồng Nai", Code = "DNA" },
                new Province { Id = 23, Name = "Đồng Tháp", Code = "DT" },
                new Province { Id = 24, Name = "Gia Lai", Code = "GL" },
                new Province { Id = 25, Name = "Hà Giang", Code = "HG" },
                new Province { Id = 26, Name = "Hà Nam", Code = "HNA" },
                new Province { Id = 27, Name = "Hà Tĩnh", Code = "HT" },
                new Province { Id = 28, Name = "Hải Dương", Code = "HD" },
                new Province { Id = 29, Name = "Hậu Giang", Code = "HGI" },
                new Province { Id = 30, Name = "Hòa Bình", Code = "HB" },
                new Province { Id = 31, Name = "Hưng Yên", Code = "HY" },
                new Province { Id = 32, Name = "Khánh Hòa", Code = "KH" },
                new Province { Id = 33, Name = "Kiên Giang", Code = "KG" },
                new Province { Id = 34, Name = "Kon Tum", Code = "KT" },
                new Province { Id = 35, Name = "Lai Châu", Code = "LC" },
                new Province { Id = 36, Name = "Lâm Đồng", Code = "LD" },
                new Province { Id = 37, Name = "Lạng Sơn", Code = "LS" },
                new Province { Id = 38, Name = "Lào Cai", Code = "LCA" },
                new Province { Id = 39, Name = "Long An", Code = "LA" },
                new Province { Id = 40, Name = "Nam Định", Code = "ND" },
                new Province { Id = 41, Name = "Nghệ An", Code = "NA" },
                new Province { Id = 42, Name = "Ninh Bình", Code = "NB" },
                new Province { Id = 43, Name = "Ninh Thuận", Code = "NT" },
                new Province { Id = 44, Name = "Phú Thọ", Code = "PT" },
                new Province { Id = 45, Name = "Phú Yên", Code = "PY" },
                new Province { Id = 46, Name = "Quảng Bình", Code = "QB" },
                new Province { Id = 47, Name = "Quảng Nam", Code = "QNA" },
                new Province { Id = 48, Name = "Quảng Ngãi", Code = "QNG" },
                new Province { Id = 49, Name = "Quảng Ninh", Code = "QNI" },
                new Province { Id = 50, Name = "Quảng Trị", Code = "QT" },
                new Province { Id = 51, Name = "Sóc Trăng", Code = "ST" },
                new Province { Id = 52, Name = "Sơn La", Code = "SL" },
                new Province { Id = 53, Name = "Tây Ninh", Code = "TN" },
                new Province { Id = 54, Name = "Thái Bình", Code = "TB" },
                new Province { Id = 55, Name = "Thái Nguyên", Code = "TNG" },
                new Province { Id = 56, Name = "Thanh Hóa", Code = "TH" },
                new Province { Id = 57, Name = "Thừa Thiên Huế", Code = "TTH" },
                new Province { Id = 58, Name = "Tiền Giang", Code = "TG" },
                new Province { Id = 59, Name = "Trà Vinh", Code = "TV" },
                new Province { Id = 60, Name = "Tuyên Quang", Code = "TQ" },
                new Province { Id = 61, Name = "Vĩnh Long", Code = "VL" },
                new Province { Id = 62, Name = "Vĩnh Phúc", Code = "VP" },
                new Province { Id = 63, Name = "Yên Bái", Code = "YB" }
            );
        }
    }
}
