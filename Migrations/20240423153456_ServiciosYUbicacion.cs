using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PruebaTecnicaLisit.Migrations
{
    public partial class ServiciosYUbicacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdComuna",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Paises",
                columns: table => new
                {
                    IdPais = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paises", x => x.IdPais);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    IdServicio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.IdServicio);
                });

            migrationBuilder.CreateTable(
                name: "Regiones",
                columns: table => new
                {
                    IdRegion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPais = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regiones", x => x.IdRegion);
                    table.ForeignKey(
                        name: "FK_Regiones_Paises_IdPais",
                        column: x => x.IdPais,
                        principalTable: "Paises",
                        principalColumn: "IdPais",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiciosUsuario",
                columns: table => new
                {
                    IdAsignación = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdServicio = table.Column<int>(type: "int", nullable: false),
                    Año = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosUsuario", x => x.IdAsignación);
                    table.ForeignKey(
                        name: "FK_ServiciosUsuario_AspNetUsers_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiciosUsuario_Servicios_IdServicio",
                        column: x => x.IdServicio,
                        principalTable: "Servicios",
                        principalColumn: "IdServicio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comunas",
                columns: table => new
                {
                    IdComuna = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRegion = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunas", x => x.IdComuna);
                    table.ForeignKey(
                        name: "FK_Comunas_Regiones_IdRegion",
                        column: x => x.IdRegion,
                        principalTable: "Regiones",
                        principalColumn: "IdRegion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiciosComuna",
                columns: table => new
                {
                    ComunasIdComuna = table.Column<int>(type: "int", nullable: false),
                    ServiciosIdServicio = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosComuna", x => new { x.ComunasIdComuna, x.ServiciosIdServicio });
                    table.ForeignKey(
                        name: "FK_ServiciosComuna_Comunas_ComunasIdComuna",
                        column: x => x.ComunasIdComuna,
                        principalTable: "Comunas",
                        principalColumn: "IdComuna",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiciosComuna_Servicios_ServiciosIdServicio",
                        column: x => x.ServiciosIdServicio,
                        principalTable: "Servicios",
                        principalColumn: "IdServicio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IdComuna",
                table: "AspNetUsers",
                column: "IdComuna");

            migrationBuilder.CreateIndex(
                name: "IX_Comunas_IdRegion",
                table: "Comunas",
                column: "IdRegion");

            migrationBuilder.CreateIndex(
                name: "IX_Regiones_IdPais",
                table: "Regiones",
                column: "IdPais");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosComuna_ServiciosIdServicio",
                table: "ServiciosComuna",
                column: "ServiciosIdServicio");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosUsuario_IdServicio",
                table: "ServiciosUsuario",
                column: "IdServicio");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosUsuario_IdUsuario",
                table: "ServiciosUsuario",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Comunas_IdComuna",
                table: "AspNetUsers",
                column: "IdComuna",
                principalTable: "Comunas",
                principalColumn: "IdComuna",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Comunas_IdComuna",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ServiciosComuna");

            migrationBuilder.DropTable(
                name: "ServiciosUsuario");

            migrationBuilder.DropTable(
                name: "Comunas");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Regiones");

            migrationBuilder.DropTable(
                name: "Paises");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IdComuna",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdComuna",
                table: "AspNetUsers");
        }
    }
}
