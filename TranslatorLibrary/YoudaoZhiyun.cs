using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using System.Security.Cryptography;

namespace TranslatorLibrary
{
    public class YoudaoZhiyun : ITranslator
    {
        string appId, appKey;
        string errorInfo;

        public async Task<string> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }

            if (desLang == "zh")
                desLang = "zh-CHS";
            if (srcLang == "zh")
                srcLang = "zh-CHS";
            if (desLang == "jp")
                desLang = "ja";
            if (srcLang == "jp")
                srcLang = "ja";
            if (desLang == "kr")
                desLang = "ko";
            if (srcLang == "kr")
                srcLang = "ko";

            string q = sourceText;
            string input = q.Length <= 20 ? q : q.Substring(0, 10) + q.Length + q.Substring(q.Length - 10);
            string salt = "04a1b52f-886c-4b11-bd99-e2dfc8b01834";
            string curtime = CommonFunction.GetTimeStamp();
            SHA256 sha = SHA256.Create();
            string sign = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(appId + input + salt + curtime + appKey))).Replace("-", "").ToLower();
            sha.Dispose();
            string url = $"https://openapi.youdao.com/api?q={q}&from={srcLang}&to={desLang}&appKey={appKey}&salt={salt}&sign={sign}&signType=v3&curtime={curtime}";

            var hc = CommonFunction.GetHttpClient();
            string retString;
            try
            {
                retString = await hc.GetStringAsync(url);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                errorInfo = ex.Message;
                return null;
            }

            var result = JsonConvert.DeserializeObject<YoudaoZhiyunResult>(retString);

            if (result.errorCode == "0")
            {
                return string.Join("\n", result.translation);
            }
            else
            {
                errorInfo = "ErrorCode: " + result.errorCode;
                return null;
            }
        }

        public void TranslatorInit(string param1, string param2)
        {
            appId = param1;
            appKey = param2;
        }


        public string GetLastError()
        {
            return errorInfo;
        }

        /// <summary>
        /// 有道智云API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_allpyAPI()
        {
            return "https://ai.youdao.com/product-fanyi-text.s";
        }

        /// <summary>
        /// 有道智云API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_bill()
        {
            return "https://ai.youdao.com/console";
        }

        /// <summary>
        /// 有道智云API文档地址（错误代码）
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://ai.youdao.com/DOCSIRMA/html/自然语言翻译/API文档/文本翻译服务/文本翻译服务-API文档.html";
        }
    }

#pragma warning disable 0649
    struct YoudaoZhiyunResult
    {
        public string errorCode, query, l;
        public string[] translation;
    }
}
