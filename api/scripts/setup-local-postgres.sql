-- Run in pgAdmin Query Tool (connected as "postgres" to database "postgres").
-- Execute each statement separately (select one block, then F5).

-- 1) Create login role (skip if you get "already exists")
CREATE ROLE reminder WITH LOGIN PASSWORD 'reminder';

-- 2) Create database (must be outside DO/transaction — run alone)
CREATE DATABASE reminders OWNER reminder;

-- 3) Grants
GRANT ALL PRIVILEGES ON DATABASE reminders TO reminder;
