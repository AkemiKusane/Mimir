﻿using IniParser;
using IniParser.Model;
using Mimir.SQL;
using RUL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimir
{
    /// <summary>
    /// 配置文件管理类
    /// </summary>
    class ConfigWorker
    {
        private static Logger log = new Logger("Config");
        private static readonly FileIniDataParser parser = new FileIniDataParser();
        private static IniData ini;

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static void Load()
        {
            log.Info("Loading configs...");

            if (File.Exists("config.ini"))
            {
                ini = parser.ReadFile("config.ini");
            }
            else
            {
                ini = new IniData();
            }

            Program.ServerName = Read("General", "ServerName", Program.ServerName);
            bool.TryParse(Read("General", "IsDebug", Program.IsDebug.ToString()), out Program.IsDebug);
            int.TryParse(Read("General", "Port", Program.Port.ToString()), out Program.Port);
            int.TryParse(Read("General", "MaxConnection", Program.MaxConnection.ToString()), out Program.MaxConnection);

            Enum.TryParse(Read("SQL", "Type", Program.SqlType.ToString(), true), out Program.SqlType);
            CommitWrite("SQL", "Type", "Allow value: Sqlite, MySql.");
            Program.SqlDbName = Read("SQL", "DatabaseName", Program.SqlDbName);
            switch (Program.SqlType)
            {
                case SqlConnectionType.MySql:
                    Program.SqlIp = Read("SQL", "IP", Program.SqlIp);
                    Program.SqlUsername = Read("SQL", "Username", Program.SqlUsername);
                    Program.SqlPassword = Read("SQL", "Password", Program.SqlPassword, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool.TryParse(Read("SSL", "Enable", Program.SslIsEnable.ToString()), out Program.SslIsEnable);
            if (Program.SslIsEnable)
            {
                Enum.TryParse(Read("SSL", "CertSource", Program.SslCertSource.ToString()), out Program.SslCertSource);
                CommitWrite("SSL", "CertSource", "Allow value: Own, Generate.");
                Program.SslCertName = Read("SSL", "CertName", Program.SslCertName);
                Program.SslCertPassword = Read("SSL", "CertPassword", Program.SslCertPassword, true);
            }

            bool.TryParse(Read("Users", "AllowRegister", Program.UserAllowRegister.ToString()), out Program.UserAllowRegister);
            int.TryParse(Read("Users", "MaxRegistration", Program.UserMaxRegistration.ToString()), out Program.UserMaxRegistration);

            int.TryParse(Read("Security", "RegisterTimesPerMinute", Program.SecurityRegisterTimesPerMinute.ToString()), out Program.SecurityRegisterTimesPerMinute);
            int.TryParse(Read("Security", "LoginTimesPerMinute", Program.SecurityLoginTimesPerMinute.ToString()), out Program.SecurityLoginTimesPerMinute);
            int.TryParse(Read("Security", "MaxAPIQuery", Program.SecurityMaxApiQuery.ToString()), out Program.SecurityMaxApiQuery);

            Enum.TryParse(Read("Skins", "Source", Program.SkinSource.ToString(), true), out Program.SkinSource);
            CommitWrite("Skins", "Source", "Allow value: Mojang, Local.");
            switch (Program.SkinSource)
            {
                case SkinSource.Mojang:
                    break;
                case SkinSource.Local:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            log.Info("Configs loaded.");
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static void Save()
        {
            log.Info("Saving configs...");

            Write("General", "ServerName", Program.ServerName);
            Write("General", "IsDebug", Program.IsDebug.ToString());
            Write("General", "Port", Program.Port.ToString());
            Write("General", "MaxConnection", Program.MaxConnection.ToString());

            Write("SQL", "Type", Program.SqlType.ToString());
            Write("SQL", "DatabaseName", Program.SqlDbName);
            switch (Program.SqlType)
            {
                case SqlConnectionType.MySql:
                    Write("SQL", "IP", Program.SqlIp);
                    Write("SQL", "Username", Program.SqlUsername);
                    Write("SQL", "Password", Program.SqlPassword);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Write("SSL", "Enable", Program.SslIsEnable.ToString());
            if (Program.SslIsEnable)
            {
                Write("SSL", "CertSource", Program.SslCertSource.ToString());
                Write("SSL", "CertName", Program.SslCertName);
                Write("SSL", "CertPassword", Program.SslCertPassword);
            }

            Write("Users", "AllowRegister", Program.UserAllowRegister.ToString());
            Write("Users", "MaxRegistration", Program.UserMaxRegistration.ToString());

            Write("Security", "RegisterTimesPerMinute", Program.SecurityRegisterTimesPerMinute.ToString());
            Write("Security", "LoginTimesPerMinute", Program.SecurityLoginTimesPerMinute.ToString());
            Write("Security", "MaxAPIQuery", Program.SecurityMaxApiQuery.ToString());

            Write("Skins", "Source", Program.SkinSource.ToString());
            switch (Program.SkinSource)
            {
                case SkinSource.Mojang:
                    break;
                case SkinSource.Local:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            log.Info("Configs saved.");
        }

        /// <summary>
        /// 读INI方法
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="isHide">是否不向控制台显示值（用于密码等）</param>
        /// <returns>值</returns>
        private static string Read(string section, string key, string defaultValue, bool isHide = false)
        {
            string value = defaultValue;
            try
            {
                if (ini[section][key].Length != 0)
                {
                    value = ini[section][key];
                }
                else
                {
                    throw new NullReferenceException();
                }

                if (!isHide)
                {
                    log.Info($"Key \"{section}.{key}\" is set to value \"{value}\".");
                }
                else
                {
                    log.Info($"Key \"{section}.{key}\" is loaded.");
                }
            }
            catch (Exception)
            {
                log.Warn($"Key \"{section}.{key}\" can't load, will use default value.");
                Write(section, key, defaultValue);
            }
            return value;
        }

        /// <summary>
        /// 写INI方法
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        private static void Write(string section, string key, string value)
        {
            try
            {
                ini[section].AddKey(key, value);
                parser.WriteFile("config.ini", ini);
            }
            catch (Exception ex)
            {
                log.Warn($"Can't write value '{value}' to key '{section}.{key}'.");
                log.Error(ex);
            }
        }

        /// <summary>
        /// 写注释方法
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="commit">注释值</param>
        private static void CommitWrite(string section, string key, string commit)
        {
            try
            {
                ini[section].GetKeyData(key).Comments = new List<string>() { commit };
                parser.WriteFile("config.ini", ini);
            }
            catch (Exception ex)
            {
                log.Warn($"Can't write commit '{commit}' to key '{section}.{key}'.");
                log.Error(ex);
            }
        }

        /// <summary>
        /// 皮肤加载源
        /// </summary>
        public enum SkinSource
        {
            Mojang,
            Local
        }

        /// <summary>
        /// Ssl证书加载类型
        /// </summary>
        public enum SslCertSource
        {
            Own,
            Generate
        }
    }
}
