using System;
using System.Collections.Generic;

namespace RuuviRestAPI.models
{
    public class RawDataBlob
    {
        public List<int> blob { get; set; }
    }

    public class Tag
    {
        public double accelX { get; set; }
        public double accelY { get; set; }
        public double accelZ { get; set; }
        public int dataFormat { get; set; }
        public bool favorite { get; set; }
        public double humidity { get; set; }
        public string id { get; set; }
        public int movementCounter { get; set; }
        public string name { get; set; }
        public double pressure { get; set; }
        public RawDataBlob rawDataBlob { get; set; }
        public int rssi { get; set; }
        public double temperature { get; set; }
        public double txPower { get; set; }
        public DateTime updateAt { get; set; }
        public string userBackground { get; set; }
        public double voltage { get; set; }
        public int? defaultBackground { get; set; }
        public int? measurementSequenceNumber { get; set; }
    }

    public class Location
    {
        public double accuracy { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class RootObject
    {
        public List<Tag> tags { get; set; }
        public int batteryLevel { get; set; }
        public string deviceId { get; set; }
        public string eventId { get; set; }
        public Location location { get; set; }
        public DateTime time { get; set; }
    }}