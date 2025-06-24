using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Domain.Interfaces
{
    public interface IBaseRepository
    {
        public List<T> Query<T>(string query);
        public T QuerySingleOrDefault<T>(string query);
        public T FirstOrDefault<T>(string query);
        public void Insert<T>(T t);
        public Task<T> FirstOrDefaultAsync<T>(string query);
        Task<T> FirstOrDefaultSp<T>(string sp, dynamic param);
        void Execute(string sp, dynamic param);
        public void ExecuteQuery(string query);
        void Update<T>(T model);
        List<T> GetAll<T>();
        T Get<T>(string id);
    }
}
