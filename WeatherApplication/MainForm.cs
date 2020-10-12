using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using Timer = System.Windows.Forms.Timer;
using MySql.Data.MySqlClient;

namespace WeatherApplicationV2
{
    public partial class MainForm : Form
    {
        Timer timer = new Timer();
        Thread thread = new Thread(new ThreadStart(startScreenSplash));
        Database db = new Database();
        const string APIKey = "9fc08d7cdee03b4d00def9c9e108dbf2";
        bool isCityExisting;
        public static string city { get; set; }
        public static double cTemp { get; set; }
        public static double fTemp { get; set; }
        public static string date { get; set; }
        public static string time { get; set; }
        public string icon { get; set; }
        public string valueButton { get; set; }
        public string condition { get; set; }
        public float feelsLikeC { get; set; }
        public float windSpeed { get; set; }
        public double feelsLikeF { get; set; }
        public List<double> avgCTemperature = new List<double>();
        public List<double> avgFTemperature = new List<double>();
        public List<string> dateList = new List<string>();
        public MainForm()
        {
            timer.Tick += new EventHandler(Timer_Tick);
            thread.Start();
            Thread.Sleep(3000);
            InitializeComponent();
            thread.Abort();
        }
        public static void startScreenSplash()
        {
            Application.Run(new SplashScreen());
        }
        public void getWeather(string cityName)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var json = "";
                    json = wc.DownloadString(string.Format("http://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}&units=metric&cnt=6", cityName, APIKey));
                    var deserializedUrl = JsonConvert.DeserializeObject<WeatherJSON>(json);
                    WeatherJSON result = deserializedUrl;
                    city = result.name;
                    cTemp = result.main.temp;
                    fTemp = Math.Round((result.main.temp * 9 / 5) + 32);
                    date = DateTime.Now.ToString("yyyy/MM/dd");
                    time = string.Format(DateTime.Now.ToString("HH:mm:ss"));
                    icon = result.weather[0].icon;
                    condition = result.weather[0].description;
                    feelsLikeC = result.main.feels_like;
                    feelsLikeF = Math.Round((result.main.feels_like * 9 / 5) + 32);
                    windSpeed = result.wind.speed;
                    dateTime.Text = time;
                    isCityExisting = true;
                    setLabels();
                }
            }
            catch (WebException)
            {
                MessageBox.Show("Enter a correct city");
                isCityExisting = false;
            }

        }
        public void setLabels()
        {
            if (isCityExisting)
            {
                if (valueButton == "F")
                {
                    weatherCF.Text = string.Format("{0} \u00B0" + "F", Math.Round(fTemp));
                    weatherFeelsLike.Text = string.Format("{0} \u00B0" + "F", feelsLikeF);
                    fCToolStripMenuItem.Text = string.Format("{0} \u00B0" + "F", Math.Round(fTemp));
                }
                else
                {
                    weatherCF.Text = string.Format("{0} \u00B0" + "C", Math.Round(cTemp));
                    weatherFeelsLike.Text = string.Format("{0} \u00B0" + "C", feelsLikeC);
                    fCToolStripMenuItem.Text = string.Format("{0} \u00B0" + "C", Math.Round(cTemp));
                }
                weatherCity.Text = string.Format("{0}", city);
                weatherIcon.Load(string.Format("http://openweathermap.org/img/wn/{0}@2x.png", icon));
                weatherDescription.Text = string.Format("{0}", condition);
                weatherWindSpeed.Text = string.Format("{0}" + " km/h", windSpeed);
                dateTime.Text = time;
            }
        }
        public void showGraphData()
        {
            getGraphData();
            List<double> tempList = new List<double>();
            if (valueButton == "C" || String.IsNullOrEmpty(valueButton))
            {
                tempList = avgCTemperature;
                chart1.Series[0].Name = "C";
            }
            else
            {
                tempList = avgFTemperature;
                chart1.Series[0].Name = "F";
            }
            int index = 0;
            for (int i = 0; i < dateList.Count; i++)
            {
                chart1.Series[0].Points.AddXY(dateList[index], tempList[index]);
                index++;
            }
        }
        public void getGraphData()
        {
            String DBConnectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=weatherdata";
            using (MySqlConnection DBConnection = new MySqlConnection(DBConnectionString))
            {
                using (MySqlCommand stmt = new MySqlCommand())
                {
                    stmt.Connection = DBConnection;
                    stmt.CommandType = CommandType.Text;
                    String Query = $"SELECT AVG(weatherhistory.CTemp) AS CTemp, AVG(weatherhistory.FTemp) AS FTemp, weatherhistory.TodayDate FROM weatherhistory WHERE weatherhistory.City = '{city}' AND(weatherhistory.TodayDate between date_sub(Curdate(), interval 5 day) and  curdate()) GROUP BY weatherhistory.TodayDate";
                    stmt.CommandText = Query;
                    try
                    {
                        DBConnection.Open();
                        MySqlDataReader myReader = stmt.ExecuteReader();
                        avgCTemperature.Clear();
                        avgFTemperature.Clear();
                        dateList.Clear();
                        while (myReader.Read())
                        {
                            avgCTemperature.Add(Convert.ToDouble(myReader["CTemp"]));
                            avgFTemperature.Add(Convert.ToDouble(myReader["FTemp"]));
                            dateList.Add(Convert.ToString(myReader["TodayDate"]));
                        }
                    }
                    catch (MySqlException e)
                    {
                        MessageBox.Show("Error Message: " + e.Message);
                    }
                    finally
                    {
                        stmt.Connection.Close();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int number = (int)interval.Value * 1000;
            bool isChecked = radioButton1.Checked;
            if (String.IsNullOrEmpty(cityName.Text))
            {
                MessageBox.Show("Please enter a city");
            }
            else
            {
                city = cityName.Text;
            }
            if (!radioButton1.Checked && !radioButton2.Checked)
            {
                MessageBox.Show("Please select a type of showing the temperature");
                return;
            }
            else
            {
                if (isChecked)
                {
                    valueButton = radioButton1.Text;
                }
                else
                {
                    valueButton = radioButton2.Text;
                }
            }
            if (number == 0)
            {
                number = 60000;
            }

            timer.Interval = number;
            timer.Start();
            getWeather(city);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            getWeather("Emmen");
            db.insertIntoDatabase(null, city, cTemp, fTemp, date, time);
            timer.Interval = 60000;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            getWeather(city);
            db.insertIntoDatabase(null, city, cTemp, fTemp, date, time);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            tabControl1.SelectTab("tabPage1");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.Show();
        }
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getWeather("Emmen");
            tabControl1.SelectTab("tabPage3");
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            chart1.Series[0].Points.Clear();
            if (tabControl1.SelectedTab == tabPage2)
            {
                showGraphData();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                notifyIcon1.Visible = true;
            }
        }
    }
}
