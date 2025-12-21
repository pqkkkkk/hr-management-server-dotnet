namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the reward_program table.
/// </summary>
[Migration(202512210002)]
public class M202512_002_CreateRewardProgramTable : Migration
{
    public override void Up()
    {
        Create.Table("reward_program")
            .WithColumn("reward_program_id").AsString(255).PrimaryKey()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString().Nullable()
            .WithColumn("start_date").AsDateTime().NotNullable()
            .WithColumn("end_date").AsDateTime().NotNullable()
            .WithColumn("status").AsString(50).NotNullable()
            .WithColumn("banner_url").AsString(500).Nullable();
    }

    public override void Down()
    {
        Delete.Table("reward_program");
    }
}
