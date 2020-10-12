using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;

namespace WeatherApplicationV2
{
    class Database
    {
        public void insertIntoDatabase(int? id, String city, Double cTemp, Double fTemp, String date, String time)
        {
            String DBConnectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=weatherdata";
            using (MySqlConnection DBConnection = new MySqlConnection(DBConnectionString))
            {
                using (MySqlCommand stmt = new MySqlCommand())
                {
                    stmt.Connection = DBConnection;
                    stmt.CommandType = CommandType.Text;
                    stmt.CommandText = "INSERT into WeatherHistory (id,City,CTemp,FTemp,TodayDate,RefreshedTime) VALUES (@id,@City,@CTemp,@FTemp,@Date,@Time)";
                    stmt.Parameters.AddWithValue("@id", id);
                    stmt.Parameters.AddWithValue("@City", city);
                    stmt.Parameters.AddWithValue("@CTemp", cTemp);
                    stmt.Parameters.AddWithValue("@FTemp", fTemp);
                    stmt.Parameters.AddWithValue("@Date", date);
                    stmt.Parameters.AddWithValue("@Time", time);
                    try
                    {
                        DBConnection.Open();
                        MySqlDataReader myReader = stmt.ExecuteReader();
                    }
                    catch (MySqlException e)
                    {
                        MessageBox.Show("Error: " + e.Message);
                    }
                    finally
                    {
                        stmt.Connection.Close();
                    }
                }
            }
        }
    }
}
