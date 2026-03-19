-- Initialize database
-- This script runs automatically when PostgreSQL container starts

-- Create any required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Add any initial database setup here
-- CREATE TABLE IF NOT EXISTS example_table (...);

-- Grant permissions if needed
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;

\echo 'Database initialization complete'
