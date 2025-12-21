namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the point_transaction table.
/// </summary>
[Migration(202512210005)]
public class M202512_005_CreatePointTransactionTable : Migration
{
    public override void Up()
    {
        Create.Table("point_transaction")
            .WithColumn("point_transaction_id").AsString(255).PrimaryKey()
            .WithColumn("type").AsString(50).NotNullable() // GIVE, RECEIVE, EXCHANGE
            .WithColumn("amount").AsFloat().NotNullable().WithDefaultValue(0)
            .WithColumn("source_wallet_id").AsString(255).Nullable()
                .ForeignKey("fk_point_transaction_source", "user_wallet", "user_wallet_id")
            .WithColumn("destination_wallet_id").AsString(255).Nullable()
                .ForeignKey("fk_point_transaction_destination", "user_wallet", "user_wallet_id")
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("ix_point_transaction_source")
            .OnTable("point_transaction")
            .OnColumn("source_wallet_id");

        Create.Index("ix_point_transaction_destination")
            .OnTable("point_transaction")
            .OnColumn("destination_wallet_id");

        Create.Index("ix_point_transaction_type")
            .OnTable("point_transaction")
            .OnColumn("type");

        Create.Index("ix_point_transaction_created_at")
            .OnTable("point_transaction")
            .OnColumn("created_at").Descending();
    }

    public override void Down()
    {
        Delete.Index("ix_point_transaction_created_at").OnTable("point_transaction");
        Delete.Index("ix_point_transaction_type").OnTable("point_transaction");
        Delete.Index("ix_point_transaction_destination").OnTable("point_transaction");
        Delete.Index("ix_point_transaction_source").OnTable("point_transaction");
        Delete.Table("point_transaction");
    }
}
