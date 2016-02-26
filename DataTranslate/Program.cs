using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using SHWD.DataTranslate.Model;
using SHWD.DataTranslate.Process;

namespace SHWD.DataTranslate
{
    class Program
    {
        private static List<DevStatPair> _devStatPairList;

        private static Translater _translater;

        static void Main()
        {
            Init();
            //Daemon();
            RunOnce();
        }

        static void Init()
        {
            _translater = new Translater();
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
                    
                }
                Thread.Sleep(60000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        static void DoTranslate()
        {
            foreach (var devStatPair in _devStatPairList)
            {
                _translater.TranslateMinToWdDb(devStatPair.DevId, devStatPair.StatCode, DateTime.Now.AddMinutes(-1), DateTime.Now);
            }
        }

        static void RunOnce()
        {
            foreach (var devStatPair in _devStatPairList)
            {
                _translater.TranslateMinToWdDb(devStatPair.DevId, devStatPair.StatCode, DateTime.Now.AddYears(-2), DateTime.Now);
            }
        }
    }
}
