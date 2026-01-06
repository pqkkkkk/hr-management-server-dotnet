namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Adds user_name column to user_wallet table.
/// Stores user name for display purposes without calling external API.
/// </summary>
[Migration(202601060015)]
public class M202601_015_AddUserNameToUserWallet : Migration
{
    public override void Up()
    {
        Alter.Table("user_wallet")
            .AddColumn("user_name").AsString(255).Nullable().WithDefaultValue(string.Empty);
    }

    public override void Down()
    {
        Delete.Column("user_name").FromTable("user_wallet");
    }
}
