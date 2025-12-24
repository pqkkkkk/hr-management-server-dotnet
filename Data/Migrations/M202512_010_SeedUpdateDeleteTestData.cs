namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Seeds additional sample data for Update and Delete integration tests.
/// </summary>
[Migration(202512240010)]
public class M202512_010_SeedUpdateDeleteTestData : Migration
{
    public override void Up()
    {
        // =====================================================================
        // REWARD PROGRAMS FOR UPDATE/DELETE TESTS
        // =====================================================================

        // Program 3: For UPDATE tests (ACTIVE, no transactions)
        Insert.IntoTable("reward_program").Row(new
        {
            reward_program_id = "program-003",
            name = "Update Test Program",
            description = "Program for testing update functionality",
            start_date = new DateTime(2025, 1, 1),
            end_date = new DateTime(2025, 6, 30),
            status = "ACTIVE",
            default_giving_budget = 50,
            banner_url = "https://example.com/banners/update-test.jpg"
        });

        // Program 4: For DELETE tests (ACTIVE, no transactions, no wallets)
        Insert.IntoTable("reward_program").Row(new
        {
            reward_program_id = "program-004",
            name = "Delete Test Program",
            description = "Program for testing delete functionality",
            start_date = new DateTime(2025, 1, 1),
            end_date = new DateTime(2025, 6, 30),
            status = "ACTIVE",
            default_giving_budget = 30,
            banner_url = (string?)null
        });

        // =====================================================================
        // REWARD ITEMS FOR UPDATE/DELETE TESTS
        // =====================================================================

        // Items for Program 3 (Update tests)
        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-005",
            program_id = "program-003",
            name = "Update Test Item 1",
            required_points = 100f,
            quantity = 10,
            image_url = "https://example.com/items/update-1.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-006",
            program_id = "program-003",
            name = "Update Test Item 2",
            required_points = 200f,
            quantity = 5,
            image_url = "https://example.com/items/update-2.jpg"
        });

        // Items for Program 4 (Delete tests)
        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-007",
            program_id = "program-004",
            name = "Delete Test Item",
            required_points = 150f,
            quantity = 8,
            image_url = "https://example.com/items/delete.jpg"
        });

        // =====================================================================
        // POLICIES FOR UPDATE/DELETE TESTS
        // =====================================================================

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-003",
            program_id = "program-003",
            policy_type = "OVERTIME",
            calculation_period = "WEEKLY",
            unit_value = 30,
            points_per_unit = 5,
            is_active = true
        });

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-004",
            program_id = "program-004",
            policy_type = "NOT_LATE",
            calculation_period = "WEEKLY",
            unit_value = 1,
            points_per_unit = 10,
            is_active = true
        });
    }

    public override void Down()
    {
        // Delete in reverse order due to foreign key constraints
        Delete.FromTable("reward_program_policy").Row(new { policy_id = "policy-004" });
        Delete.FromTable("reward_program_policy").Row(new { policy_id = "policy-003" });
        Delete.FromTable("reward_item").Row(new { reward_item_id = "item-007" });
        Delete.FromTable("reward_item").Row(new { reward_item_id = "item-006" });
        Delete.FromTable("reward_item").Row(new { reward_item_id = "item-005" });
        Delete.FromTable("reward_program").Row(new { reward_program_id = "program-004" });
        Delete.FromTable("reward_program").Row(new { reward_program_id = "program-003" });
    }
}
