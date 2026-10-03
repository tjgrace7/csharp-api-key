IF DB_ID('ApiKeyDb') IS NULL
    CREATE DATABASE ApiKeyDb;
GO

USE ApiKeyDb;
GO

IF OBJECT_ID('dbo.Person', 'U') IS NULL
CREATE TABLE dbo.Person (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    username      NVARCHAR(200) NOT NULL UNIQUE,
    password_hash NVARCHAR(200) NOT NULL,
    role          NVARCHAR(50) NOT NULL
);
GO

IF OBJECT_ID('dbo.Ticket', 'U') IS NULL
CREATE TABLE dbo.Ticket (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    priority   NVARCHAR(10) NOT NULL
        CONSTRAINT df_ticket_priority DEFAULT 'low'
        CONSTRAINT ck_ticket_priority CHECK (priority IN ('low', 'medium', 'high')),
    type       NVARCHAR(10) NOT NULL
        CONSTRAINT ck_ticket_type CHECK (type IN ('bug', 'feature', 'question', 'task')),
    status     NVARCHAR(15) NOT NULL
        CONSTRAINT df_ticket_status DEFAULT 'submitted'
        CONSTRAINT ck_ticket_status CHECK (status IN ('submitted', 'in_progress', 'completed')),
    created_by INT NOT NULL
        CONSTRAINT fk_ticket_created_by REFERENCES dbo.Person(id),
    updated_by INT NULL
        CONSTRAINT fk_ticket_updated_by REFERENCES dbo.Person(id),
    notes   TEXT
);
GO
IF OBJECT_ID('dbo.TicketUpdate', 'U') IS NULL
CREATE TABLE dbo.TicketUpdate(
    id INT IDENTITY(1,1) PRIMARY KEY,
    ticket_id INT NOT NULL
        CONSTRAINT fk_ticketupdate_ticket REFERENCES dbo.Ticket(id),
    updated_by INT NOT NULL
        CONSTRAINT fk_ticketupdate_person REFERENCES dbo.Person(id),
    updated_at DATETIME2 NOT NULL
        CONSTRAINT df_ticketupdate_at DEFAULT SYSUTCDATETIME(),
    note NVARCHAR(500) NULL
)
GO

INSERT INTO dbo.Person (username, password_hash, role)
    VALUES ('admin', '$2a$12$OS02QD0e6x77t.XlMHhOBu3EsGDAIu5x1Rxt/ECLuEU/QXcM0ouvq', 'IT')
GO