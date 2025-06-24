using Dapper;
using GoBangladesh.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Infra.Data.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        private readonly SqlConnection _conn;

        public BaseRepository(IConnectionStringProvider _conProvider)
        {
            _conn = new SqlConnection(_conProvider.GetConnectionString());

        }

        public List<T> Query<T>(string query)
        {
            try
            {
                return _conn.Query<T>(query).ToList();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public T QuerySingleOrDefault<T>(string query)
        {
            try
            {
                return _conn.QuerySingleOrDefault<T>(query);
            }
            catch (Exception ex)
            {
                return default;
            }
        }


        public T FirstOrDefault<T>(string query)
        {
            try
            {
                return _conn.QueryFirstOrDefault<T>(query);
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public void Update<T>(T t)
        {

        //    var properties = t.GetType().GetProperties().Select(x => x.Name).ToList();

        //    var nameValue = new Dictionary<string, String>();

        //    var subQuery = new List<string>();


        //    foreach (var property in properties)
        //    {
        //        var val = t.GetType().GetProperty(property)?.GetValue(t, null)?.ToString();
        //        if (!string.IsNullOrEmpty(val) && val != "1/1/0001 12:00:00 am")
        //        {
        //            nameValue.Add(property, val);
        //            subQuery.Add($" {property} = '{val}' ");

        //        }
        //    }

        //    var finalSubQuery = string.Join(",", subQuery);

        //    var primaryKeyColumn = t.GetType().GetProperties().First(prop => Attribute.IsDefined(prop, typeof(PrimaryKey))).Name;
        //    var primaryKeyValue = nameValue[primaryKeyColumn];
        //    var query = $"update  {GetValidTableName<T>()} set  {finalSubQuery} where {primaryKeyColumn} = '{primaryKeyValue}' ";
        //    try
        //    {
        //        _conn.Execute(query);
        //    }
        //    catch (Exception e)
        //    {

        //    }
        }

        public List<T> GetAll<T>()
        {
            var tableName = typeof(T).Name;
            try
            {
                return _conn.Query<T>($"select * from {tableName}").ToList();
            }
            catch (Exception e)
            {
                return new List<T>();
            }
        }

        public T Get<T>(string id)
        {
            var tableName = typeof(T).Name;
            try
            {
                return _conn.Query<T>($"select * from {tableName} where id = '{id}'").First();
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public void Insert<T>(T t)
        {

            var propertyInfos = t.GetType().GetProperties().Select(x => x.Name).ToList();

            var values = propertyInfos.Select(propertyInfo => typeof(T).GetProperty(propertyInfo)?.GetValue(t, null)).Select(value => value?.ToString()).ToList();

            var query = $"insert into {GetValidTableName<T>()} ({string.Join(",", propertyInfos)}) values ('{string.Join("','", values)}')";
            try
            {
                _conn.Execute(query);
            }
            catch (Exception e)
            {

            }

        }

        private string GetValidTableName<T>()
        {
            return typeof(T).Name == "User" ? "\"User\"" : typeof(T).Name;
        }

        public Task<T> FirstOrDefaultSp<T>(string sp, dynamic param)
        {
            try
            {
                return _conn.QueryFirstOrDefaultAsync<T>(sp, new { param }, commandType: CommandType.StoredProcedure);

            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public void Execute(string sp, dynamic param)
        {
            try
            {
                _conn.QueryFirstOrDefaultAsync(sp, new { param }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {


            }
        }
        public void ExecuteQuery(string query)
        {
            try
            {
                _conn.Execute(query);
            }
            catch (Exception)
            {


            }
        }

        public Task<T> FirstOrDefaultAsync<T>(string query)
        {
            try
            {
                return _conn.QueryFirstOrDefaultAsync<T>(query);

            }
            catch (Exception ex)
            {
                return default;
            }
        }
    }
}
