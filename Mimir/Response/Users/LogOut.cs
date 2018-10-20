﻿using Newtonsoft.Json;
using RUL.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimir.Response.Users
{
    class LogOut
    {

        public static Tuple<int, string> OnPost(string PostData)
        {
            // Post /users/logout
            JsonConvert.DeserializeObject<HttpReq>(PostData);

            return new Tuple<int, string>(204, "");
        }
    }
}
