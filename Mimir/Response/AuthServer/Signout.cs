﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimir.Response.AuthServer
{
    public class Signout
    {
        public static Tuple<int, string> OnPost(string PostData)
        {
            JsonConvert.DeserializeObject<Request>(PostData);

            return new Tuple<int, string>(204, "");
        }

        struct Request
        {
            public string username;
            public string password;
        }
    }
}
