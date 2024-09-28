CREATE TABLE Database
(
    Name            TEXT PRIMARY KEY NOT NULL,
    SourceFolder    TEXT             NOT NULL,
    EncryptedFolder TEXT             NOT NULL,
    Node            TEXT,                      -- JSON string
    EncryptScheme   TEXT             NOT NULL, -- JSON string
    Password        TEXT             NOT NULL,
    DbType          TEXT             NOT NULL, -- Enum
    EncDepth        INTEGER,
    CreatedAt       TEXT             NOT NULL, -- DateTime as TEXT
    UpdatedAt       TEXT             NOT NULL  -- DateTime as TEXT
);

CREATE INDEX idx_created_at ON Database (CreatedAt);
CREATE INDEX idx_updated_at ON Database (UpdatedAt);