using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYSGES_MAGs.Migrations
{
    /// <inheritdoc />
    public partial class valdes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TypeMags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PeriodeDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodeFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeMags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bkmvtis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeAgence = table.Column<string>(type: "text", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    CodeIN = table.Column<string>(type: "text", nullable: true),
                    CodeDevise = table.Column<string>(type: "text", nullable: true),
                    EstActif = table.Column<string>(type: "text", nullable: true),
                    NumeroCompte = table.Column<string>(type: "text", nullable: false),
                    TypeBeneficiaire = table.Column<string>(type: "text", nullable: false),
                    ReferenceBeneficiaire = table.Column<int>(type: "integer", nullable: false),
                    CleBeneficiaire = table.Column<int>(type: "integer", nullable: false),
                    DatePrelevement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrixUnitCarte = table.Column<int>(type: "integer", nullable: false),
                    ReferenceOperation = table.Column<string>(type: "text", nullable: false),
                    CodeOperation = table.Column<string>(type: "text", nullable: false),
                    CodeEmetteur = table.Column<string>(type: "text", nullable: false),
                    IndicateurDomiciliation = table.Column<string>(type: "text", nullable: false),
                    TypeMagId = table.Column<Guid>(type: "uuid", nullable: false),
                    LibelleCarte = table.Column<string>(type: "text", nullable: false),
                    Carte = table.Column<string>(type: "text", nullable: false),
                    DateValiditeCarte = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateCreationCarte = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CodeTarif = table.Column<string>(type: "text", nullable: false),
                    CodeCarte = table.Column<string>(type: "text", nullable: false),
                    startPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    endPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bkmvtis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bkmvtis_TypeMags_TypeMagId",
                        column: x => x.TypeMagId,
                        principalTable: "TypeMags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bkmvtis_TypeMagId",
                table: "Bkmvtis",
                column: "TypeMagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bkmvtis");

            migrationBuilder.DropTable(
                name: "TypeMags");
        }
    }
}
