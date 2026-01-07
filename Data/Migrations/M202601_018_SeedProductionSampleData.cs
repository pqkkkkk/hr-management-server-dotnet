using FluentMigrator;

namespace HrManagement.Api.Data.Migrations;

/// <summary>
/// Comprehensive sample data for Production and Development environments.
/// Provides realistic data for all business scenarios and features.
/// 
/// ENVIRONMENT: Production, Development ONLY (excluded from Testing)
/// PURPOSE: Rich sample data for demos, staging, and development testing
/// EXCLUDED FROM: Testing environment (integration tests use minimal test data)
/// 
/// USER PERSONAS:
/// Managers:
///   - manager-001: Alice Johnson (existing in test data)
///   - manager-002: Bob Smith (new, production only)
/// 
/// Employees:
///   - employee-001/002/003: Existing test users  
///   - employee-004: Fiona Taylor - Zero balance user (test insufficient points)
///   - employee-005: George Miller - High performer (5500 points, top leaderboard)
///   - employee-006: Hannah Anderson - Occasional participant
///   - employee-007: Ian Martinez - Recent joiner (low points)
///   - employee-008: Julia Garcia - Regular employee (medium activity)
///   - employee-009: Kevin Lee - Part-time participant
///   - employee-010: Laura White - Rewards-focused user (high points, no activities)
/// </summary>
[Migration(20260107171500)]
public class M202601_018_SeedProductionSampleData : Migration
{
    public override void Up()
    {
        // Check environment variable to prevent seeding production data in Testing environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        // If we are in Testing environment, SKIP this migration's data seeding
        if (string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        #region Additional Reward Programs
        // =====================================================================
        // REWARD PROGRAMS - Production Sample Data
        // =====================================================================

        // Program 005: Current active program for Q1 2025
        Insert.IntoTable("reward_program").Row(new
        {
            reward_program_id = "program-005",
            name = "2025 Q1 Recognition Program",
            description = "First quarter employee recognition and rewards program",
            start_date = new DateTime(2025, 1, 1),
            end_date = new DateTime(2025, 3, 31),
            status = "ACTIVE",
            default_giving_budget = 150,
            banner_url = "https://example.com/banners/q1-2025.jpg"
        });

        // Program 006: Special event program
        Insert.IntoTable("reward_program").Row(new
        {
            reward_program_id = "program-006",
            name = "Company Anniversary Celebration",
            description = "Special rewards for company 10th anniversary",
            start_date = new DateTime(2025, 2, 1),
            end_date = new DateTime(2025, 2, 28),
            status = "ACTIVE",
            default_giving_budget = 200,
            banner_url = "https://example.com/banners/anniversary.jpg"
        });

        // Program 007: Past program for historical data
        Insert.IntoTable("reward_program").Row(new
        {
            reward_program_id = "program-007",
            name = "2024 Year-End Bonus Program",
            description = "Year-end appreciation program (completed)",
            start_date = new DateTime(2024, 11, 1),
            end_date = new DateTime(2024, 12, 31),
            status = "INACTIVE",
            default_giving_budget = 100,
            banner_url = (string?)null
        });

        #endregion

        #region Additional Reward Items
        // =====================================================================
        // REWARD ITEMS - Various price ranges and quantities
        // =====================================================================

        // Items for program-005
        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-prod-001",
            program_id = "program-005",
            name = "Premium Headphones",
            required_points = 2000f,
            quantity = 5,
            image_url = "https://example.com/items/headphones.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-prod-002",
            program_id = "program-005",
            name = "Lunch Voucher $20",
            required_points = 150f,
            quantity = 100,
            image_url = "https://example.com/items/lunch-voucher.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-prod-003",
            program_id = "program-005",
            name = "Fitness Tracker",
            required_points = 1500f,
            quantity = 8,
            image_url = "https://example.com/items/fitness-tracker.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-prod-004",
            program_id = "program-005",
            name = "Spa Day Package",
            required_points = 3000f,
            quantity = 3,
            image_url = "https://example.com/items/spa-package.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-prod-005",
            program_id = "program-005",
            name = "Tech Gadget Bundle",
            required_points = 5000f,
            quantity = 2, // Limited quantity
            image_url = "https://example.com/items/tech-bundle.jpg"
        });

