#r "Newtonsoft.Json"
#r "System.Data"

using System;
using System.Data.SqlClient;
using System.Text;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Configuration;

public static bool sensorDataReceived = false;

public static void Run(string myIoTHubMessage, TraceWriter log)
{

    TelemetryDataPoint newDataPoint = new TelemetryDataPoint();
    log.Info($"C# IoT Hub trigger function processed a message: {myIoTHubMessage}");

    try
    {
        /* read in the sensor data from json data */
        newDataPoint =  Newtonsoft.Json.JsonConvert.DeserializeObject<TelemetryDataPoint>(myIoTHubMessage);

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
		string myConnection = ConfigurationManager.ConnectionStrings["dbaseConnectionString"].ConnectionString;

        using (SqlConnection connection = new SqlConnection(myConnection))
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
                        Object[] dataFields = new Object[nrOfFields];
                        reader.GetSqlValues(dataFields);
                        // log.Info("Id: " + reader.GetName(0) + " " + dataFields[0]);
                        // log.Info("Id: " + reader.GetName(2) + " " + dataFields[2]);
                    }
                    log.Info("New records read from db:");
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
            /* create connection string */
			string myConnectionWrite = ConfigurationManager.ConnectionStrings["dbaseConnectionString"].ConnectionString;

            /* SQL server does not have boolean data type, so the value has to bit - either 0 or 1 */
            int HallValue = 0;
            if (newDataPoint.Switch.Hall)
            {
                HallValue = 1;
            }

            using (SqlConnection connection = new SqlConnection(myConnectionWrite))
            {
                log.Info("\nWrite data to Azure SQL database");

                /* Note that when ',' is used as a delimeter for the decimal number, it need to be changed into '.' due SQL syntax */
                string sqlCommandContents = String.Format("INSERT INTO SensorDataStream " +
                    "(SensorId,Temperature, Humidity, AccelerationX, AccelerationY, AccelerationZ, AmbientLight, " +
                    "Pressure, SwitchDataUser, Hall, VibrationFrequency, VibrationPower, BatteryLevel) " +
                    "VALUES ({0}, {1:0.##}, {2:0.##}, {3:0.####}, {4:0.####}, {5:0.####}, {6}, {7}, '{8}', {9}, {10}, {11}, {12})", 
                    newDataPoint.SensorNodeId, newDataPoint.Temperature.ToString().Replace(",", "."),
                    newDataPoint.Humidity.ToString().Replace(",", "."), newDataPoint.Acceleration.X.ToString().Replace(",", "."), 
                    newDataPoint.Acceleration.Y.ToString().Replace(",", "."), newDataPoint.Acceleration.Z.ToString().Replace(",", "."), 
                    newDataPoint.Ambient_light, newDataPoint.Pressure, newDataPoint.Switch.User1, HallValue,
                    newDataPoint.Vibration.Frequency, newDataPoint.Vibration.Power, newDataPoint.Node_status.Battery_level);

                SqlCommand cmd = new SqlCommand(sqlCommandContents, connection);
               
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
