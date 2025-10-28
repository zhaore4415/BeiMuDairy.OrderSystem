using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BeiMuDairy.OrderSystem
{
    public class DBHelper
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["BeiMuDairy.OrderSystem.Properties.Settings.OrderSystemConnectionString"].ConnectionString;

        /// <summary>
        /// 执行查询并返回DataTable
        /// </summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        adapter.SelectCommand.Parameters.AddRange(parameters);
                    }

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// 执行非查询SQL语句（插入、更新、删除）
        /// </summary>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 执行查询并返回第一行第一列的值
        /// </summary>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 执行带事务的非查询SQL语句
        /// </summary>
        public static int ExecuteNonQueryWithTransaction(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                
                try
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Transaction = transaction;
                        
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        int result = command.ExecuteNonQuery();
                        transaction.Commit();
                        return result;
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 检查数据库连接是否正常
        /// </summary>
        public static bool CheckConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}