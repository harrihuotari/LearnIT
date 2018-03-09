#r "Newtonsoft.Json"
#r "System.Data"

using System;
using System.Data.SqlClient;
using System.Text;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

public static int SensorId;
public static double Temperature;
public static double Humidity;
public static double AccelarationX;
public static double AccelarationY;
public static double AccelarationZ;
public static int AmbientLight;
public static int Pressure;
public static string SwitchDataUser;
public static bool Hall;
public static int VibrationFrequency;
public static int VibrationPower;
public static int BatteryLevel;
public static bool sensorDataReceived = false;

public static void Run(string myIoTHubMessage, TraceWriter log)
{

    log.Info($"C# IoT Hub trigger function processed a message: {myIoTHubMessage}");

    try
    {
        /* read in the sensor data from json data */
        TelemetryDataPoint newDataPoint =  Newtonsoft.Json.JsonConvert.DeserializeObject<TelemetryDataPoint>(myIoTHubMessage);

        /* get sensor values ready for storing into the SQL */
        SensorId = newDataPoint.SensorNodeId;
        Temperature = newDataPoint.Temperature;
        Humidity = newDataPoint.Humidity;
        AccelarationX = newDataPoint.Acceleration.X;
        AccelarationY = newDataPoint.Acceleration.Y;
        AccelarationZ = newDataPoint.Acceleration.Z;
        AmbientLight = newDataPoint.Ambient_light;
        Pressure = newDataPoint.Pressure;
        SwitchDataUser = newDataPoint.Switch.User1;
        Hall = newDataPoint.Switch.Hall;
        VibrationFrequency = newDataPoint.Vibration.Frequency;
        VibrationPower = newDataPoint.Vibration.Power;
        BatteryLevel = newDataPoint.Node_status.Battery_level;
        sensorDataReceived = true;
        log.Info("Sensor data available");
    }
    catch (Exception noSensorData)
    {
        log.Info("No sensor data available");
    }

    /* Read from SQL database */
    try
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.DataSource = "tcp:slanesqlserver.database.windows.net,1433";
        builder.UserID = "xxxxxxx";
        builder.Password = "xxxxx";
        builder.InitialCatalog = "slanesqldb";
        builder.PersistSecurityInfo = false;
        builder.MultipleActiveResultSets = false;
        builder.Encrypt = true;
        builder.TrustServerCertificate = false;

        using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
        {
            log.Info("\nQuery sensor data");

            connection.Open();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM SensorDataStream");
            String sql = sb.ToString();
            // log.Info("=========================================\n");

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var nrOfFields = reader.FieldCount;
                    while (reader.Read())
                    {
                        log.Info("New record read from db:");
                        Object[] dataFields = new Object[nrOfFields];
                        reader.GetSqlValues(dataFields);
                        log.Info("Id: " + reader.GetName(0) + " " + dataFields[0]);
                        log.Info("Id: " + reader.GetName(2) + " " + dataFields[2]);
                    }
                }
            }
            connection.Close();
        }
    }
    catch (SqlException errorReceived)
    {
        log.Info("Reading from database failed");
    }

    /* Write into SQL database */
    if (sensorDataReceived == true)
    {
        sensorDataReceived = false;
        try
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "tcp:slanesqlserver.database.windows.net,1433";
            builder.UserID = "xxxxx";
            builder.Password = "xxxxxx";
            builder.InitialCatalog = "slanesqldb";
            builder.PersistSecurityInfo = false;
            builder.MultipleActiveResultSets = false;
            builder.Encrypt = true;
            builder.TrustServerCertificate = false;

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                log.Info("\nWrite data to Azure SQL database");
                
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
            log.Info("Writing to SQL not succeeded");
        }
    }
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
