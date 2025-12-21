namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the reward_item table.
/// </summary>
[Migration(202512210003)]
public class M202512_003_CreateRewardItemTable : Migration
{
    public override void Up()
    {
        Create.Table("reward_item")
            .WithColumn("reward_item_id").AsString(255).PrimaryKey()
            .WithColumn("program_id").AsString(255).NotNullable()
                .ForeignKey("fk_reward_item_program", "reward_program", "reward_program_id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("required_points").AsFloat().NotNullable().WithDefaultValue(0)
            .WithColumn("quantity").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("image_url").AsString(500).Nullable();

        Create.Index("ix_reward_item_program_id")
            .OnTable("reward_item")
            .OnColumn("program_id");
    }

    public override void Down()
    {
        Delete.Index("ix_reward_item_program_id").OnTable("reward_item");
        Delete.Table("reward_item");
    }
}
