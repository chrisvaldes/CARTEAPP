using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYSGES_MAGs.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAlreadyDownloadToTypeMag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isAlreadyDownload",
                table: "TypeMags",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isAlreadyDownload",
                table: "TypeMags");
        }
    }
}
