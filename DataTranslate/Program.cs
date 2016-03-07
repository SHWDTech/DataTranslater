using log4net;
using SHWD.DataTranslate.Model;
using SHWD.DataTranslate.Process;
using SHWD.DataTranslate.WDDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace SHWD.DataTranslate
{
    internal class Program
    {
        private static List<DevStatPair> _devStatPairList;

        private static Translater _translater;

        private static ESMonitorEntities _sqlContext;

        private static Dictionary<string, Thread> _threads;

        private static Thread _devStaThread;

        private static void Main()
        {
            if (!Init())
            {
                return;
            }

            Daemon();
        }

        private static bool Init()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            _devStatPairList = new List<DevStatPair>();

            _threads = new Dictionary<string, Thread>();
            _devStaThread = new Thread(TryGetUpdateStatList);
            _devStaThread.Start();

            try
            {
                _translater = new Translater();
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Error("数据库连接建立失败", ex);
                Console.WriteLine("数据库连接建立失败！");
                Console.ReadKey();
                return false;
            }

            return true;
        }

        private static void Daemon()
        {
            while (true)
            {
                lock (_devStatPairList)
                {
                    UpdateComplatedThreads();

                    RefrashThreads();
                }

                Thread.Sleep(90000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void UpdateComplatedThreads()
        {
            foreach (var devStatPair in from devStatPair in _devStatPairList
                                        where _threads.ContainsKey(devStatPair.StatCode)
                                        let thread = _threads.First(dic => dic.Key == devStatPair.StatCode)
                                        where !thread.Value.IsAlive
                                        select devStatPair)
            {
                _threads.Remove(devStatPair.StatCode);
            }
        }

        private static void RefrashThreads()
        {
            foreach (var devStatPair in _devStatPairList)
            {
                if (_threads.ContainsKey(devStatPair.StatCode)) continue;

                var updateThreads = new Thread(TryUpdateProcess);
                updateThreads.Start(devStatPair);

                _threads.Add(devStatPair.StatCode, updateThreads);
            }
        }

        private static void TryGetUpdateStatList()
        {
            while (true)
            {
                _sqlContext = new ESMonitorEntities();

                var devs =
                    _sqlContext.T_Devs.Where(
                        obj => obj.OuterCode != null && obj.OuterCode != string.Empty && obj.OuterCode != "").ToList();

                lock (_devStatPairList)
                {
                    foreach (var dev in devs)
                    {
                        if (_devStatPairList.Any(list => list.DevId == dev.Id)) continue;

                        var pair = new DevStatPair()
                        {
                            DevId = dev.Id,
                            StatCode = dev.OuterCode
                        };

                        _devStatPairList.Add(pair);
                    }

                    for (var i = 0; i < _devStatPairList.Count; i++)
                    {
                        if (devs.All(obj => obj.Id != _devStatPairList[i].DevId))
                        {
                            _devStatPairList.RemoveAt(i);
                        }
                    }
                }
                Thread.Sleep(120000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void TryUpdateProcess(object obj)
        {
            if (!(obj is DevStatPair)) return;
            var pair = (DevStatPair)obj;

            while (true)
            {
                if (_devStatPairList.All(item => item.DevId != pair.DevId)) return;
                try
                {
                    DoTranslate(pair.DevId, pair.StatCode);
                }
                catch (Exception ex)
                {
                    var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                    log.Error("数据转换失败", ex);
                    Console.WriteLine($"数据转换失败,{ex.Message}，：{DateTime.Now}");
                }

                Thread.Sleep(60000);
            }
        }

        private static void DoTranslate(int devId, string statCode)
        {
            var resultCount = _translater.TranslateMinToWdDb(devId, statCode);
            Console.WriteLine($"数据转换完成，原数据设备号：{devId}，本地设备编号：{statCode}，影响行数：{resultCount}。：{DateTime.Now}");
        }
    }
}