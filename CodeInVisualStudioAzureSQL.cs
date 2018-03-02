using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft;

namespace WebApplication8
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string message = "{\"SensorNodeId\":12346,\"Temperature\":23.51,\"Humidity\":38.81,\"Acceleration\":{\"X\":-1,\"Y\":0,\"Z\":1},\"Ambient_light\":5563,\"Pressure\":900,\"Switch\":{\"User1\":\"NodeP\",\"Hall\":1},\"Vibration\":{\"Frequency\":228,\"Power\":122},\"Node_status\":{\"Battery_level\":72}}";

            TelemetryDataPoint messageDataPoint = Newtonsoft.Json.JsonConvert.DeserializeObject<TelemetryDataPoint>(message);
            TelemetryDataPoint newPoint = new TelemetryDataPoint()
            {
                SensorNodeId = 2,
                Temperature = 25,
                Humidity = 78,
                Ambient_light = 344,
                Pressure = 455
            };
            newPoint.Acceleration = new TelemetryDataPoint.Accel()
            {
                X = 10,
                Y = 20,
                Z = 30
            };
            newPoint.Switch = new TelemetryDataPoint.Switchdata()
            {
                User1 = "Hewho",
                Hall = true
            };
            newPoint.Vibration = new TelemetryDataPoint.Vibr()
            {
                Frequency = 2300,
                Power = 3455
            };
            newPoint.Node_status = new TelemetryDataPoint.Node_s()
            {
                Battery_level = 67
            };
            System.Diagnostics.Debug.WriteLine(newPoint.Pressure + " testi");
            System.Diagnostics.Debug.WriteLine(messageDataPoint.Pressure + " testi2");

            /* Read from SQL database */
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "tcp:slanetestserver.database.windows.net,1433";
                builder.UserID = "hhuotari";
                builder.Password = "slade765D";
                builder.InitialCatalog = "slaneTestdb";
                builder.PersistSecurityInfo = false;
                builder.MultipleActiveResultSets = false;
                builder.Encrypt = true;
                builder.TrustServerCertificate = false;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    System.Diagnostics.Debug.WriteLine("\nQuery data example:");

                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM SensorDataValues");
                    String sql = sb.ToString();
                    System.Diagnostics.Debug.WriteLine("=========================================\n");

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            var nrOfFields = reader.FieldCount;
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine("New record read from db:");
                                Object[] dataFields = new Object[nrOfFields];
                                reader.GetSqlValues(dataFields);
                                System.Diagnostics.Debug.WriteLine("Id: " + reader.GetName(0) + " " + dataFields[0]);
                                System.Diagnostics.Debug.WriteLine("Id: " + reader.GetName(2) + " " + dataFields[2]);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException errorReceived)
            {
                Console.WriteLine(errorReceived.ToString());
            }

            /* Write into SQL database */

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "tcp:slanetestserver.database.windows.net,1433";
                builder.UserID = "hhuotari";
                builder.Password = "slade765D";
                builder.InitialCatalog = "slaneTestdb";
                builder.PersistSecurityInfo = false;
                builder.MultipleActiveResultSets = false;
                builder.Encrypt = true;
                builder.TrustServerCertificate = false;

                int SensorId = messageDataPoint.SensorNodeId;
                double Temperature = messageDataPoint.Temperature;
                double Humidity = messageDataPoint.Humidity;
                double AccelarationX = messageDataPoint.Acceleration.X;
                double AccelarationY = messageDataPoint.Acceleration.Y;
                double AccelarationZ = messageDataPoint.Acceleration.Z;
                int AmbientLight = messageDataPoint.Ambient_light;
                int Pressure = messageDataPoint.Pressure;
                string SwitchDataUser = messageDataPoint.Switch.User1;
                bool Hall = messageDataPoint.Switch.Hall;
                int VibrationFrequency = messageDataPoint.Vibration.Frequency;
                int VibrationPower = messageDataPoint.Vibration.Power;
                int BatteryLevel = messageDataPoint.Node_status.Battery_level;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    System.Diagnostics.Debug.WriteLine("\nWrite data example:");
                    
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO SensorDataStream (
                        SensorId,Temperature, Humidity, AccelerationX, AccelerationY, AccelerationZ,
	                    AmbientLight, Pressure, SwitchDataUser, Hall, VibrationFrequency, VibrationPower, BatteryLevel)
                        VALUES (@SensorId,@Temperature, @Humidity, @AccelerationX, @AccelerationY, @AccelerationZ,
                            @AmbientLight, @Pressure, @SwitchDataUser, @Hall, @VibrationFrequency, @VibrationPower, @BatteryLevel)", connection);

                    cmd.Parameters.AddWithValue("@SensorId", SensorId);
                    cmd.Parameters.AddWithValue("@Temperature", Temperature);
                    cmd.Parameters.AddWithValue("@Humidity", Humidity);
                    cmd.Parameters.AddWithValue("@AccelerationX", AccelarationX);
                    cmd.Parameters.AddWithValue("@AccelerationY", AccelarationY);
                    cmd.Parameters.AddWithValue("@AccelerationZ", AccelarationZ);
                    cmd.Parameters.AddWithValue("@AmbientLight", AmbientLight);
                    cmd.Parameters.AddWithValue("@Pressure", Pressure);
                    cmd.Parameters.AddWithValue("@SwitchDataUser", SwitchDataUser);
                    cmd.Parameters.AddWithValue("@Hall", Hall);
                    cmd.Parameters.AddWithValue("@VibrationFrequency", VibrationFrequency);
                    cmd.Parameters.AddWithValue("@VibrationPower", VibrationPower);
                    cmd.Parameters.AddWithValue("@BatteryLevel", BatteryLevel);
                    
                    cmd.Connection = connection;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SqlException errorReceived)
            {
                Console.WriteLine(errorReceived.ToString());
            } 

            Console.ReadLine();            
        }

        public class TelemetryDataPoint
        {
            public int SensorNodeId { get; set; }
            public double Temperature { get; set; }
            public double Humidity { get; set; }
            public class Accel
            {
                public double X { get; set; }
                public double Y { get; set; }
                public double Z { get; set; }
            }
            public Accel Acceleration { get; set; }
            public int Ambient_light { get; set; }
            public int Pressure { get; set; }
            public class Switchdata
            {
                public string User1 { get; set; }
                public bool Hall { get; set; }
            }
            public Switchdata Switch { get; set; }
            public class Vibr
            {
                public int Frequency { get; set; }
                public int Power { get; set; }
            }
            public Vibr Vibration { get; set; }
            public class Node_s
            {
                public int Battery_level { get; set; }
            }
            public Node_s Node_status { get; set; }
        }
    }
}
