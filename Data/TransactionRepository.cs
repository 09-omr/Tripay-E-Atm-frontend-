using Npgsql;
using digitalpayment3.Models;

namespace digitalpayment3.Data
{
    public class TransactionRepository
    {
        private readonly string _connectionString;

        public TransactionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSQL") ?? "";
        }

        // tx_type ve status normalizasyon helper metodu
        private string NormalizeType(string txType)
        {
            // "DEPOSIT" veya DEPOSIT -> Deposit
            // "WITHDRAW" veya WITHDRAW -> Withdraw
            var cleaned = txType.Trim('"').Trim();
            return cleaned switch
            {
                "DEPOSIT" => "Deposit",
                "WITHDRAW" => "Withdraw",
                _ => cleaned
            };
        }

        private string NormalizeStatus(string status)
        {
            // "COMPLETED" -> Success
            // "PENDING" -> Pending
            // "FAILED" -> Failed
            var cleaned = status.Trim('"').Trim();
            return cleaned switch
            {
                "COMPLETED" => "Success",
                "PENDING" => "Pending",
                "FAILED" => "Failed",
                _ => cleaned
            };
        }

        public List<TransactionViewModel> GetByAccountId(int accountId)
        {
            var transactions = new List<TransactionViewModel>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT id, account_id, amount, tx_type, status, created_at, description 
                FROM transactions 
                WHERE account_id = @accountId 
                ORDER BY created_at DESC",
                conn);
            cmd.Parameters.AddWithValue("accountId", (long)accountId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                transactions.Add(new TransactionViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)),
                    AccountId = Convert.ToInt32(reader.GetInt64(1)),
                    Amount = reader.GetDecimal(2),
                    Type = NormalizeType(reader.GetString(3)), // ? Normalize
                    Status = NormalizeStatus(reader.GetString(4)), // ? Normalize
                    CreatedAt = reader.GetDateTime(5),
                    Description = reader.IsDBNull(6) ? "" : reader.GetString(6)
                });
            }
            return transactions;
        }

        public List<TransactionViewModel> GetAll()
        {
            var transactions = new List<TransactionViewModel>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT id, account_id, amount, tx_type, status, created_at, description 
                FROM transactions 
                ORDER BY created_at DESC 
                LIMIT 100",
                conn);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                transactions.Add(new TransactionViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)),
                    AccountId = Convert.ToInt32(reader.GetInt64(1)),
                    Amount = reader.GetDecimal(2),
                    Type = NormalizeType(reader.GetString(3)), // ? Normalize
                    Status = NormalizeStatus(reader.GetString(4)), // ? Normalize
                    CreatedAt = reader.GetDateTime(5),
                    Description = reader.IsDBNull(6) ? "" : reader.GetString(6)
                });
            }
            return transactions;
        }

        public void Insert(TransactionViewModel transaction)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            // Insert ederken de tersi: Deposit -> DEPOSIT
            var txType = transaction.Type.ToUpper();
            var status = transaction.Status switch
            {
                "Success" => "COMPLETED",
                "Pending" => "PENDING",
                "Failed" => "FAILED",
                _ => transaction.Status.ToUpper()
            };

            var cmd = new NpgsqlCommand(@"
                INSERT INTO transactions (account_id, amount, tx_type, status, created_at, description) 
                VALUES (@accountId, @amount, @txType, @status, @createdAt, @description)",
                conn);
            
            cmd.Parameters.AddWithValue("accountId", (long)transaction.AccountId);
            cmd.Parameters.AddWithValue("amount", transaction.Amount);
            cmd.Parameters.AddWithValue("txType", txType); // DEPOSIT veya WITHDRAW
            cmd.Parameters.AddWithValue("status", status); // COMPLETED
            cmd.Parameters.AddWithValue("createdAt", DateTime.Now);
            cmd.Parameters.AddWithValue("description", transaction.Description ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public int GetTotalTransactionCount()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM transactions", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<TransactionViewModel> GetRecentTransactions(int limit = 5)
        {
            var transactions = new List<TransactionViewModel>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT id, account_id, amount, tx_type, status, created_at, description 
                FROM transactions 
                ORDER BY created_at DESC 
                LIMIT @limit",
                conn);
            cmd.Parameters.AddWithValue("limit", limit);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                transactions.Add(new TransactionViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)),
                    AccountId = Convert.ToInt32(reader.GetInt64(1)),
                    Amount = reader.GetDecimal(2),
                    Type = NormalizeType(reader.GetString(3)), // ? Normalize
                    Status = NormalizeStatus(reader.GetString(4)), // ? Normalize
                    CreatedAt = reader.GetDateTime(5),
                    Description = reader.IsDBNull(6) ? "" : reader.GetString(6)
                });
            }
            return transactions;
        }

        public List<TransactionViewModel> GetRecentByUserId(int userId, int limit = 5)
        {
            var transactions = new List<TransactionViewModel>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT t.id, t.account_id, t.amount, t.tx_type, t.status, t.created_at, t.description 
                FROM transactions t
                INNER JOIN accounts a ON t.account_id = a.id
                WHERE a.user_id = @userId 
                ORDER BY t.created_at DESC 
                LIMIT @limit",
                conn);
            cmd.Parameters.AddWithValue("userId", (long)userId);
            cmd.Parameters.AddWithValue("limit", limit);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                transactions.Add(new TransactionViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)),
                    AccountId = Convert.ToInt32(reader.GetInt64(1)),
                    Amount = reader.GetDecimal(2),
                    Type = NormalizeType(reader.GetString(3)), // ? Normalize
                    Status = NormalizeStatus(reader.GetString(4)), // ? Normalize
                    CreatedAt = reader.GetDateTime(5),
                    Description = reader.IsDBNull(6) ? "" : reader.GetString(6)
                });
            }
            return transactions;
        }
    }
}
