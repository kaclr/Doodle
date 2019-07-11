using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace NssIntegration
{
    using Doodle;

    public static class IntegrationServer
    {
        private static string s_serverAddress;
        private static Func<string> s_onGetServerAddress;

        public static void Init(Func<string> onGetServerAddress)
        {
            s_onGetServerAddress = onGetServerAddress;
        }

        public static int RequestBuildVersion()
        {
            InitInner();

            var ret = DoPost("RequestBuildVersion", "{\"command\": \"RequestBuildVersion\"}");
            return (int)(long)ret["data"];
        }

        //public static void SetVersionNodeList(string name, VersionNodeList vnList)
        //{
        //    var dic = new Dictionary<object, object>();
        //    dic["command"] = "SetVersionNodeList";
        //    dic["name"] = name;
        //    dic["bytes"] = JsonUtil.EncodeBytes(File.ReadAllBytes(vnList.dataFile));

        //    DoPost("SetVersionNodeList", JsonConvert.SerializeObject(dic));
        //}

        //public static byte[] GetVersionNodeList(string name)
        //{
        //    var dic = new Dictionary<object, object>();
        //    dic["command"] = "GetVersionNodeList";
        //    dic["name"] = name;

        //    var ret = DoPost("GetVersionNodeList", JsonConvert.SerializeObject(dic));
        //    if (string.IsNullOrEmpty((string)ret["data"]))
        //    {
        //        return null;
        //    }

        //    return JsonUtil.DecodeBytes((string)ret["data"]);
        //}

        private static void InitInner()
        {
            if (s_serverAddress != null) return;
            if (s_onGetServerAddress == null) throw new DoodleException($"{nameof(IntegrationServer)} hasn't been Inited!");

            s_serverAddress = s_onGetServerAddress();
        }

        private static Dictionary<object, object> DoPost(string command, string data)
        {
            Logger.VerboseLog("post to http://9.134.1.105:8080");
            var retStr = HttpUtil.Post("http://9.134.1.105:8080", "application/json", data);

            var dicRet = JsonConvert.DeserializeObject<Dictionary<object, object>>(retStr);
            if ((string)dicRet["result"] == "failed")
            {
                throw new Exception(string.Format("{0}失败，详情如下：\n{1}", command, dicRet["msg"]));
            }

            return dicRet;
        }
    }
}
