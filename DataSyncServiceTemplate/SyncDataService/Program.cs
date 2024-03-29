﻿using log4net;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace SyncDataService
{
    static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("MyLogger");

        private static Service _service;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
          //  AppSettings.Init();

            // self service installer/uninstaller
          /*  if (args != null && args.Length == 1
                && (args[0][0] == '-' || args[0][0] == '/'))
            {
                switch (args[0].Substring(1).ToLower())
                {
                    case "install":
                    case "i":
                        if (!ServiceInstallerUtility.InstallMe())
                            Logger.Fatal("服务安装失败！");
                        break;
                    case "uninstall":
                    case "u":
                        if (!ServiceInstallerUtility.UninstallMe())
                            Logger.Fatal("服务卸载失败！");
                        break;
                    default:
                        Logger.Error("Unrecognized parameters (allowed: /install and /uninstall, shorten /i and /u)");
                        break;
                }
                Environment.Exit(0);
            }
            */

            _service = new Service();
            var servicesToRun = new ServiceBase[] { _service };

            // console mode
            if (Environment.UserInteractive)
            {
                // register console close event
                _consoleHandler = ConsoleEventHandler;
                SetConsoleCtrlHandler(_consoleHandler, true);

                Console.Title = "智慧水务数据同步程序";

                Logger.Debug("程序正在以Console模式运行！");
                _service.Start();

              /*  Console.WriteLine("点击任何按钮以关闭程序...");
                Console.Read();

                _service.Stop();*/
            }
            // service mode
            else
            {
                Logger.Debug("程序正在以Windows服务模式运行！");
                ServiceBase.Run(servicesToRun);
            }
        }




        #region Page Event Setup
        enum ConsoleCtrlHandlerCode : uint
        {
// ReSharper disable InconsistentNaming
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
// ReSharper restore InconsistentNaming
        }
        delegate bool ConsoleCtrlHandlerDelegate(ConsoleCtrlHandlerCode eventCode);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handlerProc, bool add);
        static ConsoleCtrlHandlerDelegate _consoleHandler;
        #endregion

        #region Page Events
        static bool ConsoleEventHandler(ConsoleCtrlHandlerCode eventCode)
        {
            // Handle close event here...
            switch (eventCode)
            {
                case ConsoleCtrlHandlerCode.CTRL_C_EVENT:
                case ConsoleCtrlHandlerCode.CTRL_CLOSE_EVENT:
                case ConsoleCtrlHandlerCode.CTRL_BREAK_EVENT:
                case ConsoleCtrlHandlerCode.CTRL_LOGOFF_EVENT:
                case ConsoleCtrlHandlerCode.CTRL_SHUTDOWN_EVENT:

                    _service.Stop();

                    Environment.Exit(0);
                    break;
            }

            return (false);
        }
        #endregion
    }
}
