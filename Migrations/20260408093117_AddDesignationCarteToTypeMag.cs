using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYSGES_MAGs.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignationCarteToTypeMag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "PrixUnitCarte",
                table: "Bkmvtis",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "DesignationCarte",
                table: "Bkmvtis",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesignationCarte",
                table: "Bkmvtis");

            migrationBuilder.AlterColumn<int>(
                name: "PrixUnitCarte",
                table: "Bkmvtis",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
