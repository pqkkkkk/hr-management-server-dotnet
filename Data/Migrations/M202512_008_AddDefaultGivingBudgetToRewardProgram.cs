namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Adds default_giving_budget column to reward_program table.
/// </summary>
[Migration(202512210008)]
public class M202512_008_AddDefaultGivingBudgetToRewardProgram : Migration
{
    public override void Up()
    {
        Alter.Table("reward_program")
            .AddColumn("default_giving_budget").AsInt32().NotNullable().WithDefaultValue(100);
    }

    public override void Down()
    {
        Delete.Column("default_giving_budget").FromTable("reward_program");
    }
}
