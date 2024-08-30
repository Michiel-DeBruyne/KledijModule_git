using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedFavorietenUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favorieten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GebruikerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorieten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorieten_Gebruikers_GebruikerId",
                        column: x => x.GebruikerId,
                        principalTable: "Gebruikers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorieten_Producten_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Producten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favorieten_GebruikerId_ProductId",
                table: "Favorieten",
                columns: new[] { "GebruikerId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorieten_ProductId",
                table: "Favorieten",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorieten");
        }
    }
}
