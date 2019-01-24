﻿using Mimir.CLI;
using RUL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mimir
{
    class Program
    {
        #region 定义变量
        public const string Name = "Mimir";
        public const string Version = "0.7.0";

        public static string Path = Directory.GetCurrentDirectory();
                
        public static string ServerName = "Mimir Server";
        
        public static int Port = 45672;
        public static int MaxConnection = 233;

        public static bool IsRunning = false;
        public static bool IsDebug = false;
        #endregion

        private static Logger log = new Logger("Main");
        private static SocketWorker socket;

        static void Main(string[] args)
        {
            log.Info($"Mimir version: {Version}, made by: Romonov! ");
            log.Info("Starting...");

            ConfigWorker.Load($@"{Path}\config.ini");

            //log.Info("Connecting database...");

            try
            {
                socket = new SocketWorker(Port, MaxConnection);
                socket.Start();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                CommandHandler.Stop(1);
            }
            
            log.Info("Welcome!!");
            log.Info("Input \"help\" for show help messages.");
            
            while (true)
            {
                string input = Console.ReadLine();
                log.WriteToFile(input);
                CommandHandler.Handle(input.Split(' '));
            }
        }

        public static Logger GetLogger()
        {
            return log;
        }
    }
}
