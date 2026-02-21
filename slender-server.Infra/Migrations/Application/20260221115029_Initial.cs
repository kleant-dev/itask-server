using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace slender_server.Infra.Migrations.Application
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "slender");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    avatar_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    last_active_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    identity_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recipient_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    actor_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    project_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body = table.Column<string>(type: "text", nullable: true),
                    entity_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    read_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.CheckConstraint("CK_Notification_SingleTarget", "(CASE WHEN \"workspace_id\" IS NOT NULL THEN 1 ELSE 0 END +\n                   CASE WHEN \"project_id\" IS NOT NULL THEN 1 ELSE 0 END +\n                   CASE WHEN \"task_id\" IS NOT NULL THEN 1 ELSE 0 END) <= 1");
                    table.ForeignKey(
                        name: "fk_notifications_users_actor_id",
                        column: x => x.actor_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notifications_users_recipient_id",
                        column: x => x.recipient_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workspaces",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    owner_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspaces", x => x.id);
                    table.ForeignKey(
                        name: "fk_workspaces_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_logs",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    actor_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_logs", x => x.id);
                    table.CheckConstraint("CK_ActivityLog_ActionNotEmpty", "LENGTH(TRIM(\"action\")) > 0");
                    table.CheckConstraint("CK_ActivityLog_EntityIdNotEmpty", "LENGTH(TRIM(\"entity_id\")) > 0");
                    table.CheckConstraint("CK_ActivityLog_EntityTypeNotEmpty", "LENGTH(TRIM(\"entity_type\")) > 0");
                    table.ForeignKey(
                        name: "fk_activity_logs_users_actor_id",
                        column: x => x.actor_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_logs_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "labels",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_labels", x => x.id);
                    table.ForeignKey(
                        name: "fk_labels_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    owner_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    icon = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_projects_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workspace_invites",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    invited_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_invites", x => x.id);
                    table.ForeignKey(
                        name: "fk_workspace_invites_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_workspace_invites_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workspace_members",
                schema: "slender",
                columns: table => new
                {
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    invited_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    joined_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_members", x => new { x.workspace_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_workspace_members_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_workspace_members_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_workspace_members_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channels",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    project_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    participant_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channels", x => x.id);
                    table.CheckConstraint("CK_Channel_NameRules", "(\"type\" = 'DirectMessage' AND \"name\" IS NULL) OR \n(\"type\" IN ('Public', 'Private') AND \"name\" IS NOT NULL)");
                    table.CheckConstraint("CK_Channel_ParticipantHashOnlyForDMs", "(\"type\" = 'DirectMessage' AND \"participant_hash\" IS NOT NULL) OR \n(\"type\" IN ('Public', 'Private') AND \"participant_hash\" IS NULL)");
                    table.CheckConstraint("CK_Channel_ProjectScope", "(\"type\" = 'DirectMessage' AND \"project_id\" IS NULL) OR \n(\"type\" IN ('Public', 'Private'))");
                    table.ForeignKey(
                        name: "fk_channels_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "slender",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_channels_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_channels_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                schema: "slender",
                columns: table => new
                {
                    project_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    added_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    joined_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_members", x => new { x.project_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_project_members_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "slender",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_members_users_added_by_user_id",
                        column: x => x.added_by_user_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_project_members_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    project_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    parent_task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    sort_order = table.Column<double>(type: "double precision", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_tasks_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "slender",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tasks_tasks_parent_task_id",
                        column: x => x.parent_task_id,
                        principalSchema: "slender",
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tasks_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_tasks_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channel_members",
                schema: "slender",
                columns: table => new
                {
                    channel_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_read_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    joined_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_members", x => new { x.channel_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_channel_members_channels_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "slender",
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_channel_members_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    channel_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    author_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reply_to_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    body = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.CheckConstraint("CK_Message_BodyNotEmpty", "LENGTH(TRIM(\"body\")) > 0");
                    table.CheckConstraint("CK_Message_NoSelfReply", "\"id\" != \"reply_to_id\"");
                    table.CheckConstraint("CK_Message_UpdatedAfterCreated", "\"updated_at_utc\" >= \"created_at_utc\"");
                    table.ForeignKey(
                        name: "fk_messages_channels_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "slender",
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_messages_reply_to_id",
                        column: x => x.reply_to_id,
                        principalSchema: "slender",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_messages_users_author_id",
                        column: x => x.author_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "calendar_events",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workspace_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    schedule_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_all_day = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_events", x => x.id);
                    table.CheckConstraint("CK_CalendarEvent_AllDayMidnight", "(\"is_all_day\" = false) OR \n          (\"is_all_day\" = true AND \n           EXTRACT(HOUR FROM \"starts_at_utc\") = 0 AND \n           EXTRACT(MINUTE FROM \"starts_at_utc\") = 0 AND \n           EXTRACT(SECOND FROM \"starts_at_utc\") = 0 AND\n           EXTRACT(HOUR FROM \"ends_at_utc\") = 0 AND \n           EXTRACT(MINUTE FROM \"ends_at_utc\") = 0 AND \n           EXTRACT(SECOND FROM \"ends_at_utc\") = 0)");
                    table.CheckConstraint("CK_CalendarEvent_EndsAfterStarts", "\"ends_at_utc\" > \"starts_at_utc\"");
                    table.CheckConstraint("CK_CalendarEvent_TitleNotEmpty", "LENGTH(TRIM(\"title\")) > 0");
                    table.CheckConstraint("CK_CalendarEvent_ValidScheduleType", "\"schedule_type\" IS NULL OR \n          \"schedule_type\" IN ('Meeting', 'Task', 'Review', 'Event', 'Reminder')");
                    table.ForeignKey(
                        name: "fk_calendar_events_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "slender",
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_calendar_events_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_calendar_events_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalSchema: "slender",
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_assignees",
                schema: "slender",
                columns: table => new
                {
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_by_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_assignees", x => new { x.task_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_task_assignees_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "slender",
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_assignees_users_assigned_by_id",
                        column: x => x.assigned_by_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_task_assignees_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_attachments",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    uploaded_by_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_attachments_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "slender",
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_attachments_users_uploaded_by_id",
                        column: x => x.uploaded_by_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "task_comments",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    author_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    parent_comment_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    body = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_comments_task_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalSchema: "slender",
                        principalTable: "task_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_task_comments_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "slender",
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_comments_users_author_id",
                        column: x => x.author_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "task_labels",
                schema: "slender",
                columns: table => new
                {
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_labels", x => new { x.task_id, x.label_id });
                    table.ForeignKey(
                        name: "fk_task_labels_labels_label_id",
                        column: x => x.label_id,
                        principalSchema: "slender",
                        principalTable: "labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_labels_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "slender",
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_comment_attachments",
                schema: "slender",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    comment_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    uploaded_by_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_comment_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_comment_attachments_task_comments_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "slender",
                        principalTable: "task_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_comment_attachments_users_uploaded_by_id",
                        column: x => x.uploaded_by_id,
                        principalSchema: "slender",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_actor_id",
                schema: "slender",
                table: "activity_logs",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_workspace_id",
                schema: "slender",
                table: "activity_logs",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedAt",
                schema: "slender",
                table: "activity_logs",
                column: "created_at_utc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Entity",
                schema: "slender",
                table: "activity_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Workspace_CreatedAt",
                schema: "slender",
                table: "activity_logs",
                columns: new[] { "workspace_id", "created_at_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Creator_StartsAt",
                schema: "slender",
                table: "calendar_events",
                columns: new[] { "created_by_id", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Task",
                schema: "slender",
                table: "calendar_events",
                column: "task_id",
                filter: "\"task_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Workspace_DateRange",
                schema: "slender",
                table: "calendar_events",
                columns: new[] { "workspace_id", "starts_at_utc", "ends_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Workspace_StartsAt",
                schema: "slender",
                table: "calendar_events",
                columns: new[] { "workspace_id", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMembers_UserId_Unread",
                schema: "slender",
                table: "channel_members",
                column: "user_id",
                filter: "\"last_read_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_channels_created_by_id",
                schema: "slender",
                table: "channels",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_participant_hash",
                schema: "slender",
                table: "channels",
                column: "participant_hash",
                filter: "\"type\" = 'DirectMessage'");

            migrationBuilder.CreateIndex(
                name: "ix_channels_project_id",
                schema: "slender",
                table: "channels",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_workspace_id",
                schema: "slender",
                table: "channels",
                column: "workspace_id",
                filter: "\"type\" = 'DirectMessage'");

            migrationBuilder.CreateIndex(
                name: "ix_channels_workspace_id_created_at_utc",
                schema: "slender",
                table: "channels",
                columns: new[] { "workspace_id", "created_at_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_channels_workspace_id_name",
                schema: "slender",
                table: "channels",
                columns: new[] { "workspace_id", "name" },
                unique: true,
                filter: "\"name\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_channels_workspace_id_participant_hash",
                schema: "slender",
                table: "channels",
                columns: new[] { "workspace_id", "participant_hash" },
                unique: true,
                filter: "\"participant_hash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_channels_workspace_id_project_id",
                schema: "slender",
                table: "channels",
                columns: new[] { "workspace_id", "project_id" },
                filter: "\"project_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_labels_workspace_id",
                schema: "slender",
                table: "labels",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_labels_workspace_id_name",
                schema: "slender",
                table: "labels",
                columns: new[] { "workspace_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Author_Created",
                schema: "slender",
                table: "messages",
                columns: new[] { "author_id", "created_at_utc" },
                descending: new[] { false, true },
                filter: "\"deleted_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Channel_Created",
                schema: "slender",
                table: "messages",
                columns: new[] { "channel_id", "created_at_utc" },
                descending: new[] { false, true },
                filter: "\"deleted_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReplyTo",
                schema: "slender",
                table: "messages",
                column: "reply_to_id",
                filter: "\"reply_to_id\" IS NOT NULL AND \"deleted_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_actor_id",
                schema: "slender",
                table: "notifications",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Recipient_Created",
                schema: "slender",
                table: "notifications",
                columns: new[] { "recipient_id", "created_at_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Unread",
                schema: "slender",
                table: "notifications",
                column: "recipient_id",
                filter: "\"read_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_project_members_added_by_user_id",
                schema: "slender",
                table: "project_members",
                column: "added_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_members_user_id",
                schema: "slender",
                table: "project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_owner_id",
                schema: "slender",
                table: "projects",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_workspace_id",
                schema: "slender",
                table: "projects",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_workspace_id_status",
                schema: "slender",
                table: "projects",
                columns: new[] { "workspace_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_task_assignees_assigned_by_id",
                schema: "slender",
                table: "task_assignees",
                column: "assigned_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignees_task_id",
                schema: "slender",
                table: "task_assignees",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignees_user_id",
                schema: "slender",
                table: "task_assignees",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_attachments_task_id",
                schema: "slender",
                table: "task_attachments",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_attachments_uploaded_by_id",
                schema: "slender",
                table: "task_attachments",
                column: "uploaded_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_comment_attachments_comment_id",
                schema: "slender",
                table: "task_comment_attachments",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_comment_attachments_uploaded_by_id",
                schema: "slender",
                table: "task_comment_attachments",
                column: "uploaded_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_comments_author_id",
                schema: "slender",
                table: "task_comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_comments_parent_comment_id",
                schema: "slender",
                table: "task_comments",
                column: "parent_comment_id",
                filter: "\"parent_comment_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_task_comments_task_id",
                schema: "slender",
                table: "task_comments",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_labels_label_id",
                schema: "slender",
                table: "task_labels",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_labels_task_id",
                schema: "slender",
                table: "task_labels",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_created_by_id",
                schema: "slender",
                table: "tasks",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_parent_task_id",
                schema: "slender",
                table: "tasks",
                column: "parent_task_id",
                filter: "\"parent_task_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_project_id_status_sort_order",
                schema: "slender",
                table: "tasks",
                columns: new[] { "project_id", "status", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_scheduled_at",
                schema: "slender",
                table: "tasks",
                column: "scheduled_at",
                filter: "\"scheduled_at\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_workspace_id_status",
                schema: "slender",
                table: "tasks",
                columns: new[] { "workspace_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "slender",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_identity_id",
                schema: "slender",
                table: "users",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspace_invites_invited_by_user_id",
                schema: "slender",
                table: "workspace_invites",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_invites_token",
                schema: "slender",
                table: "workspace_invites",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspace_invites_workspace_id_email",
                schema: "slender",
                table: "workspace_invites",
                columns: new[] { "workspace_id", "email" },
                unique: true,
                filter: "\"accepted_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_members_invited_by_user_id",
                schema: "slender",
                table: "workspace_members",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_members_user_id",
                schema: "slender",
                table: "workspace_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_owner_id",
                schema: "slender",
                table: "workspaces",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_slug",
                schema: "slender",
                table: "workspaces",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "calendar_events",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "channel_members",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "project_members",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "task_assignees",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "task_attachments",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "task_comment_attachments",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "task_labels",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "workspace_invites",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "workspace_members",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "channels",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "task_comments",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "labels",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "tasks",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "workspaces",
                schema: "slender");

            migrationBuilder.DropTable(
                name: "users",
                schema: "slender");
        }
    }
}
