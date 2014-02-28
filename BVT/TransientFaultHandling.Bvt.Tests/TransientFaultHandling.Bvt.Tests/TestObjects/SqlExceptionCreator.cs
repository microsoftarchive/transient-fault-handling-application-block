using System;
using System.Data.SqlClient;
using System.Reflection;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.TestObjects
{
    public class SqlExceptionCreator
    {
        public static SqlException CreateSqlException(string errorMessage, int errorNumber)
        {
            SqlErrorCollection collection = Construct<SqlErrorCollection>();
            SqlError error = GenerateFakeSqlError(errorNumber, errorMessage);

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(collection, new object[] { error });

            var createException = typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(SqlErrorCollection), typeof(string) }, null);

            var e = createException.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;

            return e;
        }

        public static SqlError GenerateFakeSqlError(int errorNumber, string errorMessage)
        {
            return (SqlError)Activator.CreateInstance(typeof(SqlError), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { errorNumber, default(byte), default(byte), string.Empty, errorMessage, string.Empty, 0 }, null);
        }

        private static T Construct<T>(params object[] p)
        {
            return (T)typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0].Invoke(p);
        }
    }
}
