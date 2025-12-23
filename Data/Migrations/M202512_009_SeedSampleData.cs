namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Seeds sample data for testing and frontend development.
/// </summary>
[Migration(202512230009)]
public class M202512_009_SeedSampleData : Migration
{
    public override void Up()
    {
        #region Reward Program Sample Data
        // =====================================================================
        // REWARD PROGRAMS
        // =====================================================================

        // Program 1: Active program with full data
        Insert.IntoTable("reward_program").Row(new
        {
            reward_program_id = "program-001",
            name = "Q4 2024 Employee Recognition",
            description = "Quarterly reward program to recognize outstanding employees for Q4 2024",
            start_date = new DateTime(2024, 10, 1),
            end_date = new DateTime(2024, 12, 31),
            status = "ACTIVE",
            default_giving_budget = 100,
            banner_url = "https://example.com/banners/q4-2024.jpg"
        });

        // Program 2: Inactive program
        Insert.IntoTable("reward_program").Row(new
        {
            reward_program_id = "program-002",
            name = "Annual Awards 2024",
            description = "Annual employee recognition awards ceremony",
            start_date = new DateTime(2024, 1, 1),
            end_date = new DateTime(2024, 6, 30),
            status = "INACTIVE",
            default_giving_budget = 200,
            banner_url = (string?)null
        });

        #endregion

        #region Reward Item Sample Data
        // =====================================================================
        // REWARD ITEMS
        // =====================================================================

        // Items for Program 1
        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-001",
            program_id = "program-001",
            name = "Amazon Gift Card $50",
            required_points = 500f,
            quantity = 10,
            image_url = "https://example.com/items/amazon-gc.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-002",
            program_id = "program-001",
            name = "Coffee Voucher",
            required_points = 100f,
            quantity = 50,
            image_url = "https://example.com/items/coffee.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-003",
            program_id = "program-001",
            name = "Extra Leave Day",
            required_points = 1000f,
            quantity = 5,
            image_url = "https://example.com/items/leave.jpg"
        });

        // Items for Program 2
        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-004",
            program_id = "program-002",
            name = "Premium Gift Set",
            required_points = 2000f,
            quantity = 3,
            image_url = "https://example.com/items/premium.jpg"
        });

        // =====================================================================
        // REWARD PROGRAM POLICIES
        // =====================================================================

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-001",
            program_id = "program-001",
            policy_type = "OVERTIME",
            calculation_period = "WEEKLY",
            unit_value = 30,
            points_per_unit = 5,
            is_active = true
        });

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-002",
            program_id = "program-001",
            policy_type = "NOT_LATE",
            calculation_period = "WEEKLY",
            unit_value = 1,
            points_per_unit = 10,
            is_active = true
        });

        #endregion
        #region User Wallet Sample Data
        // =====================================================================
        // USER WALLETS
        // =====================================================================

        // Manager wallet for Program 1
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-001",
            user_id = "manager-001",
            program_id = "program-001",
            personal_point = 0,
            giving_budget = 100
        });

        // Employee wallets for Program 1
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-002",
            user_id = "employee-001",
            program_id = "program-001",
            personal_point = 500,
            giving_budget = 0
        });

        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-003",
            user_id = "employee-002",
            program_id = "program-001",
            personal_point = 200,
            giving_budget = 0
        });

        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-004",
            user_id = "employee-003",
            program_id = "program-001",
            personal_point = 150,
            giving_budget = 0
        });

        // Wallets for Program 2
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-005",
            user_id = "manager-001",
            program_id = "program-002",
            personal_point = 0,
            giving_budget = 200
        });

        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-006",
            user_id = "employee-001",
            program_id = "program-002",
            personal_point = 100,
            giving_budget = 0
        });

        #endregion
        #region Transaction Sample Data

        // =====================================================================
        // POINT TRANSACTIONS (15 transactions for pagination testing)
        // =====================================================================

        // GIFT transactions
        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-001",
            type = "GIFT",
            amount = 50f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-002",
            created_at = new DateTime(2024, 12, 1, 10, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-002",
            type = "GIFT",
            amount = 30f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-003",
            created_at = new DateTime(2024, 12, 2, 11, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-003",
            type = "GIFT",
            amount = 20f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-004",
            created_at = new DateTime(2024, 12, 3, 9, 30, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-004",
            type = "GIFT",
            amount = 40f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-002",
            created_at = new DateTime(2024, 12, 4, 14, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-005",
            type = "GIFT",
            amount = 25f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-003",
            created_at = new DateTime(2024, 12, 5, 16, 0, 0)
        });

        // EXCHANGE transactions
        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-006",
            type = "EXCHANGE",
            amount = 100f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-002",
            created_at = new DateTime(2024, 12, 6, 10, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-007",
            type = "EXCHANGE",
            amount = 500f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-002",
            created_at = new DateTime(2024, 12, 7, 11, 30, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-008",
            type = "EXCHANGE",
            amount = 100f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-003",
            created_at = new DateTime(2024, 12, 8, 15, 0, 0)
        });

        // POLICY_REWARD transactions
        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-009",
            type = "POLICY_REWARD",
            amount = 25f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-002",
            created_at = new DateTime(2024, 12, 9, 8, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-010",
            type = "POLICY_REWARD",
            amount = 30f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-003",
            created_at = new DateTime(2024, 12, 10, 8, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-011",
            type = "POLICY_REWARD",
            amount = 15f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-004",
            created_at = new DateTime(2024, 12, 11, 8, 0, 0)
        });

        // More GIFT transactions for pagination
        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-012",
            type = "GIFT",
            amount = 35f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-002",
            created_at = new DateTime(2024, 12, 12, 10, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-013",
            type = "GIFT",
            amount = 45f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-003",
            created_at = new DateTime(2024, 12, 13, 11, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-014",
            type = "GIFT",
            amount = 55f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-004",
            created_at = new DateTime(2024, 12, 14, 14, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-015",
            type = "GIFT",
            amount = 60f,
            source_wallet_id = "wallet-001",
            destination_wallet_id = "wallet-002",
            created_at = new DateTime(2024, 12, 15, 16, 0, 0)
        });

        // =====================================================================
        // ITEMS IN TRANSACTION (for EXCHANGE transactions)
        // =====================================================================

        // tx-006: 1 Coffee Voucher
        Insert.IntoTable("item_in_transaction").Row(new
        {
            item_in_transaction_id = "iit-001",
            transaction_id = "tx-006",
            reward_item_id = "item-002",
            quantity = 1,
            total_points = 100
        });

        // tx-007: 1 Amazon Gift Card
        Insert.IntoTable("item_in_transaction").Row(new
        {
            item_in_transaction_id = "iit-002",
            transaction_id = "tx-007",
            reward_item_id = "item-001",
            quantity = 1,
            total_points = 500
        });

        // tx-008: 1 Coffee Voucher
        Insert.IntoTable("item_in_transaction").Row(new
        {
            item_in_transaction_id = "iit-003",
            transaction_id = "tx-008",
            reward_item_id = "item-002",
            quantity = 1,
            total_points = 100
        });

        #endregion
    }

    public override void Down()
    {
        // Delete in reverse order due to foreign key constraints
        Delete.FromTable("item_in_transaction").AllRows();
        Delete.FromTable("point_transaction").AllRows();
        Delete.FromTable("user_wallet").AllRows();
        Delete.FromTable("reward_program_policy").AllRows();
        Delete.FromTable("reward_item").AllRows();
        Delete.FromTable("reward_program").AllRows();
    }
}
