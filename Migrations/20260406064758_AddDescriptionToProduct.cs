using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYSGES_MAGs.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bkmvtis_TypeMags_TypeMagId",
                table: "Bkmvtis");

            migrationBuilder.DropIndex(
                name: "IX_Bkmvtis_TypeMagId",
                table: "Bkmvtis");

            migrationBuilder.RenameColumn(
                name: "TypeMagId",
                table: "Bkmvtis",
                newName: "TypeMag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeMag",
                table: "Bkmvtis",
                newName: "TypeMagId");

            migrationBuilder.CreateIndex(
                name: "IX_Bkmvtis_TypeMagId",
                table: "Bkmvtis",
                column: "TypeMagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bkmvtis_TypeMags_TypeMagId",
                table: "Bkmvtis",
                column: "TypeMagId",
                principalTable: "TypeMags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
