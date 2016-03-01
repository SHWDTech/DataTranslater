using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading;
using log4net;
using SHWD.DataTranslate.Model;
using SHWD.DataTranslate.Process;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SHWD.DataTranslate
{
    class Program
    {
        private static List<DevStatPair> _devStatPairList;

        private static Translater _translater;

        static void Main()
        {
            if (!Init())
            {
                return;
            }

            Daemon();
        }

        static bool Init()
        {
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
            _devStatPairList = new List<DevStatPair>();

            var devStatPairString = ConfigurationManager.AppSettings["DevStatPair"].Split(';');
            foreach (var str in devStatPairString)
            {
                var keyValuePair = str.Split(',');
                var pair = new DevStatPair()
                {
                    DevId = int.Parse(keyValuePair[0]),
                    StatCode = keyValuePair[1]
                };

                _devStatPairList.Add(pair);
            }

            return true;
        }

        static void Daemon()
        {
            while (true)
            {
                try
                {
                    DoTranslate();
                }
                catch (Exception ex)
                {
                    var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                    log.Error("数据转换失败", ex);
                    Console.WriteLine($"数据转换失败,{ex.Message}，：{DateTime.Now}");
                }
                Thread.Sleep(60000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        static void DoTranslate()
        {
            foreach (var devStatPair in _devStatPairList)
            {
                var resultCount = _translater.TranslateMinToWdDb(devStatPair.DevId, devStatPair.StatCode);
                Console.WriteLine($"数据转换完成，原数据设备号：{devStatPair.StatCode}，本地设备编号：{devStatPair.DevId}，影响行数：{resultCount}。：{DateTime.Now}");
            }
        }
    }
}
