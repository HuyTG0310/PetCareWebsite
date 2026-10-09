using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCareBooking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomTypeIdToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoomTypeId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_RoomTypeId",
                table: "Services",
                column: "RoomTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_RoomTypes_RoomTypeId",
                table: "Services",
                column: "RoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_RoomTypes_RoomTypeId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_RoomTypeId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "RoomTypeId",
                table: "Services");
        }
    }
}
