namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Adds employee_name column to participant table.
/// Stores employee name for display purposes without calling external API.
/// </summary>
[Migration(202601060016)]
public class M202601_016_AddEmployeeNameToParticipant : Migration
{
    public override void Up()
    {
        Alter.Table("participant")
            .AddColumn("employee_name").AsString(255).Nullable().WithDefaultValue(string.Empty);
    }

    public override void Down()
    {
        Delete.Column("employee_name").FromTable("participant");
    }
}
