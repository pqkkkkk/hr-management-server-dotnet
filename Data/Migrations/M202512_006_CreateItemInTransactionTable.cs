namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Creates the item_in_transaction table (junction table for items exchanged).
/// </summary>
[Migration(202512210006)]
public class M202512_006_CreateItemInTransactionTable : Migration
{
    public override void Up()
    {
        Create.Table("item_in_transaction")
            .WithColumn("item_in_transaction_id").AsString(255).PrimaryKey()
            .WithColumn("transaction_id").AsString(255).NotNullable()
                .ForeignKey("fk_item_in_transaction_transaction", "point_transaction", "point_transaction_id")
            .WithColumn("reward_item_id").AsString(255).NotNullable()
                .ForeignKey("fk_item_in_transaction_item", "reward_item", "reward_item_id")
            .WithColumn("quantity").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("total_points").AsInt32().NotNullable().WithDefaultValue(0);

        Create.Index("ix_item_in_transaction_transaction_id")
            .OnTable("item_in_transaction")
            .OnColumn("transaction_id");

        Create.Index("ix_item_in_transaction_reward_item_id")
            .OnTable("item_in_transaction")
            .OnColumn("reward_item_id");
    }

    public override void Down()
    {
        Delete.Index("ix_item_in_transaction_reward_item_id").OnTable("item_in_transaction");
        Delete.Index("ix_item_in_transaction_transaction_id").OnTable("item_in_transaction");
        Delete.Table("item_in_transaction");
    }
}
