﻿using Mimir.CLI;
using Mimir.SQL;
using Mimir.Util;
using RUL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mimir
{
    class Program
    {
        #region 定义变量
        public const string Version = "0.7.4";

        public static string Path = Directory.GetCurrentDirectory();
                
        public static string ServerName = "Mimir Server";
        
        public static int Port = 45672;
        public static int MaxConnection = 233;

        public static bool IsRunning = false;
        public static bool IsDebug = false;

        public static SqlConnectionType SqlType = SqlConnectionType.MySql;
        public static string SqlDbName = "mimir";
        public static string SqlIp = "127.0.0.1";
        public static string SqlUsername = "root";
        public static string SqlPassword = "123456";

        public static bool UserAllowRegister = false;
        public static int UserRegisterTimesPerMinute = 5;
        internal static int UserRegisterTimes = 0;
        public static int UserMaxRegistration = 4567;
        public static int UserLoginTryTimesPerMinute = 5;
        public static int UserMaxApiQuery = 2;

        public static string SkinPublicKey = "";
        public static ConfigWorker.SkinSource SkinSource = ConfigWorker.SkinSource.Mojang;

        #endregion

        private static Logger log = new Logger("Main");
        private static SocketWorker socket;

        static void Main(string[] args)
        {
            log.Info($"Mimir version: {Version}, made by: Romonov! ");
            log.Info("Starting...");

            ConfigWorker.Load();

            if (!File.Exists($@"{Path}\PublicKey.xml") || !File.Exists($@"{Path}\PrivateKey.xml"))
            {
                log.Warn("Private key file is missing, and it will be generated now.");
                RSAWorker.GenKey();
            }
            RSAWorker.LoadKey();
            SkinPublicKey = RSAWorker.RSAPublicKeyConverter(RSAWorker.PublicKey.ToXmlString(false));

            try
            {
                SqlProxy.Open();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                log.Info("Init failed.");
                CommandHandler.Stop(1);
            }

            try
            {
                socket = new SocketWorker(Port, MaxConnection);
                socket.Start();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                log.Info("Init failed.");
                CommandHandler.Stop(2);
            }

            try
            {
                Poller.Start();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                log.Info("Init failed.");
                CommandHandler.Stop(3);
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
