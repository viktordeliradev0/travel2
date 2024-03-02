using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace travelingExperience.Migrations
{
    /// <inheritdoc />
    public partial class savedTravels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSavedTravel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TravelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSavedTravel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSavedTravel_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id"
                 );
                    table.ForeignKey(
                        name: "FK_UserSavedTravel_Travels_TravelId",
                        column: x => x.TravelId,
                        principalTable: "Travels",
                        principalColumn: "Id")
                        ;
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedTravel_TravelId",
                table: "UserSavedTravel",
                column: "TravelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedTravel_UserId",
                table: "UserSavedTravel",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSavedTravel");
        }
    }
}
