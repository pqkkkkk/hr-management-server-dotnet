namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the activity table for storing activities.
/// </summary>
[Migration(202512250011)]
public class M202512_011_CreateActivityTable : Migration
{
    public override void Up()
    {
        Create.Table("activity")
            .WithColumn("activity_id").AsString(255).PrimaryKey()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("activity_type").AsString(50).NotNullable().WithDefaultValue("RUNNING")
            .WithColumn("template_id").AsString(100).NotNullable()
            .WithColumn("description").AsString(int.MaxValue).Nullable()
            .WithColumn("banner_url").AsString(500).Nullable()
            .WithColumn("start_date").AsDateTime().NotNullable()
            .WithColumn("end_date").AsDateTime().NotNullable()
            .WithColumn("status").AsString(50).NotNullable().WithDefaultValue("DRAFT")
            .WithColumn("config").AsCustom("jsonb").Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("ix_activity_status")
            .OnTable("activity")
            .OnColumn("status");

        Create.Index("ix_activity_type")
            .OnTable("activity")
            .OnColumn("activity_type");
    }

    public override void Down()
    {
        Delete.Index("ix_activity_type").OnTable("activity");
        Delete.Index("ix_activity_status").OnTable("activity");
        Delete.Table("activity");
    }
}
