
using Dapper;
using MySql.Data.MySqlClient;
using SmartWeb.Helpers;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SmartWeb.DB
{
    public static class DapperContext
    {
        static readonly string ConnectionString = AppSettingHelper.GetConnectionString("dbconn");

        /// <summary>
        /// 查出一条记录的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static T QueryFirstOrDefault<T>(string sql, object param = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.QueryFirstOrDefault<T>(sql, param);
        }

        public static Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.QueryFirstOrDefaultAsync<T>(sql, param);
        }
        /// <summary>
        /// 查出多条记录的实体泛型集合
        /// </summary>
        /// <typeparam name="T">泛型T</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static int Execute(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.Execute(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }

        public static T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 同时查询多张表数据（高级查询）
        /// "select *from K_City;select *from K_Area";
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static SqlMapper.GridReader QueryMultiple(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.QueryMultiple(sql, param, transaction, commandTimeout, commandType);
        }
        public static Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using IDbConnection Db = new MySqlConnection(ConnectionString);
            return Db.QueryMultipleAsync(sql, param, transaction, commandTimeout, commandType);
        }


    }
}
