using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepStyle.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminReplyToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminReply",
                table: "ContactMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepliedAt",
                table: "ContactMessages",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminReply",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "RepliedAt",
                table: "ContactMessages");
        }
    }
}
