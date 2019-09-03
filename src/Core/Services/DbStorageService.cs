using Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services {
    public interface IDbStorageService {
        Task<int> ExecuteRaw(string raw);
        Task<string> FromQuery(string raw);
    }
    public class DbStorageService : IDbStorageService {
        readonly AppDbContext db;

        public DbStorageService(AppDbContext db) {
            this.db = db;
        }

        public Task<int> ExecuteRaw(string raw) {
            return db.Database.ExecuteSqlCommandAsync(raw);
        }
        
        public async Task<string> FromQuery(string raw) {
            var builder = new StringBuilder();
            using (var command = db.Database.GetDbConnection().CreateCommand()) {
                command.CommandText = raw;
                command.CommandType = System.Data.CommandType.Text;

                db.Database.OpenConnection();

                using (var result = await command.ExecuteReaderAsync()) {
                    while (result.Read()) {
                        for (var i = 0; i < result.FieldCount; ++i) {
                            builder.Append(result[i]).Append(' ');
                        }
                        builder.AppendLine();
                    }
                }
            }
            return builder.ToString();
        }
    }
}