using FluentMigrator;

namespace HrManagement.Api.Data.Migrations;

[Migration(202601060017)]
public class M202601_017_AddEmployeeNameToActivityLog : Migration
{
    public override void Up()
    {
        Alter.Table("activity_log")
            .AddColumn("employee_name").AsString(255).Nullable().WithDefaultValue(string.Empty);
    }

    public override void Down()
    {
        Delete.Column("employee_name").FromTable("activity_log");
    }
}
