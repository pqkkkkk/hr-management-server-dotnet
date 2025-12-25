namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the activity_log table for activity submissions.
/// </summary>
[Migration(202512250013)]
public class M202512_013_CreateActivityLogTable : Migration
{
    public override void Up()
    {
        Create.Table("activity_log")
            .WithColumn("activity_log_id").AsString(255).PrimaryKey()
            .WithColumn("participant_id").AsString(255).NotNullable()
                .ForeignKey("fk_activity_log_participant", "participant", "participant_id")
            .WithColumn("distance").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("duration_minutes").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("proof_url").AsString(500).Nullable()
            .WithColumn("log_date").AsDateTime().NotNullable()
            .WithColumn("status").AsString(50).NotNullable().WithDefaultValue("PENDING")
            .WithColumn("reject_reason").AsString(500).Nullable()
            .WithColumn("reviewer_id").AsString(255).Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("ix_activity_log_participant_id")
            .OnTable("activity_log")
            .OnColumn("participant_id");

        Create.Index("ix_activity_log_status")
            .OnTable("activity_log")
            .OnColumn("status");

        Create.Index("ix_activity_log_log_date")
            .OnTable("activity_log")
            .OnColumn("log_date");
    }

    public override void Down()
    {
        Delete.Index("ix_activity_log_log_date").OnTable("activity_log");
        Delete.Index("ix_activity_log_status").OnTable("activity_log");
        Delete.Index("ix_activity_log_participant_id").OnTable("activity_log");
        Delete.Table("activity_log");
    }
}
