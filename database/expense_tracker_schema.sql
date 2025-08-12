-- =====================================================
-- Expense Tracker Database Schema - PostgreSQL
-- =====================================================

-- Create database if it doesn't exist
-- Note: This must be run as a superuser or database owner
-- If running this script via psql, connect to postgres database first

-- Drop the database if it exists (be careful in production!)
DROP DATABASE IF EXISTS expense_tracker;

-- Create the database
CREATE DATABASE expense_tracker
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Connect to the new database
\c expense_tracker;

-- Drop existing tables if they exist (in reverse dependency order)
DROP TABLE IF EXISTS transaction_tags CASCADE;
DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS tags CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS user_sessions CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Drop existing types if they exist
DROP TYPE IF EXISTS transaction_type CASCADE;
DROP TYPE IF EXISTS category_type CASCADE;
DROP TYPE IF EXISTS recurring_frequency CASCADE;

-- =====================================================
-- Create Custom Types (Enums)
-- =====================================================

-- Transaction types
CREATE TYPE transaction_type AS ENUM ('EXPENSE', 'INCOME');

-- Category types
CREATE TYPE category_type AS ENUM ('EXPENSE', 'INCOME', 'BOTH');

-- Recurring frequency
CREATE TYPE recurring_frequency AS ENUM ('WEEKLY', 'MONTHLY', 'QUARTERLY', 'YEARLY');

-- =====================================================
-- Create Tables
-- =====================================================

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- User Sessions table (for JWT token management)
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token TEXT NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Categories table
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE, -- Made nullable for default categories
    name VARCHAR(100) NOT NULL,
    description TEXT,
    color VARCHAR(7) NOT NULL DEFAULT '#3498db',
    icon VARCHAR(50),
    type TEXT NOT NULL DEFAULT 'EXPENSE',
    is_default BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    -- Constraint to ensure unique names per user, but allow multiple NULL user_ids for default categories
    UNIQUE(user_id, name)
);

-- Tags table
CREATE TABLE tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    color VARCHAR(7) DEFAULT '#6c757d',
    usage_count INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, name) -- Each user can have unique tag names
);

-- Transactions table
CREATE TABLE transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    category_id UUID NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    amount DECIMAL(12, 2) NOT NULL CHECK (amount > 0),
    description VARCHAR(255) NOT NULL,
    notes TEXT,
    type TEXT NOT NULL,
    transaction_date DATE NOT NULL DEFAULT CURRENT_DATE,
    
    -- Recurring transaction fields
    is_recurring BOOLEAN DEFAULT FALSE,
    recurring_frequency TEXT,
    recurring_end_date DATE,
    parent_transaction_id UUID REFERENCES transactions(id) ON DELETE SET NULL,
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT valid_recurring_data CHECK (
        (is_recurring = FALSE) OR 
        (is_recurring = TRUE AND recurring_frequency IS NOT NULL)
    ),
    CONSTRAINT valid_recurring_end_date CHECK (
        (recurring_end_date IS NULL) OR 
        (recurring_end_date > transaction_date)
    )
);

-- Transaction Tags junction table (many-to-many relationship)
CREATE TABLE transaction_tags (
    transaction_id UUID NOT NULL REFERENCES transactions(id) ON DELETE CASCADE,
    tag_id UUID NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    PRIMARY KEY (transaction_id, tag_id)
);

-- =====================================================
-- Create Indexes for Performance
-- =====================================================

-- Users indexes
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_active ON users(is_active);

-- User Sessions indexes
CREATE INDEX idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_token ON user_sessions(token);
CREATE INDEX idx_user_sessions_expires_at ON user_sessions(expires_at);
CREATE INDEX idx_user_sessions_active ON user_sessions(is_active);
CREATE INDEX idx_user_sessions_user_active ON user_sessions(user_id, is_active);

-- Categories indexes
CREATE INDEX idx_categories_user_id ON categories(user_id);
CREATE INDEX idx_categories_type ON categories(type);
CREATE INDEX idx_categories_default ON categories(is_default);

-- Tags indexes
CREATE INDEX idx_tags_user_id ON tags(user_id);
CREATE INDEX idx_tags_usage_count ON tags(usage_count DESC);

-- Transactions indexes
CREATE INDEX idx_transactions_user_id ON transactions(user_id);
CREATE INDEX idx_transactions_category_id ON transactions(category_id);
CREATE INDEX idx_transactions_date ON transactions(transaction_date);
CREATE INDEX idx_transactions_type ON transactions(type);
CREATE INDEX idx_transactions_amount ON transactions(amount);
CREATE INDEX idx_transactions_recurring ON transactions(is_recurring);
CREATE INDEX idx_transactions_parent ON transactions(parent_transaction_id);

