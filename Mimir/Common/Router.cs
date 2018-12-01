﻿using Mimir.Response;
using Mimir.Response.AuthServer;
using Mimir.Response.Users;
using RUL;
using RUL.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mimir.Common
{
    class Router
    {
        /// <summary>
        /// 路由并处理请求
        /// </summary>
        /// <param name="httpReq">请求字符串</param>
        /// <param name="socket">Socket实例</param>
        /// <param name="EndPoint">远程终结点</param>
        public static void Route(string httpReq, Socket socket)
        {
            HttpReq req = HttpProtocol.Solve(httpReq);

            Logger.Info($"Got request {req.Url} from {socket.RemoteEndPoint}.");

            byte[] bcontect;
            string responseHeader = "";
            byte[] bresponse;

            Tuple<int, string, string> Response = new Tuple<int, string, string>(403, "text/plain", "");

            // 处理请求
            try
            {
                if (req.Method == Method.Get)
                {
                    switch (req.Url.ToLower())
                    {
                        case "/":
                            Response = Root.OnGet();
                            break;
                        default:
                            string reqFilePath = Program.Path + @"/Html" + req.Url;
                            if (File.Exists(reqFilePath))
                            {
                                FileInfo reqFile = new FileInfo(reqFilePath);
                                string reqFileExt = Path.GetExtension(reqFile.FullName);
                                string reqFileType = "text/plain";

                                switch (reqFileExt)
                                {
                                    case ".html":
                                    case ".htm":
                                        reqFileType = "text/html";
                                        break;
                                    case ".css":
                                        reqFileType = "text/css";
                                        break;
                                    case ".js":
                                        reqFileType = "application/javascript";
                                        break;
                                    case ".ico":
                                        reqFileType = "image/x-icon";
                                        break;
                                    default:
                                        break;
                                }
                                Response = new Tuple<int, string, string>(200, reqFileType, File.ReadAllText(reqFilePath));
                            }
                            break;
                    }
                }
                else if (req.Method == Method.Post)
                {
                    switch (req.Url.ToLower())
                    {
                        #region Users
                        case "/users/register":
                            Response = Register.OnPost(req.PostData);
                            break;
                        case "/users/login":
                            Response = Login.OnPost(req.PostData);
                            break;
                        case "/users/logout":
                            Response = LogOut.OnPost(req.PostData);
                            break;
                        #endregion

                        #region AuthServer
                        case "/authserver/authenticate":
                            Response = Authenticate.OnPost(req.PostData);
                            break;
                        case "/authserver/refresh":
                            Response = Refresh.OnPost(req.PostData);
                            break;
                        case "/authserver/validate":
                            Response = Validate.OnPost(req.PostData);
                            break;
                        case "/authserver/invalidate":
                            Response = Invalidate.OnPost(req.PostData);
                            break;
                        case "/authserver/signout":
                            Response = Signout.OnPost(req.PostData);
                            break;
                        #endregion
                        default:

                            //GET /sessionserver/session/minecraft/profile/{uuid}?unsigned={unsigned}
                            //if (Guid.TryParse(msg.Url.Split('/')[6], out Guid guid))
                            {

                            }
                            Response = new Tuple<int, string, string>(403, "text/plain", "");
                            break;
                    }
                }
                else
                {
                    Response = new Tuple<int, string, string>(403, "text/plain", "");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            // 发送返回
            bcontect = Encoding.Default.GetBytes(Response.Item3);
            responseHeader = HttpProtocol.Make(Response.Item1, Response.Item2, bcontect.Length);
            bresponse = Encoding.Default.GetBytes(responseHeader);

            if (Program.IsDebug)
            {
                Logger.Debug($"Response header: {responseHeader}");
                Logger.Debug($"Response contect: {Response.Item3}");
            }

            try
            {
                socket.Send(bresponse);
                socket.Send(bcontect);
            }
            catch (Exception e)
            {
                if (Program.IsDebug)
                {
                    Logger.Error(e);
                }
                else
                {
                    Logger.Error(e.Message);
                }
            }
            finally
            {
                socket.Close();
            }
        }

        /// <summary>
        /// 错误信息返回的自定义类型
        /// </summary>
        public struct ReturnError
        {
            public string error;
            public string errorMessage;
            public string cause;
        }
    }
}
