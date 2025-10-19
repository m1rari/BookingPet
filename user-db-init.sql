-- ===================================================================
-- USER DATABASE INITIALIZATION (ASP.NET Core Identity)
-- ===================================================================

-- Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" uuid PRIMARY KEY,
    "UserName" varchar(256),
    "NormalizedUserName" varchar(256),
    "Email" varchar(256),
    "NormalizedEmail" varchar(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamptz,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "FirstName" varchar(100) NOT NULL DEFAULT '',
    "LastName" varchar(100) NOT NULL DEFAULT '',
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginAt" timestamp,
    "IsActive" boolean NOT NULL DEFAULT true
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_NormalizedUserName" ON "Users" ("NormalizedUserName");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_NormalizedEmail" ON "Users" ("NormalizedEmail");

-- Roles table
CREATE TABLE IF NOT EXISTS "Roles" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(256),
    "NormalizedName" varchar(256),
    "ConcurrencyStamp" text
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Roles_NormalizedName" ON "Roles" ("NormalizedName");

-- UserRoles junction table
CREATE TABLE IF NOT EXISTS "UserRoles" (
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");

-- UserClaims table
CREATE TABLE IF NOT EXISTS "UserClaims" (
    "Id" serial PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserClaims_UserId" ON "UserClaims" ("UserId");

-- RoleClaims table
CREATE TABLE IF NOT EXISTS "RoleClaims" (
    "Id" serial PRIMARY KEY,
    "RoleId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_RoleClaims_RoleId" ON "RoleClaims" ("RoleId");

-- UserLogins table
CREATE TABLE IF NOT EXISTS "UserLogins" (
    "LoginProvider" varchar(128) NOT NULL,
    "ProviderKey" varchar(128) NOT NULL,
    "ProviderDisplayName" text,
    "UserId" uuid NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserLogins_UserId" ON "UserLogins" ("UserId");

-- UserTokens table
CREATE TABLE IF NOT EXISTS "UserTokens" (
    "UserId" uuid NOT NULL,
    "LoginProvider" varchar(128) NOT NULL,
    "Name" varchar(128) NOT NULL,
    "Value" text,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- ===================================================================
-- SEED DEFAULT ROLES
-- ===================================================================

INSERT INTO "Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES
    ('10000000-0000-0000-0000-000000000001', 'Customer', 'CUSTOMER', gen_random_uuid()::text),
    ('10000000-0000-0000-0000-000000000002', 'Manager', 'MANAGER', gen_random_uuid()::text),
    ('10000000-0000-0000-0000-000000000003', 'Admin', 'ADMIN', gen_random_uuid()::text)
ON CONFLICT DO NOTHING;

-- ===================================================================
-- SEED SAMPLE USERS (Optional)
-- ===================================================================

-- Sample Admin User
-- Password: Admin@123 (hashed)
INSERT INTO "Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", 
    "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", 
    "LockoutEnabled", "AccessFailedCount", "FirstName", "LastName", "CreatedAt", "IsActive")
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'admin@bookingplatform.com',
    'ADMIN@BOOKINGPLATFORM.COM',
    'admin@bookingplatform.com',
    'ADMIN@BOOKINGPLATFORM.COM',
    true,
    'AQAAAAIAAYagAAAAEGqJ7XqJ6rZhQTqZqJ7XqJ6rZhQTqZqJ7XqJ6rZhQTqZqJ7XqJ6rZhQTqZqJ7XqJ6g==', -- Placeholder (needs real hash)
    gen_random_uuid()::text,
    gen_random_uuid()::text,
    false,
    false,
    true,
    0,
    'System',
    'Administrator',
    CURRENT_TIMESTAMP,
    true
) ON CONFLICT DO NOTHING;

-- Assign Admin role
INSERT INTO "UserRoles" ("UserId", "RoleId") VALUES
    ('00000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000003')
ON CONFLICT DO NOTHING;

-- Sample Customer User
INSERT INTO "Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed",
    "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled",
    "LockoutEnabled", "AccessFailedCount", "FirstName", "LastName", "CreatedAt", "IsActive")
VALUES (
    '00000000-0000-0000-0000-000000000002',
    'customer@test.com',
    'CUSTOMER@TEST.COM',
    'customer@test.com',
    'CUSTOMER@TEST.COM',
    true,
    'AQAAAAIAAYagAAAAEGqJ7XqJ6rZhQTqZqJ7XqJ6rZhQTqZqJ7XqJ6rZhQTqZqJ7XqJ6rZhQTqZqJ7XqJ6g==', -- Placeholder
    gen_random_uuid()::text,
    gen_random_uuid()::text,
    false,
    false,
    true,
    0,
    'John',
    'Doe',
    CURRENT_TIMESTAMP,
    true
) ON CONFLICT DO NOTHING;

-- Assign Customer role
INSERT INTO "UserRoles" ("UserId", "RoleId") VALUES
    ('00000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