-- Composite indexes for common queries
CREATE INDEX idx_transactions_user_date ON transactions(user_id, transaction_date DESC);
CREATE INDEX idx_transactions_user_type ON transactions(user_id, type);
CREATE INDEX idx_transactions_user_category ON transactions(user_id, category_id);

-- =====================================================
-- Create Functions and Triggers
-- =====================================================

-- Function to update the updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Apply update triggers to tables with updated_at columns
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_user_sessions_updated_at BEFORE UPDATE ON user_sessions 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_categories_updated_at BEFORE UPDATE ON categories 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_transactions_updated_at BEFORE UPDATE ON transactions 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Function to update tag usage count
CREATE OR REPLACE FUNCTION update_tag_usage()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE tags SET usage_count = usage_count + 1 WHERE id = NEW.tag_id;
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE tags SET usage_count = GREATEST(usage_count - 1, 0) WHERE id = OLD.tag_id;
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ language 'plpgsql';

-- Trigger to automatically update tag usage counts
CREATE TRIGGER update_tag_usage_trigger
    AFTER INSERT OR DELETE ON transaction_tags
    FOR EACH ROW EXECUTE FUNCTION update_tag_usage();

-- =====================================================
-- Insert Sample Data
-- =====================================================

-- Insert sample user (password is "password123" hashed with bcrypt)
INSERT INTO users (id, username, email, password_hash, first_name, last_name) VALUES 
(
    '550e8400-e29b-41d4-a716-446655440000',
    'testuser',
    'test@example.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewGlrCLDqNjk1uva', -- password123
    'Test',
    'User'
);

-- Insert default categories (available to all users - user_id is NULL)
INSERT INTO categories (id, user_id, name, description, color, icon, type, is_default) VALUES 
-- Expense categories
('550e8400-e29b-41d4-a716-446655440001', NULL, 'Food & Dining', 'Restaurants, groceries, and food expenses', '#FF6B6B', 'restaurant', 'EXPENSE', TRUE),
('550e8400-e29b-41d4-a716-446655440002', NULL, 'Transportation', 'Gas, public transport, car maintenance', '#4ECDC4', 'directions_car', 'EXPENSE', TRUE),
('550e8400-e29b-41d4-a716-446655440003', NULL, 'Shopping', 'Clothing, electronics, and general shopping', '#45B7D1', 'shopping_cart', 'EXPENSE', TRUE),
('550e8400-e29b-41d4-a716-446655440004', NULL, 'Entertainment', 'Movies, games, hobbies', '#96CEB4', 'movie', 'EXPENSE', TRUE),
('550e8400-e29b-41d4-a716-446655440005', NULL, 'Bills & Utilities', 'Rent, electricity, water, internet', '#FFEAA7', 'receipt', 'EXPENSE', TRUE),
('550e8400-e29b-41d4-a716-446655440006', NULL, 'Healthcare', 'Medical expenses, insurance, pharmacy', '#DDA0DD', 'local_hospital', 'EXPENSE', TRUE),
-- Income categories
('550e8400-e29b-41d4-a716-446655440007', NULL, 'Salary', 'Regular employment income', '#98D8C8', 'work', 'INCOME', TRUE),
('550e8400-e29b-41d4-a716-446655440008', NULL, 'Freelance', 'Freelance and contract work', '#F7DC6F', 'laptop', 'INCOME', TRUE),
('550e8400-e29b-41d4-a716-446655440009', NULL, 'Investment', 'Dividends, capital gains, interest', '#BB8FCE', 'trending_up', 'INCOME', TRUE);

-- Insert sample tags
INSERT INTO tags (id, user_id, name, color, usage_count) VALUES 
('550e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440000', 'Business', '#007bff', 5),
('550e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440000', 'Personal', '#28a745', 8),
('550e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440000', 'Tax Deductible', '#ffc107', 3),
('550e8400-e29b-41d4-a716-446655440013', '550e8400-e29b-41d4-a716-446655440000', 'Emergency', '#dc3545', 1),
('550e8400-e29b-41d4-a716-446655440014', '550e8400-e29b-41d4-a716-446655440000', 'Vacation', '#17a2b8', 2);

