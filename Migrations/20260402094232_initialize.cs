using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYSGES_MAGs.Migrations
{
    /// <inheritdoc />
    public partial class initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bkprdcli",
                columns: table => new
                {
                    cli = table.Column<string>(type: "text", nullable: true),
                    cpro = table.Column<string>(type: "text", nullable: true),
                    cpack = table.Column<string>(type: "text", nullable: true),
                    rsou = table.Column<string>(type: "text", nullable: true),
                    modu = table.Column<string>(type: "text", nullable: true),
                    tdos = table.Column<string>(type: "text", nullable: true),
                    ndos = table.Column<string>(type: "text", nullable: true),
                    age = table.Column<string>(type: "text", nullable: true),
                    dev = table.Column<string>(type: "text", nullable: true),
                    suf = table.Column<string>(type: "text", nullable: true),
                    utsou = table.Column<string>(type: "text", nullable: true),
                    jour = table.Column<string>(type: "text", nullable: true),
                    agef = table.Column<string>(type: "text", nullable: true),
                    devf = table.Column<string>(type: "text", nullable: true),
                    suff = table.Column<string>(type: "text", nullable: true),
                    dpe = table.Column<string>(type: "text", nullable: true),
                    nbe = table.Column<string>(type: "text", nullable: true),
                    dde = table.Column<string>(type: "text", nullable: true),
                    dex = table.Column<string>(type: "text", nullable: true),
                    qte = table.Column<string>(type: "text", nullable: true),
                    fmep = table.Column<string>(type: "text", nullable: true),
                    eve = table.Column<string>(type: "text", nullable: true),
                    obs = table.Column<string>(type: "text", nullable: true),
                    ctax = table.Column<string>(type: "text", nullable: true),
                    nanti = table.Column<string>(type: "text", nullable: true),
                    menc = table.Column<string>(type: "text", nullable: true),
                    dme = table.Column<string>(type: "text", nullable: true),
                    uti = table.Column<string>(type: "text", nullable: true),
                    dou = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dmo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    atrf = table.Column<string>(type: "text", nullable: true),
                    mhtmep = table.Column<string>(type: "text", nullable: true),
                    recfac = table.Column<string>(type: "text", nullable: true),
                    dctar = table.Column<string>(type: "text", nullable: true),
                    devmep = table.Column<string>(type: "text", nullable: true),
                    rsou_pack = table.Column<string>(type: "text", nullable: true),
                    ncp = table.Column<string>(type: "text", nullable: true),
                    ddsou = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dfsou = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ctar = table.Column<string>(type: "text", nullable: true),
                    ttar = table.Column<string>(type: "text", nullable: true),
                    ncpf = table.Column<string>(type: "text", nullable: true),
                    cnet = table.Column<string>(type: "text", nullable: true),
                    eta = table.Column<string>(type: "text", nullable: true),
                    ctr = table.Column<string>(type: "text", nullable: true),
                    cqtef = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bkprdcli");
        }
    }
}
