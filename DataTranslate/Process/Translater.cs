using System;
using System.Collections.Generic;
using System.Linq;
using SHWD.DataTranslate.DataProvider;
using SHWD.DataTranslate.WDDataProvider;

namespace SHWD.DataTranslate.Process
{
    public class Translater
    {
        private readonly data_centerEntities _mySqlContext;

        private readonly ESMonitorEntities _sqlContext;

        private readonly List<T_Devs> _devList;

        private readonly List<T_Stats> _statList;

        private int LastRecordId { get; set; }

        public Translater()
        {
            _mySqlContext = new data_centerEntities();
            _sqlContext = new ESMonitorEntities();
            _devList = _sqlContext.T_Devs.ToList();
            _statList = _sqlContext.T_Stats.ToList();
            LastRecordId = -1;
        }

        public int TranslateMinToWdDb(int devid, string targetStatCode)
        {
            var dev = GetTargetDevice(devid);

            if (LastRecordId == -1)
            {
                GetLastRecordOutId(devid);
            }
            
            var stat = _statList.First(obj => obj.Id.ToString() == dev.StatId);

            var mysqlData = _mySqlContext.sensor_data_min.Where(obj => obj.StatCode == targetStatCode && obj.ID > LastRecordId)
                .ToList();

            if (mysqlData.Any())
            {
                foreach (var sensorDataMin in mysqlData)
                {
                    var sqlData = new T_ESMin
                    {
                        DB = sensorDataMin.DB,
                        TP = sensorDataMin.TP * 10,
                        DevId = dev.Id,
                        StatCode = sensorDataMin.ID,
                        StatId = int.Parse(dev.StatId),
                        Country = stat.Country.ToString(),
                        Rain = 0,
                        DataStatus = "N",
                        WindDirection = sensorDataMin.WindDirection,
                        WindSpeed = sensorDataMin.WindSpeed,
                        Temperature = sensorDataMin.Temperature,
                        Airpressure = sensorDataMin.AirPressure,
                        UpdateTime = sensorDataMin.DataTime
                    };

                    LastRecordId = sensorDataMin.ID;
                    _sqlContext.T_ESMin.Add(sqlData);
                }
            }

            return _sqlContext.SaveChanges();
        }

        private T_Devs GetTargetDevice(int deviceId)
        {
            var dev = _devList.FirstOrDefault(obj => obj.Id == deviceId);

            if(dev == null)
                throw new ArgumentException("不存在的设备ID");

            return dev;
        }

        private void GetLastRecordOutId(int deviceId)
        {
            var lastRecord = _sqlContext.T_ESMin.Where(item => item.DevId == deviceId && item.UpdateTime > new DateTime(2016, 1, 1))
                    .OrderByDescending(obj => obj.StatCode).FirstOrDefault();

            LastRecordId = lastRecord?.StatCode ?? 0;
        }
    }
}
