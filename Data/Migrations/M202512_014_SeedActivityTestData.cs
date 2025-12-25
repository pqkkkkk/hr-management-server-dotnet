using FluentMigrator;

namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Seeds sample Activity data for integration tests.
/// Creates activities, participants, and activity logs.
/// </summary>
[Migration(202512250014)]
public class M202512_014_SeedActivityTestData : Migration
{
    public override void Up()
    {
        // =============================================================================
        // ACTIVITIES
        // =============================================================================

        // Activity 001: Running activity (IN_PROGRESS) - for log submission tests
        Insert.IntoTable("activity").Row(new
        {
            activity_id = "activity-001",
            name = "Q4 2024 Running Challenge",
            activity_type = "RUNNING",
            template_id = "RUNNING_SIMPLE",
            description = "Run together, grow together!",
            banner_url = "https://example.com/running-banner.jpg",
            start_date = new DateTime(2024, 10, 1),
            end_date = new DateTime(2024, 12, 31),
            status = "IN_PROGRESS",
            config = @"{
                ""min_pace"": 4.0,
                ""max_pace"": 15.0,
                ""min_distance_per_log"": 1.0,
                ""max_distance_per_day"": 42.0,
                ""points_per_km"": 10,
                ""bonus_weekend_multiplier"": 1.5
            }",
            created_at = new DateTime(2024, 9, 15)
        });