-- Insert sample transactions
INSERT INTO transactions (id, user_id, category_id, amount, description, notes, type, transaction_date, is_recurring, recurring_frequency) VALUES 
-- Income transactions
('550e8400-e29b-41d4-a716-446655440020', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440007', 5000.00, 'Monthly Salary', 'January salary payment', 'INCOME', '2025-01-01', TRUE, 'MONTHLY'),
('550e8400-e29b-41d4-a716-446655440021', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440008', 1200.00, 'Web Development Project', 'Client project completion', 'INCOME', '2025-01-15', FALSE, NULL),

-- Expense transactions
('550e8400-e29b-41d4-a716-446655440022', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440005', 1500.00, 'Monthly Rent', 'Apartment rent payment', 'EXPENSE', '2025-01-01', TRUE, 'MONTHLY'),
('550e8400-e29b-41d4-a716-446655440023', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440001', 350.00, 'Grocery Shopping', 'Weekly groceries at Whole Foods', 'EXPENSE', '2025-01-05', FALSE, NULL),
('550e8400-e29b-41d4-a716-446655440024', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440002', 60.00, 'Gas Fill-up', 'Shell gas station', 'EXPENSE', '2025-01-10', FALSE, NULL),
('550e8400-e29b-41d4-a716-446655440025', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440004', 15.99, 'Netflix Subscription', 'Monthly streaming service', 'EXPENSE', '2025-01-12', TRUE, 'MONTHLY'),
('550e8400-e29b-41d4-a716-446655440026', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440003', 89.99, 'Running Shoes', 'Nike Air Max from Amazon', 'EXPENSE', '2025-01-18', FALSE, NULL),
('550e8400-e29b-41d4-a716-446655440027', '550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440006', 120.00, 'Doctor Visit', 'Annual checkup co-pay', 'EXPENSE', '2025-01-20', FALSE, NULL);

-- Insert transaction-tag relationships
INSERT INTO transaction_tags (transaction_id, tag_id) VALUES 
('550e8400-e29b-41d4-a716-446655440021', '550e8400-e29b-41d4-a716-446655440010'), -- Freelance -> Business
('550e8400-e29b-41d4-a716-446655440021', '550e8400-e29b-41d4-a716-446655440012'), -- Freelance -> Tax Deductible
('550e8400-e29b-41d4-a716-446655440022', '550e8400-e29b-41d4-a716-446655440011'), -- Rent -> Personal
('550e8400-e29b-41d4-a716-446655440023', '550e8400-e29b-41d4-a716-446655440011'), -- Groceries -> Personal
('550e8400-e29b-41d4-a716-446655440024', '550e8400-e29b-41d4-a716-446655440011'), -- Gas -> Personal
('550e8400-e29b-41d4-a716-446655440025', '550e8400-e29b-41d4-a716-446655440011'), -- Netflix -> Personal
('550e8400-e29b-41d4-a716-446655440026', '550e8400-e29b-41d4-a716-446655440011'), -- Shoes -> Personal
('550e8400-e29b-41d4-a716-446655440027', '550e8400-e29b-41d4-a716-446655440011'); -- Doctor -> Personal

-- =====================================================
-- Create Views for Common Queries
-- =====================================================

-- View for transaction summary with category and tag information
CREATE VIEW transaction_summary AS
SELECT 
    t.id,
    t.user_id,
    t.amount,
    t.description,
    t.notes,
    t.type,
    t.transaction_date,
    t.is_recurring,
    t.recurring_frequency,
    t.created_at,
    c.name AS category_name,
    c.color AS category_color,
    c.icon AS category_icon,
    STRING_AGG(tag.name, ', ') AS tags
FROM transactions t
JOIN categories c ON t.category_id = c.id
LEFT JOIN transaction_tags tt ON t.id = tt.transaction_id
LEFT JOIN tags tag ON tt.tag_id = tag.id
GROUP BY t.id, c.name, c.color, c.icon
ORDER BY t.transaction_date DESC;

-- View for monthly spending summary
CREATE VIEW monthly_spending_summary AS
SELECT 
    user_id,
    DATE_TRUNC('month', transaction_date) AS month,
    SUM(CASE WHEN type = 'EXPENSE' THEN amount ELSE 0 END) AS total_expenses,
    SUM(CASE WHEN type = 'INCOME' THEN amount ELSE 0 END) AS total_income,
    SUM(CASE WHEN type = 'INCOME' THEN amount ELSE -amount END) AS net_amount,
    COUNT(*) AS transaction_count
FROM transactions 
GROUP BY user_id, DATE_TRUNC('month', transaction_date)
ORDER BY user_id, month DESC;

-- View for category spending summary
CREATE VIEW category_spending_summary AS
SELECT 
    c.user_id,
    c.id AS category_id,
    c.name AS category_name,
    c.type AS category_type,
    c.color AS category_color,
    c.is_default,
    COUNT(t.id) AS transaction_count,
    COALESCE(SUM(t.amount), 0) AS total_amount,
    COALESCE(AVG(t.amount), 0) AS average_amount
FROM categories c
LEFT JOIN transactions t ON c.id = t.category_id
GROUP BY c.user_id, c.id, c.name, c.type, c.color, c.is_default
ORDER BY total_amount DESC;

-- =====================================================
-- Create Stored Procedures/Functions
-- =====================================================

-- Function to get user's dashboard summary
CREATE OR REPLACE FUNCTION get_dashboard_summary(
    p_user_id UUID,
    p_start_date DATE DEFAULT NULL,
    p_end_date DATE DEFAULT NULL
) RETURNS TABLE (
    total_income DECIMAL(12,2),
    total_expenses DECIMAL(12,2),
    net_amount DECIMAL(12,2),
    transaction_count BIGINT
) AS $$
BEGIN
    -- Set default dates if not provided
    IF p_start_date IS NULL THEN
        p_start_date := DATE_TRUNC('month', CURRENT_DATE);
    END IF;
    
    IF p_end_date IS NULL THEN
        p_end_date := CURRENT_DATE;
    END IF;
    
    RETURN QUERY
    SELECT 
        COALESCE(SUM(CASE WHEN t.type = 'INCOME' THEN t.amount ELSE 0 END), 0) AS total_income,
        COALESCE(SUM(CASE WHEN t.type = 'EXPENSE' THEN t.amount ELSE 0 END), 0) AS total_expenses,
        COALESCE(SUM(CASE WHEN t.type = 'INCOME' THEN t.amount ELSE -t.amount END), 0) AS net_amount,
        COUNT(*)::BIGINT AS transaction_count
    FROM transactions t
    WHERE t.user_id = p_user_id 
    AND t.transaction_date BETWEEN p_start_date AND p_end_date;
END;
$$ LANGUAGE plpgsql;

-- Function to get all categories available to a user (default + user-specific)
CREATE OR REPLACE FUNCTION get_user_categories(p_user_id UUID)
RETURNS TABLE (
    id UUID,
    user_id UUID,
    name VARCHAR(100),
    description TEXT,
    color VARCHAR(7),
    icon VARCHAR(50),
    type TEXT,
    is_default BOOLEAN,
    created_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        c.id,
        c.user_id,
        c.name,
        c.description,
        c.color,
        c.icon,
        c.type,
        c.is_default,
        c.created_at,
        c.updated_at
    FROM categories c
    WHERE c.user_id IS NULL -- Default categories available to all users
       OR c.user_id = p_user_id -- User-specific categories
    ORDER BY c.is_default DESC, c.name ASC; -- Default categories first, then alphabetical
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- Grant Permissions (Adjust as needed for your application user)
-- =====================================================

-- Create application user (uncomment and modify as needed)
-- CREATE USER expense_tracker_app WITH ENCRYPTED PASSWORD 'your_secure_password';
-- GRANT CONNECT ON DATABASE your_database_name TO expense_tracker_app;
-- GRANT USAGE ON SCHEMA public TO expense_tracker_app;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO expense_tracker_app;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO expense_tracker_app;

-- =====================================================
-- Verification Queries
-- =====================================================

-- Verify the setup
SELECT 'Database setup completed successfully!' as status;

-- Show table counts
SELECT 
    'users' as table_name, COUNT(*) as record_count FROM users
UNION ALL
SELECT 
    'user_sessions' as table_name, COUNT(*) as record_count FROM user_sessions
UNION ALL
SELECT 
    'categories' as table_name, COUNT(*) as record_count FROM categories
UNION ALL
SELECT 
    'tags' as table_name, COUNT(*) as record_count FROM tags
UNION ALL
SELECT 
    'transactions' as table_name, COUNT(*) as record_count FROM transactions
UNION ALL
SELECT 
    'transaction_tags' as table_name, COUNT(*) as record_count FROM transaction_tags;

-- Show sample data
SELECT 'Sample User:' as info, username, email, first_name, last_name FROM users LIMIT 1;
SELECT 'Sample Categories:' as info, name, type, color FROM categories LIMIT 5;
SELECT 'Sample Transactions:' as info, description, amount, type, transaction_date FROM transactions LIMIT 5;
