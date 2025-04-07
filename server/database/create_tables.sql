CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE Products (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL UNIQUE, -- 追加された製品コード
    Name TEXT NOT NULL,
    Description TEXT,
    Category TEXT,
    Price REAL NOT NULL,
    Stock INTEGER NOT NULL,
    SKU TEXT NOT NULL UNIQUE,
    IsActive INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    Public01 TEXT,
    Public02 TEXT,
    Public03 TEXT,
    Public04 TEXT,
    Public05 TEXT,
    Public06 TEXT,
    Public07 TEXT,
    Public08 TEXT,
    Public09 TEXT,
    Public10 TEXT
);

-- 新しく追加する製品マスタ
CREATE TABLE ProductMaster (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Price REAL NOT NULL,
    CreatedAt TEXT NOT NULL
);