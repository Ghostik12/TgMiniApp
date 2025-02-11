using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LanguageBot.Migrations
{
    /// <inheritdoc />
    public partial class AddWordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictionaryWord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    OriginalWord = table.Column<string>(type: "text", nullable: false),
                    TranslatedWord = table.Column<string>(type: "text", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewInterval = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryWord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictionaryWord_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryWord_UserID",
                table: "DictionaryWord",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictionaryWord");
        }
    }
}
