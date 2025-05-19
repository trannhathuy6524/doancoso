using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GotoCarRental.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedProvinceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Provinces",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "HN", "Hà Nội" },
                    { 2, "HCM", "Hồ Chí Minh" },
                    { 3, "DN", "Đà Nẵng" },
                    { 4, "HP", "Hải Phòng" },
                    { 5, "CT", "Cần Thơ" },
                    { 6, "AG", "An Giang" },
                    { 7, "VT", "Bà Rịa - Vũng Tàu" },
                    { 8, "BG", "Bắc Giang" },
                    { 9, "BK", "Bắc Kạn" },
                    { 10, "BL", "Bạc Liêu" },
                    { 11, "BN", "Bắc Ninh" },
                    { 12, "BT", "Bến Tre" },
                    { 13, "BD", "Bình Định" },
                    { 14, "BDG", "Bình Dương" },
                    { 15, "BP", "Bình Phước" },
                    { 16, "BTH", "Bình Thuận" },
                    { 17, "CM", "Cà Mau" },
                    { 18, "CB", "Cao Bằng" },
                    { 19, "DL", "Đắk Lắk" },
                    { 20, "DNO", "Đắk Nông" },
                    { 21, "DB", "Điện Biên" },
                    { 22, "DNA", "Đồng Nai" },
                    { 23, "DT", "Đồng Tháp" },
                    { 24, "GL", "Gia Lai" },
                    { 25, "HG", "Hà Giang" },
                    { 26, "HNA", "Hà Nam" },
                    { 27, "HT", "Hà Tĩnh" },
                    { 28, "HD", "Hải Dương" },
                    { 29, "HGI", "Hậu Giang" },
                    { 30, "HB", "Hòa Bình" },
                    { 31, "HY", "Hưng Yên" },
                    { 32, "KH", "Khánh Hòa" },
                    { 33, "KG", "Kiên Giang" },
                    { 34, "KT", "Kon Tum" },
                    { 35, "LC", "Lai Châu" },
                    { 36, "LD", "Lâm Đồng" },
                    { 37, "LS", "Lạng Sơn" },
                    { 38, "LCA", "Lào Cai" },
                    { 39, "LA", "Long An" },
                    { 40, "ND", "Nam Định" },
                    { 41, "NA", "Nghệ An" },
                    { 42, "NB", "Ninh Bình" },
                    { 43, "NT", "Ninh Thuận" },
                    { 44, "PT", "Phú Thọ" },
                    { 45, "PY", "Phú Yên" },
                    { 46, "QB", "Quảng Bình" },
                    { 47, "QNA", "Quảng Nam" },
                    { 48, "QNG", "Quảng Ngãi" },
                    { 49, "QNI", "Quảng Ninh" },
                    { 50, "QT", "Quảng Trị" },
                    { 51, "ST", "Sóc Trăng" },
                    { 52, "SL", "Sơn La" },
                    { 53, "TN", "Tây Ninh" },
                    { 54, "TB", "Thái Bình" },
                    { 55, "TNG", "Thái Nguyên" },
                    { 56, "TH", "Thanh Hóa" },
                    { 57, "TTH", "Thừa Thiên Huế" },
                    { 58, "TG", "Tiền Giang" },
                    { 59, "TV", "Trà Vinh" },
                    { 60, "TQ", "Tuyên Quang" },
                    { 61, "VL", "Vĩnh Long" },
                    { 62, "VP", "Vĩnh Phúc" },
                    { 63, "YB", "Yên Bái" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: 63);
        }
    }
}
