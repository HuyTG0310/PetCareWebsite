using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCareBooking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePromotionToVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Promotions_PromotionId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.RenameColumn(
                name: "PromotionId",
                table: "Bookings",
                newName: "VoucherId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_PromotionId",
                table: "Bookings",
                newName: "IX_Bookings_VoucherId");

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    EndDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    MaxUsage = table.Column<int>(type: "int", nullable: true),
                    CurrentUsage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Code",
                table: "Vouchers",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Vouchers_VoucherId",
                table: "Bookings",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Vouchers_VoucherId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "VoucherId",
                table: "Bookings",
                newName: "PromotionId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_VoucherId",
                table: "Bookings",
                newName: "IX_Bookings_PromotionId");

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentUsage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiscountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    MaxUsage = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Code",
                table: "Promotions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Promotions_PromotionId",
                table: "Bookings",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
