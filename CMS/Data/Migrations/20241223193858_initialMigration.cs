using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentityUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModel_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryModel_UserModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentPageId = table.Column<int>(type: "INTEGER", nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Link = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    Keywords = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Image = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageModel_CategoryModel_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CategoryModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageModel_PageModel_ParentPageId",
                        column: x => x.ParentPageId,
                        principalTable: "PageModel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PageModel_UserModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentModel_PageModel_PageId",
                        column: x => x.PageId,
                        principalTable: "PageModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentModel_UserModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryModel_PageModel_PageId",
                        column: x => x.PageId,
                        principalTable: "PageModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntryModel_UserModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageContentModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContentModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageContentModels_PageModel_PageId",
                        column: x => x.PageId,
                        principalTable: "PageModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RateModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RateModel_PageModel_PageId",
                        column: x => x.PageId,
                        principalTable: "PageModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RateModel_UserModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryModel_UserId",
                table: "CategoryModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentModel_PageId",
                table: "CommentModel",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentModel_UserId",
                table: "CommentModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryModel_PageId",
                table: "EntryModel",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryModel_UserId",
                table: "EntryModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PageContentModels_PageId",
                table: "PageContentModels",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageModel_CategoryId",
                table: "PageModel",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PageModel_ParentPageId",
                table: "PageModel",
                column: "ParentPageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageModel_UserId",
                table: "PageModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RateModel_PageId",
                table: "RateModel",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_RateModel_UserId",
                table: "RateModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModel_IdentityUserId",
                table: "UserModel",
                column: "IdentityUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentModel");

            migrationBuilder.DropTable(
                name: "EntryModel");

            migrationBuilder.DropTable(
                name: "PageContentModels");

            migrationBuilder.DropTable(
                name: "RateModel");

            migrationBuilder.DropTable(
                name: "PageModel");

            migrationBuilder.DropTable(
                name: "CategoryModel");

            migrationBuilder.DropTable(
                name: "UserModel");
        }
    }
}
