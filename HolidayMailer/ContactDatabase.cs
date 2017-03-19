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
        private string tableName = "contact";

        public ContactDatabase()
        {
            if(!File.Exists("contactdb.sqlite"))
            {
                SQLiteConnection.CreateFile("contactdb.sqlite");
                _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
                _dbConn.Open();
                string createTable = "CREATE TABLE " + tableName + " (id INTEGER PRIMARY KEY, lname VARCHAR(255), fname VARCHAR(255), email VARCHAR(255), didsend VARCHAR(255))";
                SQLiteCommand sqlCmd = new SQLiteCommand(createTable, _dbConn);
                sqlCmd.ExecuteNonQuery();
                _dbConn.Close();
            }
        }

        public void SaveEditContact(int id, ContactModel contact)
        {
            _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
            _dbConn.Open();
            var updateItemCmd = new SQLiteCommand(@"UPDATE " + tableName + " SET lname = @lname, fname = @fname, email = @email, didsend = @didsend WHERE id = @id", _dbConn);
            updateItemCmd.Parameters.AddWithValue("@id", contact.Id);
            updateItemCmd.Parameters.AddWithValue("@lname", contact.LName);
            updateItemCmd.Parameters.AddWithValue("@fname", contact.FName);
            updateItemCmd.Parameters.AddWithValue("@email", contact.Email);
            if(contact.DidSend)
                updateItemCmd.Parameters.AddWithValue("@didsend", 1);
            else
                updateItemCmd.Parameters.AddWithValue("@didsend", 0);

            updateItemCmd.ExecuteNonQuery();

            _dbConn.Close();

        }

        public void CreateContact(ContactModel contact)
        {
            _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
            _dbConn.Open();
            var insertCmd = new SQLiteCommand(@"INSERT INTO " + tableName + " (lname, fname, email, didsend) VALUES (@lname, @fname, @email, @didsend)", _dbConn);
            insertCmd.Parameters.Add(new SQLiteParameter("@lname", contact.LName));
            insertCmd.Parameters.Add(new SQLiteParameter("@fname", contact.FName));
            insertCmd.Parameters.Add(new SQLiteParameter("@email", contact.Email));
            if (contact.DidSend)
                insertCmd.Parameters.Add(new SQLiteParameter("@didsend", 1));
            else
                insertCmd.Parameters.Add(new SQLiteParameter("@didsend", 0));

            insertCmd.ExecuteNonQuery();

            _dbConn.Close();
        }

        public void DeleteContact(int id)
        {
            _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
            _dbConn.Open();
            var deleteCmd = new SQLiteCommand(@"DELETE FROM " + tableName + " WHERE id = @id", _dbConn);
            deleteCmd.Parameters.Add(new SQLiteParameter("@id", id));
            deleteCmd.ExecuteNonQuery();

            _dbConn.Close();
        }
    }
}
