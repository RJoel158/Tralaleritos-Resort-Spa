using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResortTralaleritos.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomChangesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Beds",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Rooms",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Rooms",
                newName: "RegistrationDate");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RoomTypes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDate",
                table: "RoomTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "RoomTypes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "RoomTypes");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "Rooms",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "Rooms",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<int>(
                name: "RoomTypeId",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RoomTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<int>(
                name: "Beds",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Services_RoomTypeId",
                table: "Services",
                column: "RoomTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_RoomTypes_RoomTypeId",
                table: "Services",
                column: "RoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "RoomTypeId");
        }
    }
}