        // Items for program-006 (Anniversary)
        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-prod-006",
            program_id = "program-006",
            name = "Anniversary Gift Box",
            required_points = 500f,
            quantity = 50,
            image_url = "https://example.com/items/anniversary-gift.jpg"
        });

        Insert.IntoTable("reward_item").Row(new
        {
            reward_item_id = "item-prod-007",
            program_id = "program-006",
            name = "Company Merchandise Set",
            required_points = 300f,
            quantity = 75,
            image_url = "https://example.com/items/merch-set.jpg"
        });

        #endregion

        #region Additional Reward Program Policies
        // =====================================================================
        // REWARD PROGRAM POLICIES
        // =====================================================================

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-prod-001",
            program_id = "program-005",
            policy_type = "OVERTIME",
            calculation_period = "WEEKLY",
            unit_value = 30,
            points_per_unit = 8, // Higher reward
            is_active = true
        });

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-prod-002",
            program_id = "program-005",
            policy_type = "NOT_LATE",
            calculation_period = "WEEKLY",
            unit_value = 1,
            points_per_unit = 15,
            is_active = true
        });

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-prod-003",
            program_id = "program-005",
            policy_type = "FULL_ATTENDANCE",
            calculation_period = "WEEKLY",
            unit_value = 1,
            points_per_unit = 20,
            is_active = true
        });

        Insert.IntoTable("reward_program_policy").Row(new
        {
            policy_id = "policy-prod-004",
            program_id = "program-006",
            policy_type = "NOT_LATE",
            calculation_period = "WEEKLY",
            unit_value = 1,
            points_per_unit = 25, // Bonus for anniversary month
            is_active = true
        });

        #endregion

        #region Additional User Wallets
        // =====================================================================
        // USER WALLETS - Diverse balance scenarios
        // =====================================================================

        // Manager-002 wallets
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-001",
            user_id = "manager-002",
            user_name = "Bob Smith",
            program_id = "program-005",
            personal_point = 0,
            giving_budget = 150
        });

        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-002",
            user_id = "manager-002",
            user_name = "Bob Smith",
            program_id = "program-006",
            personal_point = 200,
            giving_budget = 200
        });

        // Employee-004: Zero balance (test exchange validation)
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-003",
            user_id = "employee-004",
            user_name = "Fiona Taylor",
            program_id = "program-005",
            personal_point = 0,
            giving_budget = 0
        });

        // Employee-005: High performer (can exchange for premium items)
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-004",
            user_id = "employee-005",
            user_name = "George Miller",
            program_id = "program-005",
            personal_point = 5500,
            giving_budget = 0
        });

        // Employee-006: Medium balance
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-005",
            user_id = "employee-006",
            user_name = "Hannah Anderson",
            program_id = "program-005",
            personal_point = 1200,
            giving_budget = 0
        });

        // Employee-007: Low balance (recent joiner)
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-006",
            user_id = "employee-007",
            user_name = "Ian Martinez",
            program_id = "program-005",
            personal_point = 250,
            giving_budget = 0
        });

        // Employee-008: Regular employee
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-007",
            user_id = "employee-008",
            user_name = "Julia Garcia",
            program_id = "program-005",
            personal_point = 800,
            giving_budget = 0
        });

        // Employee-009: Part-time participant
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-008",
            user_id = "employee-009",
            user_name = "Kevin Lee",
            program_id = "program-005",
            personal_point = 450,
            giving_budget = 0
        });

        // Employee-010: Rewards-focused user (high points, no activities)
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-009",
            user_id = "employee-010",
            user_name = "Laura White",
            program_id = "program-005",
            personal_point = 3200,
            giving_budget = 0
        });

        // Additional wallets for program-006
        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-010",
            user_id = "employee-005",
            user_name = "George Miller",
            program_id = "program-006",
            personal_point = 1500,
            giving_budget = 0
        });

        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-011",
            user_id = "employee-006",
            user_name = "Hannah Anderson",
            program_id = "program-006",
            personal_point = 800,
            giving_budget = 0
        });

        Insert.IntoTable("user_wallet").Row(new
        {
            user_wallet_id = "wallet-prod-012",
            user_id = "employee-010",
            user_name = "Laura White",
            program_id = "program-006",
            personal_point = 2000,
            giving_budget = 0
        });

        #endregion

        #region Additional Point Transactions
        // =====================================================================
        // POINT TRANSACTIONS - Realistic transaction history
        // =====================================================================

        // GIFT transactions from manager-002
        for (int i = 1; i <= 15; i++)
        {
            var employee = i % 7 + 4; // Rotate through employee-004 to employee-010
            var walletId = $"wallet-prod-{employee - 1:D3}"; // Approximate wallet mapping
            if (employee == 7) walletId = "wallet-prod-006";
            if (employee == 8) walletId = "wallet-prod-007";
            if (employee == 9) walletId = "wallet-prod-008";
            if (employee == 10) walletId = "wallet-prod-009";

            Insert.IntoTable("point_transaction").Row(new
            {
                point_transaction_id = $"tx-prod-gift-{i:D3}",
                type = "GIFT",
                amount = (float)(50 + (i * 5)), // 55, 60, 65, etc.
                source_wallet_id = "wallet-prod-001",
                destination_wallet_id = walletId,
                created_at = new DateTime(2025, 1, i, 10 + (i % 8), i % 60, 0)
            });
        }

        // EXCHANGE transactions (employees redeeming points)
        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-prod-exchange-001",
            type = "EXCHANGE",
            amount = 2000f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-prod-004", // George Miller
            created_at = new DateTime(2025, 1, 10, 14, 30, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-prod-exchange-002",
            type = "EXCHANGE",
            amount = 1500f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-prod-005", // Hannah Anderson
            created_at = new DateTime(2025, 1, 12, 15, 0, 0)
        });

        Insert.IntoTable("point_transaction").Row(new
        {
            point_transaction_id = "tx-prod-exchange-003",
            type = "EXCHANGE",
            amount = 3000f,
            source_wallet_id = (string?)null,
            destination_wallet_id = "wallet-prod-009", // Laura White
            created_at = new DateTime(2025, 1, 15, 11, 0, 0)
        });

        // POLICY_REWARD transactions
        for (int i = 1; i <= 20; i++)
        {
            var employee = i % 7 + 4;
            var walletId = $"wallet-prod-{employee - 1:D3}";
            if (employee == 7) walletId = "wallet-prod-006";
            if (employee == 8) walletId = "wallet-prod-007";
            if (employee == 9) walletId = "wallet-prod-008";
            if (employee == 10) walletId = "wallet-prod-009";

            var policyType = i % 3 == 0 ? "OVERTIME" : i % 3 == 1 ? "NOT_LATE" : "FULL_ATTENDANCE";
            var amount = policyType == "OVERTIME" ? 40f : policyType == "NOT_LATE" ? 15f : 20f;

            Insert.IntoTable("point_transaction").Row(new
            {
                point_transaction_id = $"tx-prod-policy-{i:D3}",
                type = "POLICY_REWARD",
                amount = amount,
                source_wallet_id = (string?)null,
                destination_wallet_id = walletId,
                created_at = new DateTime(2025, 1, 3 + (i / 3), 8, 0, 0) // Weekly batches
            });
        }

        #endregion

        #region Item In Transactions
        // =====================================================================
        // ITEMS IN TRANSACTIONS
        // =====================================================================

        Insert.IntoTable("item_in_transaction").Row(new
        {
            item_in_transaction_id = "iit-prod-001",
            transaction_id = "tx-prod-exchange-001",
            reward_item_id = "item-prod-001", // Premium Headphones
            quantity = 1,
            total_points = 2000
        });

        Insert.IntoTable("item_in_transaction").Row(new
        {
            item_in_transaction_id = "iit-prod-002",
            transaction_id = "tx-prod-exchange-002",
            reward_item_id = "item-prod-003", // Fitness Tracker
            quantity = 1,
            total_points = 1500
        });

        Insert.IntoTable("item_in_transaction").Row(new
        {
            item_in_transaction_id = "iit-prod-003",
            transaction_id = "tx-prod-exchange-003",
            reward_item_id = "item-prod-004", // Spa Day Package
            quantity = 1,
            total_points = 3000
        });

        #endregion

        #region Additional Activities
        // =====================================================================
        // ACTIVITIES - More diverse statuses and configurations
        // =====================================================================

        // Activity 005: Open for registration
        Insert.IntoTable("activity").Row(new
        {
            activity_id = "activity-005",
            name = "2025 Health Challenge",
            activity_type = "RUNNING",
            template_id = "RUNNING_SIMPLE",
            description = "Annual company-wide health and wellness challenge",
            banner_url = "https://example.com/health-2025.jpg",
            start_date = new DateTime(2025, 2, 1),
            end_date = new DateTime(2025, 6, 30),
            status = "OPEN",
            config = @"{
                ""min_pace"": 4.0,
                ""max_pace"": 15.0,
                ""min_distance_per_log"": 1.0,
                ""max_distance_per_day"": 42.0,
                ""points_per_km"": 12,
                ""bonus_weekend_multiplier"": 1.8
            }",
            created_at = new DateTime(2025, 1, 15)
        });

        // Activity 006: In progress with many participants
        Insert.IntoTable("activity").Row(new
        {
            activity_id = "activity-006",
            name = "Marathon Training Program",
            activity_type = "RUNNING",
            template_id = "RUNNING_SIMPLE",
            description = "Prepare for the city marathon together!",
            banner_url = "https://example.com/marathon-prep.jpg",
            start_date = new DateTime(2025, 1, 10),
            end_date = new DateTime(2025, 4, 30),
            status = "IN_PROGRESS",
            config = @"{
                ""min_pace"": 4.0,
                ""max_pace"": 12.0,
                ""min_distance_per_log"": 3.0,
                ""max_distance_per_day"": 42.0,
                ""points_per_km"": 15,
                ""bonus_weekend_multiplier"": 2.0
            }",
            created_at = new DateTime(2025, 1, 1)
        });

        // Activity 007: Closed (registration ended)
        Insert.IntoTable("activity").Row(new
        {
            activity_id = "activity-007",
            name = "Sprint Challenge December",
            activity_type = "RUNNING",
            template_id = "RUNNING_SIMPLE",
            description = "Quick sprint challenge (now closed)",
            banner_url = "https://example.com/sprint-dec.jpg",
            start_date = new DateTime(2024, 12, 1),
            end_date = new DateTime(2024, 12, 20),
            status = "CLOSED",
            config = @"{
                ""min_pace"": 3.5,
                ""max_pace"": 10.0,
                ""min_distance_per_log"": 0.5,
                ""max_distance_per_day"": 10.0,
                ""points_per_km"": 20,
                ""bonus_weekend_multiplier"": 1.5
            }",
            created_at = new DateTime(2024, 11, 15)
        });

        #endregion

        #region Additional Participants
        // =====================================================================
        // PARTICIPANTS - Diverse participation patterns
        // =====================================================================

        // Participants for activity-005 (Open - registrations)
        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-001",
            activity_id = "activity-005",
            employee_id = "employee-005",
            employee_name = "George Miller",
            joined_at = new DateTime(2025, 1, 16),
            status = "ACTIVE",
            total_score = 0 // Just registered
        });

        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-002",
            activity_id = "activity-005",
            employee_id = "employee-006",
            employee_name = "Hannah Anderson",
            joined_at = new DateTime(2025, 1, 17),
            status = "PENDING", // Awaiting approval
            total_score = 0
        });

        // Participants for activity-006 (In Progress - active with scores)
        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-003",
            activity_id = "activity-006",
            employee_id = "employee-005",
            employee_name = "George Miller",
            joined_at = new DateTime(2025, 1, 10),
            status = "ACTIVE",
            total_score = 450
        });

        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-004",
            activity_id = "activity-006",
            employee_id = "employee-006",
            employee_name = "Hannah Anderson",
            joined_at = new DateTime(2025, 1, 10),
            status = "ACTIVE",
            total_score = 320
        });

        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-005",
            activity_id = "activity-006",
            employee_id = "employee-007",
            employee_name = "Ian Martinez",
            joined_at = new DateTime(2025, 1, 11),
            status = "ACTIVE",
            total_score = 180
        });

        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-006",
            activity_id = "activity-006",
            employee_id = "employee-008",
            employee_name = "Julia Garcia",
            joined_at = new DateTime(2025, 1, 11),
            status = "ACTIVE",
            total_score = 250
        });

        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-007",
            activity_id = "activity-006",
            employee_id = "employee-009",
            employee_name = "Kevin Lee",
            joined_at = new DateTime(2025, 1, 12),
            status = "ACTIVE",
            total_score = 140
        });

        // Participants for activity-007 (Closed)
        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-008",
            activity_id = "activity-007",
            employee_id = "employee-005",
            employee_name = "George Miller",
            joined_at = new DateTime(2024, 12, 1),
            status = "ACTIVE",
            total_score = 280
        });

        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-009",
            activity_id = "activity-007",
            employee_id = "employee-006",
            employee_name = "Hannah Anderson",
            joined_at = new DateTime(2024, 12, 1),
            status = "ACTIVE",
            total_score = 210
        });

        Insert.IntoTable("participant").Row(new
        {
            participant_id = "participant-prod-010",
            activity_id = "activity-007",
            employee_id = "employee-004",
            employee_name = "Fiona Taylor",
            joined_at = new DateTime(2024, 12, 2),
            status = "DISQUALIFIED", // Edge case
            total_score = 0
        });

        #endregion

        #region Additional Activity Logs
        // =====================================================================
        // ACTIVITY LOGS - Realistic submission patterns
        // =====================================================================

        // Logs for activity-006 (active participants)
        // Employee-005 (George Miller) - Top performer
        for (int i = 1; i <= 10; i++)
        {
            var status = i <= 8 ? "APPROVED" : "PENDING";
            Insert.IntoTable("activity_log").Row(new
            {
                activity_log_id = $"log-prod-george-{i:D2}",
                participant_id = "participant-prod-003",
                employee_name = "George Miller",
                distance = 8.0m + i,
                duration_minutes = 50 + (i * 2),
                proof_url = $"https://example.com/proofs/george-{i:D2}.jpg",
                log_date = new DateTime(2025, 1, 10 + i),
                status = status,
                reviewer_id = status == "APPROVED" ? "manager-002" : null,
                created_at = new DateTime(2025, 1, 10 + i, 18, 30, 0)
            });
        }

        // Employee-006 (Hannah Anderson) - Regular participant
        for (int i = 1; i <= 7; i++)
        {
            var status = i <= 5 ? "APPROVED" : i == 6 ? "REJECTED" : "PENDING";
            var rejectReason = i == 6 ? "Distance exceeds daily maximum" : null;

            Insert.IntoTable("activity_log").Row(new
            {
                activity_log_id = $"log-prod-hannah-{i:D2}",
                participant_id = "participant-prod-004",
                employee_name = "Hannah Anderson",
                distance = i == 6 ? 45.0m : 6.0m + i, // One invalid log
                duration_minutes = 40 + (i * 3),
                proof_url = $"https://example.com/proofs/hannah-{i:D2}.jpg",
                log_date = new DateTime(2025, 1, 11 + i),
                status = status,
                reject_reason = rejectReason,
                reviewer_id = status != "PENDING" ? "manager-002" : null,
                created_at = new DateTime(2025, 1, 11 + i, 19, 0, 0)
            });
        }

        // Employee-007 (Ian Martinez) - Recent joiner
        for (int i = 1; i <= 4; i++)
        {
            Insert.IntoTable("activity_log").Row(new
            {
                activity_log_id = $"log-prod-ian-{i:D2}",
                participant_id = "participant-prod-005",
                employee_name = "Ian Martinez",
                distance = 5.0m + i,
                duration_minutes = 35 + (i * 2),
                proof_url = $"https://example.com/proofs/ian-{i:D2}.jpg",
                log_date = new DateTime(2025, 1, 12 + i),
                status = "APPROVED",
                reviewer_id = "manager-002",
                created_at = new DateTime(2025, 1, 12 + i, 20, 0, 0)
            });
        }

        // Employee-008 (Julia Garcia) - Mix of statuses
        for (int i = 1; i <= 6; i++)
        {
            var status = i % 3 == 0 ? "PENDING" : i % 3 == 1 ? "APPROVED" : "REJECTED";
            var rejectReason = status == "REJECTED" ? "Invalid proof image" : null;

            Insert.IntoTable("activity_log").Row(new
            {
                activity_log_id = $"log-prod-julia-{i:D2}",
                participant_id = "participant-prod-006",
                employee_name = "Julia Garcia",
                distance = 7.0m + i,
                duration_minutes = 45 + (i * 2),
                proof_url = $"https://example.com/proofs/julia-{i:D2}.jpg",
                log_date = new DateTime(2025, 1, 13 + i),
                status = status,
                reject_reason = rejectReason,
                reviewer_id = status != "PENDING" ? "manager-002" : null,
                created_at = new DateTime(2025, 1, 13 + i, 19, 30, 0)
            });
        }

        // Employee-009 (Kevin Lee) - Part-time participant
        for (int i = 1; i <= 3; i++)
        {
            Insert.IntoTable("activity_log").Row(new
            {
                activity_log_id = $"log-prod-kevin-{i:D2}",
                participant_id = "participant-prod-007",
                employee_name = "Kevin Lee",
                distance = 4.5m + i,
                duration_minutes = 32 + (i * 3),
                proof_url = $"https://example.com/proofs/kevin-{i:D2}.jpg",
                log_date = new DateTime(2025, 1, 14 + (i * 2)),
                status = "APPROVED",
                reviewer_id = "manager-002",
                created_at = new DateTime(2025, 1, 14 + (i * 2), 21, 0, 0)
            });
        }

        // Logs for activity-007 (Closed activity)
        // Historical approved logs
        for (int i = 1; i <= 5; i++)
        {
            Insert.IntoTable("activity_log").Row(new
            {
                activity_log_id = $"log-prod-closed-{i:D2}",
                participant_id = "participant-prod-008",
                employee_name = "George Miller",
                distance = 6.0m + i,
                duration_minutes = 38 + (i * 2),
                proof_url = $"https://example.com/proofs/closed-{i:D2}.jpg",
                log_date = new DateTime(2024, 12, 2 + i),
                status = "APPROVED",
                reviewer_id = "manager-001",
                created_at = new DateTime(2024, 12, 2 + i, 18, 0, 0)
            });
        }

        #endregion
    }

    public override void Down()
    {
        // Note: FluentMigrator Delete syntax doesn't support WHERE clause filtering.
        // To rollback this migration, either:
        // 1. Drop and recreate the database
        // 2. Manually delete records using SQL with WHERE clauses
        // 3. Use Execute.Sql() with DELETE FROM ... WHERE statements

        // Since this is production sample data (not structural changes),
        // rollback is typically not needed in normal operation.
    }
}
