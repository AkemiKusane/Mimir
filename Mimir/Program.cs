﻿using Mimir.Common;
using Mimir.SQL;
using RUL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mimir
{
    class Program
    {
        #region 定义变量
        public const string Name = "Mimir";
        public const string Version = "0.6.0";

        public static string Path = Directory.GetCurrentDirectory();

        public static string SQLIP = "localhost";
        public static string SQLUsername = "root";
        public static string SQLPassword = "123456";
        public const string SQLDatabase = "mimir";
        
        public static string SkinPublicKey = "";

        public static string ServerName = "Mimir Server";

        public static string[] SkinDomains;

        public static int Port = 45672;
        public static int MaxConnection = 233;
        public static int SkinDomainsCount = 1;

        public static bool IsRunning = false;

        static SocketWorker SocketListener = new SocketWorker();
        
        public static ConfigWorker.SQLType SQLType = ConfigWorker.SQLType.MySql;
        #endregion

        static void Main(string[] args)
        {
            Logger.Info($"Mimir version: {Version}, made by: Romonov! ");
            Logger.Info("Starting...");

            // 初始化
            try
            {
                // 加载配置文件
                Logger.Info("Loading configs...");
                string ConfigPath = Directory.GetCurrentDirectory() + @"\config.ini";

                if (!File.Exists(ConfigPath))
                {
                    ConfigWorker.Init(ConfigPath);
                }
                else
                {
                    ConfigWorker.Read(ConfigPath);
                }

                Logger.Info("Configs loaded!");

                // 加载签名秘钥
                if (!File.Exists(Directory.GetCurrentDirectory() + @"\PrivateKey.xml"))
                {
                    Logger.Warn("Private key file is missing, and it will be generated now.");
                    RSAWorker.GenKey();
                }

                RSAWorker.LoadKey();

                Thread.Sleep(500);
                SkinPublicKey = RSAWorker.RSAPublicKeyConverter(File.ReadAllText(Directory.GetCurrentDirectory() + @"\PublicKey.xml"));

                // 打开SQL和Socket链接
                SqlProxy.Open();

                SocketListener.Init(Port, MaxConnection);
                SocketListener.Start();

                IsRunning = true;

            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            Logger.Info("Welcome!!");

            // 主循环 
            while (true)
            {
                string input = Console.ReadLine();
                Logger.WriteToFile(input);
                switch (input)
                {
                    case "stop":
                        SqlProxy.Close();
                        ConfigWorker.Save(Directory.GetCurrentDirectory() + @"\config.ini");
                        Environment.Exit(0);
                        break;
                    default:
                        Logger.Error("No such command.");
                        continue;
                }
            }
        }
    }
}
