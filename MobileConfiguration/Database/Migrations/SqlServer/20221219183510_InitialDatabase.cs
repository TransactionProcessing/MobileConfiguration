using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileConfiguration.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConfigType = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnableAutoUpdates = table.Column<bool>(type: "bit", nullable: false),
                    HostAddresses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => new { x.Id, x.ConfigType });
                });

            migrationBuilder.CreateTable(
                name: "LoggingLevels",
                columns: table => new
                {
                    LogLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggingLevels", x => x.LogLevelId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "LoggingLevels");
        }
    }
}
