-- Dijital Ödeme Sistemi - PostgreSQL Veritabaný Scripti

-- Veritabaný oluþturma (isteðe baðlý)
-- CREATE DATABASE digitalpayment;

-- Users tablosu
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(100) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'User',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Accounts tablosu
CREATE TABLE IF NOT EXISTS accounts (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    owner_name VARCHAR(100) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'TRY',
    balance DECIMAL(15, 2) DEFAULT 0.00,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Transactions tablosu
CREATE TABLE IF NOT EXISTS transactions (
    id SERIAL PRIMARY KEY,
    account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
    amount DECIMAL(15, 2) NOT NULL,
    type VARCHAR(20) NOT NULL CHECK (type IN ('Deposit', 'Withdraw')),
    status VARCHAR(20) NOT NULL DEFAULT 'Success',
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Ýndeksler
CREATE INDEX IF NOT EXISTS idx_accounts_user_id ON accounts(user_id);
CREATE INDEX IF NOT EXISTS idx_transactions_account_id ON transactions(account_id);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Test kullanýcýlarý ekleme
INSERT INTO users (full_name, email, password, role) VALUES
('Admin User', 'admin@test.com', 'admin123', 'Admin'),
('Ahmet Yýlmaz', 'user@test.com', 'user123', 'User'),
('Ayþe Demir', 'ayse@test.com', 'user123', 'User'),
('Mehmet Kaya', 'mehmet@test.com', 'user123', 'User')
ON CONFLICT (email) DO NOTHING;

-- Test hesaplarý ekleme
INSERT INTO accounts (user_id, owner_name, currency, balance, is_active) VALUES
(1, 'Admin User', 'TRY', 50000.00, TRUE),
(1, 'Admin User', 'USD', 5000.00, TRUE),
(2, 'Ahmet Yýlmaz', 'TRY', 15000.00, TRUE),
(2, 'Ahmet Yýlmaz', 'EUR', 2000.00, TRUE),
(3, 'Ayþe Demir', 'TRY', 8000.00, TRUE),
(3, 'Ayþe Demir', 'USD', 1500.00, TRUE),
(4, 'Mehmet Kaya', 'TRY', 25000.00, TRUE),
(4, 'Mehmet Kaya', 'GBP', 1000.00, TRUE);

-- Test iþlemleri ekleme
INSERT INTO transactions (account_id, amount, type, status, description) VALUES
(1, 10000.00, 'Deposit', 'Success', 'Ýlk yatýrma'),
(1, 5000.00, 'Withdraw', 'Success', 'ATM çekimi'),
(2, 2000.00, 'Deposit', 'Success', 'Maaþ yatýrýmý'),
(3, 3000.00, 'Deposit', 'Success', 'Havale'),
(3, 1000.00, 'Withdraw', 'Success', 'Online alýþveriþ'),
(4, 500.00, 'Deposit', 'Success', 'Para yatýrma'),
(5, 5000.00, 'Deposit', 'Success', 'Ýlk bakiye'),
(5, 2000.00, 'Withdraw', 'Success', 'Fatura ödemesi'),
(6, 1000.00, 'Deposit', 'Success', 'Freelance gelir'),
(7, 15000.00, 'Deposit', 'Success', 'Ýþten gelir'),
(7, 3000.00, 'Withdraw', 'Success', 'Kira ödemesi'),
(8, 800.00, 'Deposit', 'Success', 'Tasarruf');

-- Veritabaný görünümleri (Views)
CREATE OR REPLACE VIEW v_account_summary AS
SELECT 
    a.id,
    a.owner_name,
    a.currency,
    a.balance,
    a.is_active,
    u.full_name as user_name,
    u.email as user_email,
    COUNT(t.id) as transaction_count,
    COALESCE(SUM(CASE WHEN t.type = 'Deposit' THEN t.amount ELSE 0 END), 0) as total_deposits,
    COALESCE(SUM(CASE WHEN t.type = 'Withdraw' THEN t.amount ELSE 0 END), 0) as total_withdrawals
FROM accounts a
LEFT JOIN users u ON a.user_id = u.id
LEFT JOIN transactions t ON a.id = t.account_id
GROUP BY a.id, a.owner_name, a.currency, a.balance, a.is_active, u.full_name, u.email;

-- Faydalý sorgular için fonksiyonlar
CREATE OR REPLACE FUNCTION get_user_total_balance(user_id_param INTEGER)
RETURNS DECIMAL(15, 2) AS $$
DECLARE
    total DECIMAL(15, 2);
BEGIN
    SELECT COALESCE(SUM(balance), 0) INTO total
    FROM accounts
    WHERE user_id = user_id_param AND is_active = TRUE;
    RETURN total;
END;
$$ LANGUAGE plpgsql;

-- Trigger: Ýþlem sonrasý bakiye güncelleme (otomatik)
CREATE OR REPLACE FUNCTION update_account_balance()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.type = 'Deposit' THEN
        UPDATE accounts SET balance = balance + NEW.amount WHERE id = NEW.account_id;
    ELSIF NEW.type = 'Withdraw' THEN
        UPDATE accounts SET balance = balance - NEW.amount WHERE id = NEW.account_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Not: Bu trigger'ý kullanmak isterseniz aþaðýdaki komutu çalýþtýrýn
-- Ancak Controller'da manuel güncelleme yapýldýðý için þimdilik devre dýþý
-- CREATE TRIGGER trg_update_balance
-- AFTER INSERT ON transactions
-- FOR EACH ROW
-- EXECUTE FUNCTION update_account_balance();

-- Tablo bilgilerini göster
SELECT 'Users' as table_name, COUNT(*) as record_count FROM users
UNION ALL
SELECT 'Accounts', COUNT(*) FROM accounts
UNION ALL
SELECT 'Transactions', COUNT(*) FROM transactions;

-- Kullanýcý baþýna hesap sayýsý
SELECT 
    u.full_name,
    u.email,
    u.role,
    COUNT(a.id) as account_count,
    COALESCE(SUM(a.balance), 0) as total_balance
FROM users u
LEFT JOIN accounts a ON u.id = a.user_id
GROUP BY u.id, u.full_name, u.email, u.role
ORDER BY u.role DESC, u.full_name;

-- Para birimi bazýnda toplam bakiye
SELECT 
    currency,
    COUNT(*) as account_count,
    SUM(balance) as total_balance,
    AVG(balance) as average_balance
FROM accounts
WHERE is_active = TRUE
GROUP BY currency
ORDER BY total_balance DESC;

COMMIT;
