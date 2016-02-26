﻿using System;
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

        public Translater()
        {
            _mySqlContext = new data_centerEntities();
            _sqlContext = new ESMonitorEntities();
            _devList = _sqlContext.T_Devs.ToList();
            _statList = _sqlContext.T_Stats.ToList();
        }

        public void TranslateMinToWdDb(int devid, string targetStatCode, DateTime startDate, DateTime endDate)
        {
            var dev = _devList.First(obj => obj.Id == devid);
            var stat = _statList.First(obj => obj.Id.ToString() == dev.StatId);

            var mysqlData = _mySqlContext.sensor_data_min.Where(obj => obj.DataTime >= startDate && obj.DataTime <= endDate && obj.StatCode == targetStatCode)
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

                    _sqlContext.T_ESMin.Add(sqlData);
                }
            }

            _sqlContext.SaveChanges();
        }
    }
}