        // Activity 002: Running activity (OPEN) - for registration tests
        Insert.IntoTable("activity").Row(new
        {
            activity_id = "activity-002",
            name = "New Year Running 2025",
            activity_type = "RUNNING",
            template_id = "RUNNING_SIMPLE",
            description = "Start the new year with running!",
            banner_url = "https://example.com/newyear-banner.jpg",
            start_date = new DateTime(2025, 1, 1),
            end_date = new DateTime(2025, 3, 31),
            status = "OPEN",
            config = @"{
                ""min_pace"": 4.0,
                ""max_pace"": 15.0,
                ""min_distance_per_log"": 1.0,
                ""max_distance_per_day"": 42.0,
                ""points_per_km"": 15,
                ""bonus_weekend_multiplier"": 2.0
            }",
            created_at = new DateTime(2024, 12, 1)
        });

        // Activity 003: Running activity (COMPLETED) - for archive/stats tests
        Insert.IntoTable("activity").Row(new
        {
            activity_id = "activity-003",
            name = "Summer Running 2024",
            activity_type = "RUNNING",
            template_id = "RUNNING_SIMPLE",
            description = "Summer challenge completed!",
            banner_url = "https://example.com/summer-banner.jpg",
            start_date = new DateTime(2024, 6, 1),
            end_date = new DateTime(2024, 8, 31),
            status = "COMPLETED",
            config = @"{
                ""min_pace"": 4.0,
                ""max_pace"": 15.0,
                ""min_distance_per_log"": 1.0,
                ""max_distance_per_day"": 42.0,
                ""points_per_km"": 10,
                ""bonus_weekend_multiplier"": 1.5
            }",
            created_at = new DateTime(2024, 5, 15)
        });

        // Activity 004: Draft activity (for delete tests)
        Insert.IntoTable("activity").Row(new
        {
            activity_id = "activity-004",
            name = "Draft Activity - Safe to Delete",
            activity_type = "RUNNING",
            template_id = "RUNNING_SIMPLE",
            description = "This activity can be deleted in tests",
            start_date = new DateTime(2025, 6, 1),
            end_date = new DateTime(2025, 8, 31),
            status = "DRAFT",
            config = @"{
                ""min_pace"": 4.0,
                ""max_pace"": 15.0,
                ""min_distance_per_log"": 1.0,
                ""max_distance_per_day"": 42.0,
                ""points_per_km"": 10,
                ""bonus_weekend_multiplier"": 1.5
            }",
            created_at = new DateTime(2024, 12, 15)
        });

        // =============================================================================
        // PARTICIPANTS
        // =============================================================================

        // Participant 001: employee-001 in activity-001 (top scorer)
        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-001",
            activity_id = "activity-001",
            employee_id = "employee-001",
            joined_at = new DateTime(2024, 10, 1),
            status = "ACTIVE",
            total_score = 150
        });

        // Participant 002: employee-002 in activity-001 (second place)
        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-002",
            activity_id = "activity-001",
            employee_id = "employee-002",
            joined_at = new DateTime(2024, 10, 2),
            status = "ACTIVE",
            total_score = 100
        });

        // Participant 003: employee-003 in activity-001 (third place)
        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-003",
            activity_id = "activity-001",
            employee_id = "employee-003",
            joined_at = new DateTime(2024, 10, 3),
            status = "ACTIVE",
            total_score = 50
        });

        // Participant 004: employee-001 in activity-003 (completed activity)
        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-004",
            activity_id = "activity-003",
            employee_id = "employee-001",
            joined_at = new DateTime(2024, 6, 1),
            status = "ACTIVE",
            total_score = 200
        });

        // =============================================================================
        // ACTIVITY LOGS
        // =============================================================================

        // Log 001: participant-001, approved log (contributes to score)
        Insert.IntoTable("activity_log").Row(new
        {
            activity_log_id = "log-001",
            participant_id = "participant-001",
            distance = 10.0m,
            duration_minutes = 60,
            proof_url = "https://example.com/proof-001.jpg",
            log_date = new DateTime(2024, 10, 15),
            status = "APPROVED",
            reviewer_id = "manager-001",
            created_at = new DateTime(2024, 10, 15)
        });

        // Log 002: participant-001, pending log (awaiting review)
        Insert.IntoTable("activity_log").Row(new
        {
            activity_log_id = "log-002",
            participant_id = "participant-001",
            distance = 5.0m,
            duration_minutes = 30,
            proof_url = "https://example.com/proof-002.jpg",
            log_date = new DateTime(2024, 10, 20),
            status = "PENDING",
            created_at = new DateTime(2024, 10, 20)
        });

        // Log 003: participant-002, rejected log
        Insert.IntoTable("activity_log").Row(new
        {
            activity_log_id = "log-003",
            participant_id = "participant-002",
            distance = 50.0m,
            duration_minutes = 100,
            proof_url = "https://example.com/proof-003.jpg",
            log_date = new DateTime(2024, 10, 18),
            status = "REJECTED",
            reject_reason = "Distance exceeds daily maximum",
            reviewer_id = "manager-001",
            created_at = new DateTime(2024, 10, 18)
        });

        // Log 004: participant-002, approved log
        Insert.IntoTable("activity_log").Row(new
        {
            activity_log_id = "log-004",
            participant_id = "participant-002",
            distance = 8.0m,
            duration_minutes = 48,
            proof_url = "https://example.com/proof-004.jpg",
            log_date = new DateTime(2024, 10, 22),
            status = "APPROVED",
            reviewer_id = "manager-001",
            created_at = new DateTime(2024, 10, 22)
        });

        // Log 005: participant-003, pending log
        Insert.IntoTable("activity_log").Row(new
        {
            activity_log_id = "log-005",
            participant_id = "participant-003",
            distance = 3.0m,
            duration_minutes = 20,
            proof_url = "https://example.com/proof-005.jpg",
            log_date = new DateTime(2024, 10, 25),
            status = "PENDING",
            created_at = new DateTime(2024, 10, 25)
        });
    }

    public override void Down()
    {
        Delete.FromTable("activity_log").Row(new { activity_log_id = "log-001" });
        Delete.FromTable("activity_log").Row(new { activity_log_id = "log-002" });
        Delete.FromTable("activity_log").Row(new { activity_log_id = "log-003" });
        Delete.FromTable("activity_log").Row(new { activity_log_id = "log-004" });
        Delete.FromTable("activity_log").Row(new { activity_log_id = "log-005" });

        Delete.FromTable("participant").Row(new { participant_id = "participant-001" });
        Delete.FromTable("participant").Row(new { participant_id = "participant-002" });
        Delete.FromTable("participant").Row(new { participant_id = "participant-003" });
        Delete.FromTable("participant").Row(new { participant_id = "participant-004" });

        Delete.FromTable("activity").Row(new { activity_id = "activity-001" });
        Delete.FromTable("activity").Row(new { activity_id = "activity-002" });
        Delete.FromTable("activity").Row(new { activity_id = "activity-003" });
        Delete.FromTable("activity").Row(new { activity_id = "activity-004" });
    }
}
