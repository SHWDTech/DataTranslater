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

        public int TranslateMinToWdDb(int devid, string targetStatCode, int taskIndex)
        {
            var dev = GetTargetDevice(devid);

            Console.WriteLine($"获取到的设备信息：{dev.Id},- {dev.DevCode} - {dev.OuterCode} - {dev.StatId}");

            if (LastRecordId == -1)
            {
                GetLastRecordOutId(devid);
            }

            Console.WriteLine($"最后一次记录的ID号：{LastRecordId}");
            
            var stat = _statList.First(obj => obj.Id.ToString() == dev.StatId);

            Console.WriteLine($"工地相关信息：{stat.Id} - {stat.StatCode}");

            var mysqlData = _mySqlContext.sensor_data_min.Where(obj => obj.StatCode == targetStatCode && obj.ID > LastRecordId)
                .ToList();

            Console.WriteLine($"一共还有{mysqlData.Count}条记录要转换。");

            var count = 0;

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
                        UpdateTime = sensorDataMin.DataTime,
                        Humidity = sensorDataMin.Humidity
                    };

                    LastRecordId = sensorDataMin.ID;
                    _sqlContext.T_ESMin.Add(sqlData);
                    count += _sqlContext.SaveChanges();
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss fff")}：当前任务号：{taskIndex}保存数据成功。当前第{count}条，共{mysqlData.Count}条。");
                }
            }

            return count;
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
