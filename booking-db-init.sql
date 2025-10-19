-- ===================================================================
-- BOOKING DATABASE INITIALIZATION
-- ===================================================================

CREATE TABLE IF NOT EXISTS "Bookings" (
    "Id" uuid PRIMARY KEY,
    "ResourceId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "StartTime" timestamp NOT NULL,
    "EndTime" timestamp NOT NULL,
    "TotalPrice" decimal(18,2) NOT NULL,
    "Currency" varchar(3) NOT NULL DEFAULT 'USD',
    "Status" varchar(50) NOT NULL,
    "CancellationReason" varchar(1000),
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ConfirmedAt" timestamp,
    "CancelledAt" timestamp
);

CREATE INDEX IF NOT EXISTS "IX_Bookings_ResourceId" ON "Bookings" ("ResourceId");
CREATE INDEX IF NOT EXISTS "IX_Bookings_UserId" ON "Bookings" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Bookings_Status" ON "Bookings" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Bookings_CreatedAt" ON "Bookings" ("CreatedAt");

-- Sample Booking (Optional)
INSERT INTO "Bookings" VALUES
(
    'b0000000-0000-0000-0000-000000000001',
    'a0000000-0000-0000-0000-000000000001', -- Conference Room Alpha
    '00000000-0000-0000-0000-000000000001', -- Sample User
    '2025-10-22 10:00:00',
    '2025-10-22 12:00:00',
    10000.00,
    'RUB',
    'Confirmed',
    NULL,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    NULL
) ON CONFLICT DO NOTHING;

