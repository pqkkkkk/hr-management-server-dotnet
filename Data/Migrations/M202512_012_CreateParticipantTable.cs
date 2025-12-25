namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the participant table for activity participants.
/// </summary>
[Migration(202512250012)]
public class M202512_012_CreateParticipantTable : Migration
{
    public override void Up()
    {
        Create.Table("participant")
            .WithColumn("participant_id").AsString(255).PrimaryKey()
            .WithColumn("activity_id").AsString(255).NotNullable()
                .ForeignKey("fk_participant_activity", "activity", "activity_id")
            .WithColumn("employee_id").AsString(255).NotNullable()
            .WithColumn("joined_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("status").AsString(50).NotNullable().WithDefaultValue("ACTIVE")
            .WithColumn("total_score").AsDecimal(18, 2).NotNullable().WithDefaultValue(0);

        Create.Index("ix_participant_activity_id")
            .OnTable("participant")
            .OnColumn("activity_id");

        Create.Index("ix_participant_employee_id")
            .OnTable("participant")
            .OnColumn("employee_id");

        // Unique constraint: one employee can only join an activity once
        Create.Index("ix_participant_activity_employee")
            .OnTable("participant")
            .OnColumn("activity_id").Ascending()
            .OnColumn("employee_id").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Index("ix_participant_activity_employee").OnTable("participant");
        Delete.Index("ix_participant_employee_id").OnTable("participant");
        Delete.Index("ix_participant_activity_id").OnTable("participant");
        Delete.Table("participant");
    }
}
