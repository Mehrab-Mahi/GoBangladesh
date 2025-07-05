using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using GoBangladesh.Infra.Data.Context;
using GoBangladesh.Infra.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GoBangladesh.Infra.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        private readonly GoBangladeshDbContext _dbContext;
        private IHttpContextAccessor _httpContextAccessor;
        private string LoggedInUserName = string.Empty;
        public Repository(GoBangladeshDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            ParseLoggedInUser();
        }

        private void ParseLoggedInUser()
        {
            var bytes = new byte[1024];
            _httpContextAccessor.HttpContext.Session.TryGetValue("userId", out bytes);
            if (bytes is not null)
            {
                LoggedInUserName = System.Text.Encoding.UTF8.GetString(bytes).Trim('"');
            }
        }

        #region Public Generic Methods

        public void Insert(T model)
        {
            if (model != null)
            {
                var createdTime = DateTime.UtcNow;
                model.CreateTime = createdTime;
                model.LastModifiedTime = createdTime;
            }
            model.CreatedBy = LoggedInUserName;
            model.LastModifiedBy = LoggedInUserName;

            _dbContext.Entry(model).State = EntityState.Added;
        }

        public void InsertWithUserData(T model)
        {
            if (model != null)
            {
                var createdTime = DateTime.Now;
                model.CreateTime = createdTime;
                model.LastModifiedTime = createdTime;
            }
            _dbContext.Entry(model).State = EntityState.Added;
        }

        public virtual IQueryable<T> GetConditionalList(Expression<Func<T, bool>> expression)
        {
            return _dbContext.Set<T>().Where(expression);
        }

        public virtual T GetConditional(Expression<Func<T, bool>> expression)
        {
            return _dbContext.Set<T>().Where(expression).FirstOrDefault();
        }

        public virtual IQueryable<T> GetAll()
        {
            return _dbContext.Set<T>();
        }

        public T Find(string id)
        {
            return GetAll().FirstOrDefault(_ => _.Id == id);
        }

        public void Update(T entity)
        {
            if (entity != null)
                entity.LastModifiedTime = DateTime.UtcNow;
            entity.LastModifiedBy = LoggedInUserName;
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateUnion(T entity)
        {
            if (entity != null)
                entity.LastModifiedTime = DateTime.Now;
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void AddOrUpdate(T entity)
        {
            var exists = Any(_ => _.Id == entity.Id);
            if (!exists) Insert(entity);
            else Update(entity);
        }

        public void RemoveAll(List<T> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public virtual void Delete(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Deleted;
        }

        public void Delete(List<T> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public virtual bool Any(Expression<Func<T, bool>> expression)
        {
            return GetAll().Any(expression);
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        public void Delete(string id)
        {
            Delete(Find(id));
        }

        public void Insert(List<T> models)
        {
            foreach (var model in models)
            {
                Insert(model);
            }
        }

        public void InsertList(List<T> models)
        {
            foreach (var model in models)
            {
                AddOrUpdate(model);
            }
        }

        public void ExecuteQuery(string query)
        {
            _dbContext.Database.ExecuteSqlRaw(query);
        }

        #endregion Public Generic Methods
    }
}