-- ===================================================================
-- PAYMENT DATABASE INITIALIZATION (SQL Server)
-- ===================================================================

CREATE TABLE Payments (
    Id uniqueidentifier PRIMARY KEY,
    BookingId uniqueidentifier NOT NULL,
    UserId uniqueidentifier NOT NULL,
    Amount decimal(18,2) NOT NULL,
    Currency varchar(3) NOT NULL DEFAULT 'USD',
    Status varchar(50) NOT NULL,
    ExternalTransactionId varchar(200),
    PaymentMethod varchar(50),
    FailureReason varchar(1000),
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt datetime2,
    RefundedAt datetime2
);

CREATE INDEX IX_Payments_BookingId ON Payments (BookingId);
CREATE INDEX IX_Payments_UserId ON Payments (UserId);
CREATE INDEX IX_Payments_Status ON Payments (Status);
CREATE INDEX IX_Payments_ExternalTransactionId ON Payments (ExternalTransactionId);

-- Sample Payment (Optional)
INSERT INTO Payments VALUES (
    NEWID(),
    'b0000000-0000-0000-0000-000000000001', -- Sample booking
    '00000000-0000-0000-0000-000000000001', -- Sample user
    10000.00,
    'RUB',
    'Completed',
    'TXN_sample123',
    'CreditCard',
    NULL,
    GETUTCDATE(),
    GETUTCDATE(),
    NULL
);

