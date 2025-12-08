using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResortTralaleritos.Migrations
{
    /// <inheritdoc />
    public partial class AddMainFixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Evitar excepción si la tabla ya fue eliminada por otra migración
            migrationBuilder.Sql("IF OBJECT_ID('dbo.RoomService','U') IS NOT NULL DROP TABLE [RoomService];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomService",
                columns: table => new
                {
                    RoomsRoomId = table.Column<int>(type: "int", nullable: false),
                    ServicesServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomService", x => new { x.RoomsRoomId, x.ServicesServiceId });
                    table.ForeignKey(
                        name: "FK_RoomService_Rooms_RoomsRoomId",
                        column: x => x.RoomsRoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomService_Services_ServicesServiceId",
                        column: x => x.ServicesServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomService_ServicesServiceId",
                table: "RoomService",
                column: "ServicesServiceId");
        }
    }
}
