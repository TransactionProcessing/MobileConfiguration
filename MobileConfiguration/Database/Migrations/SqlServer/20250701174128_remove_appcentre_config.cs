
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileConfiguration.Database.Migrations.SqlServer;
/// <inheritdoc />
public partial class remove_appcentre_config : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ApplicationCentreConfigurations");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationCentreConfigurations",
            columns: table => new
            {
                ApplicationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                AndroidKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IosKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MacosKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                WindowsKey = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationCentreConfigurations", x => x.ApplicationId);
            });
    }
}
