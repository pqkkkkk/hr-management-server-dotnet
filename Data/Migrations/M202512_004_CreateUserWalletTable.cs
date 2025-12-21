namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the user_wallet table.
/// Note: user_id references User table in Spring Boot app, no FK constraint.
/// </summary>
[Migration(202512210004)]
public class M202512_004_CreateUserWalletTable : Migration
{
    public override void Up()
    {
        Create.Table("user_wallet")
            .WithColumn("user_wallet_id").AsString(255).PrimaryKey()
            .WithColumn("user_id").AsString(255).NotNullable() // No FK - references Spring Boot User
            .WithColumn("program_id").AsString(255).NotNullable()
                .ForeignKey("fk_user_wallet_program", "reward_program", "reward_program_id")
            .WithColumn("personal_point").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("giving_budget").AsInt32().NotNullable().WithDefaultValue(0);

        Create.Index("ix_user_wallet_user_id")
            .OnTable("user_wallet")
            .OnColumn("user_id");

        Create.Index("ix_user_wallet_program_id")
            .OnTable("user_wallet")
            .OnColumn("program_id");

        // Unique constraint: one wallet per user per program
        Create.Index("uq_user_wallet_user_program")
            .OnTable("user_wallet")
            .OnColumn("user_id").Ascending()
            .OnColumn("program_id").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Index("uq_user_wallet_user_program").OnTable("user_wallet");
        Delete.Index("ix_user_wallet_program_id").OnTable("user_wallet");
        Delete.Index("ix_user_wallet_user_id").OnTable("user_wallet");
        Delete.Table("user_wallet");
    }
}
