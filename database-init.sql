-- ===================================================================
-- INVENTORY DATABASE INITIALIZATION
-- ===================================================================

-- Connect to InventoryDB
-- \c InventoryDB

CREATE TABLE IF NOT EXISTS "Resources" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(200) NOT NULL,
    "Description" varchar(2000),
    "Type" varchar(50) NOT NULL,
    "PricePerHour" decimal(18,2) NOT NULL,
    "Status" varchar(50) NOT NULL,
    "Address" varchar(500) NOT NULL,
    "City" varchar(100) NOT NULL,
    "Country" varchar(100) NOT NULL,
    "PostalCode" varchar(20),
    "Latitude" double precision,
    "Longitude" double precision,
    "MaxPeople" int NOT NULL,
    "MinPeople" int NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Resources_Type" ON "Resources" ("Type");
CREATE INDEX IF NOT EXISTS "IX_Resources_Status" ON "Resources" ("Status");

CREATE TABLE IF NOT EXISTS "TimeSlots" (
    "ResourceId" uuid NOT NULL,
    "StartTime" timestamp NOT NULL,
    "EndTime" timestamp NOT NULL,
    "Status" varchar(50) NOT NULL,
    FOREIGN KEY ("ResourceId") REFERENCES "Resources" ("Id") ON DELETE CASCADE
);

-- ===================================================================
-- BOOKING DATABASE INITIALIZATION  
-- ===================================================================

-- Connect to BookingDB
-- \c BookingDB

CREATE TABLE IF NOT EXISTS "Bookings" (
    "Id" uuid PRIMARY KEY,
    "ResourceId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "StartTime" timestamp NOT NULL,
    "EndTime" timestamp NOT NULL,
    "TotalPrice" decimal(18,2) NOT NULL,
    "Currency" varchar(3) NOT NULL,
    "Status" varchar(50) NOT NULL,
    "CancellationReason" varchar(1000),
    "CreatedAt" timestamp NOT NULL,
    "ConfirmedAt" timestamp,
    "CancelledAt" timestamp,
    "xmin" xid NOT NULL  -- PostgreSQL optimistic concurrency
);

CREATE INDEX IF NOT EXISTS "IX_Bookings_ResourceId" ON "Bookings" ("ResourceId");
CREATE INDEX IF NOT EXISTS "IX_Bookings_UserId" ON "Bookings" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Bookings_Status" ON "Bookings" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Bookings_CreatedAt" ON "Bookings" ("CreatedAt");

-- ===================================================================
-- SAMPLE DATA (Optional)
-- ===================================================================

-- Sample Resources
INSERT INTO "Resources" VALUES
(
    'a0000000-0000-0000-0000-000000000001',
    'Конференц-зал "Альфа"',
    'Просторный зал с современным оборудованием для презентаций и конференций',
    'ConferenceRoom',
    5000.00,
    'Active',
    'ул. Пушкина, 10',
    'Москва',
    'Россия',
    '123456',
    55.7558,
    37.6173,
    50,
    10
) ON CONFLICT DO NOTHING;

INSERT INTO "Resources" VALUES
(
    'a0000000-0000-0000-0000-000000000002',
    'Коворкинг "Бета"',
    'Открытое пространство для командной работы',
    'CoworkingSpace',
    2000.00,
    'Active',
    'ул. Ленина, 5',
    'Москва',
    'Россия',
    '123457',
    55.7600,
    37.6200,
    20,
    5
) ON CONFLICT DO NOTHING;

INSERT INTO "Resources" VALUES
(
    'a0000000-0000-0000-0000-000000000003',
    'Спортивный зал "Гамма"',
    'Современный спортивный зал с полным оборудованием',
    'SportsField',
    3000.00,
    'Active',
    'ул. Спортивная, 15',
    'Москва',
    'Россия',
    '123458',
    55.7650,
    37.6250,
    30,
    10
) ON CONFLICT DO NOTHING;

