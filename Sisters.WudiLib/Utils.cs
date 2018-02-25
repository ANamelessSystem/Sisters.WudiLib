﻿using Newtonsoft.Json;
using Sisters.WudiLib.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sisters.WudiLib
{
    internal static class Utils
    {
        private static async Task<HttpApiResponse<T>> PostApiAsync<T>(string url, object data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data), "data不能为null");
            string json = JsonConvert.SerializeObject(data);
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var http = new HttpClient())
            {
                using (var response = (await http.PostAsync(url, content)).EnsureSuccessStatusCode())
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<HttpApiResponse<T>>(responseContent);
                    return result;
                }
            }
        }

        /// <summary>
        /// 通过 POST 请求访问API，返回数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">API请求地址</param>
        /// <param name="data">请求参数</param>
        /// <returns>从 HTTP API 返回的数据</returns>
        public static async Task<T> PostAsync<T>(string url, object data)
        {
            var response = await PostApiAsync<T>(url, data);
            return response.Retcode == HttpApiResponse.RetcodeOK ? response.Data : default(T);
        }

        /// <summary>
        /// 通过 POST 请求访问API，返回数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">API请求地址</param>
        /// <param name="data">请求参数</param>
        /// <returns>从 HTTP API 返回的数据</returns>
        public static T Post<T>(string url, object data)
        {
            try
            {
                return PostAsync<T>(url, data).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        public static async Task<bool> PostAsync(string url, object data)
        {
            try
            {
                var response = await PostApiAsync<object>(url, data);
                return response.Retcode == HttpApiResponse.RetcodeOK;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// 通过 POST 请求访问API，返回是否成功（Retcode 为 0）
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">API请求地址</param>
        /// <param name="data">请求参数</param>
        /// <param name="responseData">从 HTTP API 返回的数据</param>
        /// <returns>调用 API 是否成功</returns>
        public static bool Post<T>(string url, object data, out T responseData)
        {
            try
            {
                var response = PostApiAsync<T>(url, data).Result;
                responseData = response.Data;
                return response.Retcode == HttpApiResponse.RetcodeOK;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        internal static string BeforeSend(this string before)
            => before.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;").Replace(",", "&#44;");

        internal static string AfterReceive(this string received)
            => received.Replace("&#44;", ",").Replace("&#91;", "[").Replace("&#93;", "]").Replace("&amp;", "&");
    }
}
