using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Triplog.Entries.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishEntrySagaState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "publish_entry_saga_state",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    current_state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_media_count = table.Column<int>(type: "integer", nullable: false),
                    finalized_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    first_failure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publish_entry_saga_state", x => x.correlation_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_publish_entry_saga_state_current_state",
                table: "publish_entry_saga_state",
                column: "current_state");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "publish_entry_saga_state");
        }
    }
}
