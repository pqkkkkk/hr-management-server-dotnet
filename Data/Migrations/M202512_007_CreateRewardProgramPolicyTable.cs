namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the reward_program_policy table for auto point distribution.
/// </summary>
[Migration(202512210007)]
public class M202512_007_CreateRewardProgramPolicyTable : Migration
{
    public override void Up()
    {
        Create.Table("reward_program_policy")
            .WithColumn("policy_id").AsString(255).PrimaryKey()
            .WithColumn("program_id").AsString(255).NotNullable()
                .ForeignKey("fk_policy_program", "reward_program", "reward_program_id")
            .WithColumn("policy_type").AsString(50).NotNullable() // NOT_LATE, OVERTIME, FULL_ATTENDANCE
            .WithColumn("calculation_period").AsString(50).NotNullable().WithDefaultValue("WEEKLY")
            .WithColumn("unit_value").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("points_per_unit").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true);

        Create.Index("ix_policy_program_id")
            .OnTable("reward_program_policy")
            .OnColumn("program_id");

        Create.Index("ix_policy_type")
            .OnTable("reward_program_policy")
            .OnColumn("policy_type");
    }

    public override void Down()
    {
        Delete.Index("ix_policy_type").OnTable("reward_program_policy");
        Delete.Index("ix_policy_program_id").OnTable("reward_program_policy");
        Delete.Table("reward_program_policy");
    }
}
