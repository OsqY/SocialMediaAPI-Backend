using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ApiUserId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ApiUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Likes",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ApiUserId",
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

            migrationBuilder.CreateTable(
                name: "Post_Likes",
                columns: table => new
                {
                    LikedByUsersId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LikedPostsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post_Likes", x => new { x.LikedByUsersId, x.LikedPostsId });
                    table.ForeignKey(
                        name: "FK_Post_Likes_AspNetUsers_LikedByUsersId",
                        column: x => x.LikedByUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Post_Likes_Posts_LikedPostsId",
                        column: x => x.LikedPostsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApiUserApiUser_FollowingId",
                table: "ApiUserApiUser",
                column: "FollowingId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Likes_LikedPostsId",
                table: "Post_Likes",
                column: "LikedPostsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiUserApiUser");

            migrationBuilder.DropTable(
                name: "Post_Likes");

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ApiUserId",
                table: "AspNetUsers",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ApiUserId",
                table: "AspNetUsers",
                column: "ApiUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ApiUserId",
                table: "AspNetUsers",
                column: "ApiUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
