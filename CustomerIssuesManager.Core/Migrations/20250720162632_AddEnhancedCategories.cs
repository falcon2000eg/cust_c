using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CustomerIssuesManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "عبث بالعداد", "#e74c3c", "التلاعب في قراءات العداد أو كسره" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "توصيلات غير شرعية", "#e67e22", "توصيلات غاز غير مرخصة" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "خطأ قراءة", "#f39c12", "خطأ في قراءة العداد" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "مشكلة فواتير", "#9b59b6", "مشاكل في الفواتير والمدفوعات" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "تغيير نشاط", "#3498db", "طلب تغيير نوع النشاط" });

            migrationBuilder.InsertData(
                table: "IssueCategories",
                columns: new[] { "Id", "CategoryName", "ColorCode", "Description" },
                values: new object[,]
                {
                    { 6, "تصحيح رقم عداد", "#1abc9c", "تصحيح أرقام العدادات" },
                    { 7, "نقل رقم مشترك", "#2ecc71", "نقل الاشتراك لموقع آخر" },
                    { 8, "كسر بالشاشة", "#e74c3c", "كسر أو تلف شاشة العداد" },
                    { 9, "عطل عداد", "#c0392b", "أعطال فنية في العداد" },
                    { 10, "هدم وازالة", "#7f8c8d", "طلبات هدم أو إزالة التوصيلات" },
                    { 11, "أخرى", "#95a5a6", "مشاكل أخرى غير مصنفة" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "فواتير", "#FF6347", "مشاكل متعلقة بالفواتير والمديونية" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "صيانة", "#4682B4", "طلبات صيانة أو مشاكل فنية" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "شكاوى", "#FFD700", "شكاوى عامة من العملاء" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "استفسارات", "#90EE90", "استفسارات عامة" });

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CategoryName", "ColorCode", "Description" },
                values: new object[] { "أخرى", "#D3D3D3", "تصنيف عام للمشاكل الأخرى" });
        }
    }
}
