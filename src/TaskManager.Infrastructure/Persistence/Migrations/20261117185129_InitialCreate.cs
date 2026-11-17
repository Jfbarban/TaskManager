using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Responsables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaLimite = table.Column<DateOnly>(type: "date", nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tareas_Responsables_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Responsables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Responsables",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ana García" },
                    { 2, "Luis Pérez" },
                    { 3, "María López" }
                });

            migrationBuilder.InsertData(
                table: "Tareas",
                columns: new[] { "Id", "Descripcion", "Estado", "FechaLimite", "ResponsableId", "Titulo" },
                values: new object[,]
                {
                    { 1, "Estructura inicial de la solución", "Hecha", new DateOnly(2026, 12, 1), 1, "Configurar el proyecto" },
                    { 2, "Entidades y relaciones", "Hecha", new DateOnly(2026, 12, 3), 1, "Diseñar el modelo de datos" },
                    { 3, "Controllers y servicios", "EnCurso", new DateOnly(2026, 12, 10), 2, "Implementar la API" },
                    { 4, "Listado, detalle y formularios", "EnCurso", new DateOnly(2026, 12, 12), 3, "Crear las páginas Razor" },
                    { 5, "Servicios e integración", "Pendiente", new DateOnly(2026, 12, 15), 2, "Escribir tests" },
                    { 6, null, "Pendiente", new DateOnly(2026, 12, 16), 1, "Redactar el README" },
                    { 7, "DataAnnotations en los DTOs", "Pendiente", new DateOnly(2026, 12, 18), 3, "Revisar validaciones" },
                    { 8, "Con más de 10 tareas", "Pendiente", new DateOnly(2026, 12, 19), 3, "Probar paginación" },
                    { 9, "400, 404, 409", "Pendiente", new DateOnly(2026, 12, 20), 2, "Revisar códigos HTTP" },
                    { 10, null, "Pendiente", new DateOnly(2026, 12, 22), 1, "Preparar demo" },
                    { 11, "Quitar usings y archivos sobrantes", "Pendiente", new DateOnly(2026, 12, 23), 2, "Limpiar código" },
                    { 12, "Último commit y push", "Pendiente", new DateOnly(2026, 12, 28), 1, "Subir versión final" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_Estado",
                table: "Tareas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_ResponsableId",
                table: "Tareas",
                column: "ResponsableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tareas");

            migrationBuilder.DropTable(
                name: "Responsables");
        }
    }
}
