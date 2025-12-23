using Npgsql;
using digitalpayment3.Models;

namespace digitalpayment3.Data
{
    public class AccountRepository
    {
        private readonly string _connectionString;

        public AccountRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSQL") ?? "";
        }

        public List<AccountViewModel> GetAll()
        {
            var accounts = new List<AccountViewModel>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            // Sizin tablo yapýnýza göre: id, user_id, currency, balance, is_active, created_at
            var cmd = new NpgsqlCommand(@"
                SELECT 
                    a.id, 
                    COALESCE(u.full_name, 'Unknown') as owner_name,
                    a.currency, 
                    a.balance, 
                    a.is_active, 
                    a.created_at 
                FROM accounts a
                LEFT JOIN users u ON a.user_id = u.id
                ORDER BY a.created_at DESC", conn);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                accounts.Add(new AccountViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)), // BIGSERIAL -> int64
                    OwnerName = reader.GetString(1),
                    Currency = reader.GetString(2),
                    Balance = reader.GetDecimal(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }
            return accounts;
        }

        public List<AccountViewModel> GetByUserId(int userId)
        {
            var accounts = new List<AccountViewModel>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT 
                    a.id, 
                    COALESCE(u.full_name, 'Unknown') as owner_name,
                    a.currency, 
                    a.balance, 
                    a.is_active, 
                    a.created_at 
                FROM accounts a
                LEFT JOIN users u ON a.user_id = u.id
                WHERE a.user_id = @userId 
                ORDER BY a.created_at DESC", conn);
            
            cmd.Parameters.AddWithValue("userId", (long)userId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                accounts.Add(new AccountViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)),
                    OwnerName = reader.GetString(1),
                    Currency = reader.GetString(2),
                    Balance = reader.GetDecimal(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }
            return accounts;
        }

        public AccountViewModel? GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT 
                    a.id, 
                    COALESCE(u.full_name, 'Unknown') as owner_name,
                    a.currency, 
                    a.balance, 
                    a.is_active, 
                    a.created_at 
                FROM accounts a
                LEFT JOIN users u ON a.user_id = u.id
                WHERE a.id = @id", conn);
            
            cmd.Parameters.AddWithValue("id", (long)id);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new AccountViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)),
                    OwnerName = reader.GetString(1),
                    Currency = reader.GetString(2),
                    Balance = reader.GetDecimal(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5)
                };
            }
            return null;
        }

        public void Insert(AccountViewModel account, int userId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO accounts (user_id, currency, balance, is_active, created_at) 
                VALUES (@userId, @currency, @balance, @isActive, @createdAt)",
                conn);
            
            cmd.Parameters.AddWithValue("userId", (long)userId);
            cmd.Parameters.AddWithValue("currency", account.Currency);
            cmd.Parameters.AddWithValue("balance", account.Balance);
            cmd.Parameters.AddWithValue("isActive", account.IsActive);
            cmd.Parameters.AddWithValue("createdAt", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public void Update(AccountViewModel account)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                UPDATE accounts 
                SET currency = @currency, 
                    balance = @balance, 
                    is_active = @isActive 
                WHERE id = @id",
                conn);
            
            cmd.Parameters.AddWithValue("id", (long)account.Id);
            cmd.Parameters.AddWithValue("currency", account.Currency);
            cmd.Parameters.AddWithValue("balance", account.Balance);
            cmd.Parameters.AddWithValue("isActive", account.IsActive);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("DELETE FROM accounts WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", (long)id);
            cmd.ExecuteNonQuery();
        }

        public int GetTotalAccountCount()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM accounts", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
