using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedFolloersRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiUserApiUser");

            migrationBuilder.AddColumn<string>(
                name: "UserFollowersFollowerId",
                table: "AspNetUsers",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserFollowersFollowingId",
                table: "AspNetUsers",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserFollowers",
                columns: table => new
                {
                    FollowerId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FollowingId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollowers", x => new { x.FollowerId, x.FollowingId });
                    table.ForeignKey(
                        name: "FK_UserFollowers_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFollowers_AspNetUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserFollowersFollowerId_UserFollowersFollowingId",
                table: "AspNetUsers",
                columns: new[] { "UserFollowersFollowerId", "UserFollowersFollowingId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollowers_FollowingId",
                table: "UserFollowers",
                column: "FollowingId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserFollowers_UserFollowersFollowerId_UserFollow~",
                table: "AspNetUsers",
                columns: new[] { "UserFollowersFollowerId", "UserFollowersFollowingId" },
                principalTable: "UserFollowers",
                principalColumns: new[] { "FollowerId", "FollowingId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserFollowers_UserFollowersFollowerId_UserFollow~",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserFollowers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserFollowersFollowerId_UserFollowersFollowingId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserFollowersFollowerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserFollowersFollowingId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "ApiUserApiUser",
                columns: table => new
                {
                    FollowersId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FollowingId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiUserApiUser", x => new { x.FollowersId, x.FollowingId });
                    table.ForeignKey(
                        name: "FK_ApiUserApiUser_AspNetUsers_FollowersId",
                        column: x => x.FollowersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiUserApiUser_AspNetUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApiUserApiUser_FollowingId",
                table: "ApiUserApiUser",
                column: "FollowingId");
        }
    }
}
