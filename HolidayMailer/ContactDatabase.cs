using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolidayMailer
{
    class ContactDatabase
    {
        private SQLiteConnection _dbConn;

        public ContactDatabase()
        {
            if(!File.Exists("contactdb.sqlite"))
            {
                SQLiteConnection.CreateFile("contactdb.sqlite");
                _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
                _dbConn.Open();
                string createTable = "CREATE TABLE contact (id INTEGER PRIMARY KEY, lname VARCHAR(255), fname VARCHAR(255), email VARCHAR(255), didsend VARCHAR(255))";
                SQLiteCommand sqlCmd = new SQLiteCommand(createTable, _dbConn);
                sqlCmd.ExecuteNonQuery();
                _dbConn.Close();
            }
        }
    }
}
