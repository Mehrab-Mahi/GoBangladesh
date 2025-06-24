using GoBangladesh.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Domain.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        void Insert(T model);
        void InsertWithUserData(T model);
        void Insert(List<T> models);
        void InsertList(List<T> models);
        void Update(T entity);
        void AddOrUpdate(T entity);
        IQueryable<T> GetConditionalList(Expression<Func<T, bool>> expression);
        T GetConditional(Expression<Func<T, bool>> expression);
        IQueryable<T> GetAll();
        T Find(string id);
        void Delete(T entity);
        void Delete(List<T> entities);
        void UpdateUnion(T entity);
        void RemoveAll(List<T> entities);
        bool Any(Expression<Func<T, bool>> expression);
        void SaveChanges();
        void Delete(string id);
        void ExecuteQuery(string query);

    }
}
