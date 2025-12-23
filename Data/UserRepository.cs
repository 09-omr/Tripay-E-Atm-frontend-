using Npgsql;
using digitalpayment3.Models;

namespace digitalpayment3.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSQL") ?? "";
        }

        public List<UserViewModel> GetAll()
        {
            var users = new List<UserViewModel>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            // users + user_roles + roles tablolarýný JOIN et
            var cmd = new NpgsqlCommand(@"
                SELECT 
                    u.id, 
                    u.full_name, 
                    u.email, 
                    COALESCE(r.name, 'user') as role,
                    u.is_active,
                    u.created_at
                FROM users u
                LEFT JOIN user_roles ur ON u.id = ur.user_id
                LEFT JOIN roles r ON ur.role_id = r.id
                ORDER BY u.created_at DESC", conn);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new UserViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)), // BIGSERIAL
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = char.ToUpper(reader.GetString(3)[0]) + reader.GetString(3).Substring(1).ToLower(), // admin->Admin
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }
            return users;
        }

        public UserViewModel? GetByEmailAndPassword(string email, string password)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT 
                    u.id, 
                    u.full_name, 
                    u.email, 
                    COALESCE(r.name, 'user') as role,
                    u.is_active,
                    u.created_at
                FROM users u
                LEFT JOIN user_roles ur ON u.id = ur.user_id
                LEFT JOIN roles r ON ur.role_id = r.id
                WHERE u.email = @email AND u.password_hash = @password", conn);
            
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("password", password);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserViewModel
                {
                    Id = Convert.ToInt32(reader.GetInt64(0)),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = char.ToUpper(reader.GetString(3)[0]) + reader.GetString(3).Substring(1).ToLower(),
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5)
                };
            }
            return null;
        }

        public int GetTotalUserCount()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
