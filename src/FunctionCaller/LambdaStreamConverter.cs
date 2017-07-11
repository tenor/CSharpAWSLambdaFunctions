using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace FunctionHandler
{
    internal static class LambdaStreamConverter
    {
        public static HttpRequestMessage GetRequestMessage(Stream inputStream)
        {

            //Read stream;
            string content;

            using (var reader = new StreamReader(inputStream, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            //Attempt to deserialize input stream
            Object deserialized;

            try
            {
                deserialized = JsonConvert.DeserializeObject(content);
            }
            catch
            {
                //Error attempting to deserialize stream
                //Return an HttpRequestMessage with contents of payload.
                return new HttpRequestMessage
                {
                    Content = new StringContent(content, Encoding.UTF8)
                };
            }

            if (deserialized == null || !(deserialized is JObject))
            {
                return new HttpRequestMessage
                {
                    Content = new StringContent(content, Encoding.UTF8)
                };
            }

            var jsonObj = deserialized as JObject;

            //Check if input stream is from a Lambda proxy
            if (jsonObj["headers"] == null
                || jsonObj["httpMethod"] == null
                || jsonObj["path"] == null
                || jsonObj["requestContext"] == null)
            {
                return new HttpRequestMessage
                {
                    Content = new StringContent(content, Encoding.UTF8)
                };
            }

            //Create request message and add headers
            HttpRequestMessage req = new HttpRequestMessage();

            if (jsonObj["headers"] != null && jsonObj["headers"].HasValues)
            {
                foreach (var header in jsonObj["headers"])
                {
                    if (header is JProperty)
                    {
                        var key = (header as JProperty).Name;
                        var value = (header as JProperty).Value;

                        if (value != null && value is JValue && (value as JValue).Value is string)
                        {
                            req.Headers.Add(key, (value as JValue).Value as string);
                        }
                    }
                }
            }

            //Set the Uri
            string path;
            if (jsonObj["requestContext"]["path"] != null)
            {
                path = jsonObj["requestContext"]["path"].Value<string>();
            }
            else
            {
                path = jsonObj["path"].Value<string>();
            }

            //Add QueryStrings
            StringBuilder sb = new StringBuilder();

            if (jsonObj["queryStringParameters"] != null && jsonObj["queryStringParameters"].HasValues)
            {
                bool hasAppended = false;
                sb.Append("?");
                foreach (var query in jsonObj["queryStringParameters"])
                {
                    sb.Append(hasAppended ? '&' : '?');
                    hasAppended = true;

                    if (query is JProperty)
                    {
                        sb.Append((query as JProperty).Name);
                        var value = (query as JProperty).Value;
                        sb.Append('=');
                        if (value != null && value is JValue && (value as JValue).Value is string)
                        {
                            sb.Append((value as JValue).Value as string);
                        }
                    }
                }
            }

            req.RequestUri = new UriBuilder("https", req.Headers.Host, 443, path, sb.ToString()).Uri;

            //Set the HTTP method
            req.Method = new HttpMethod(jsonObj["httpMethod"].Value<string>());

            //Add body
            if (jsonObj["body"] != null && (jsonObj["body"] as JValue).Value != null)
            {
                if (jsonObj["isBase64Encoded"] != null && jsonObj["isBase64Encoded"].Value<bool>() == true)
                {
                    req.Content = new ByteArrayContent(Convert.FromBase64String(jsonObj["body"].Value<string>()));
                }
                else
                {
                    req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonObj["body"].Value<string>()));
                }
            }

            return req;
        }

        public static string GetResponseString(HttpResponseMessage message)
        {
            bool isBase64Encoded;
            string body = GetJsonFriendlyContent(message.Content, out isBase64Encoded);

            JObject respObj = new JObject(
                new JProperty("isBase64Encoded", isBase64Encoded),
                new JProperty("statusCode", (int)message.StatusCode)
                );

            JObject headers = new JObject();

            //Add response headers
            if (message.Headers != null)
            {
                foreach (var h in message.Headers)
                {
                    if (!String.IsNullOrWhiteSpace(h.Key))
                    {
                        var hVal = String.Empty;
                        if (h.Value != null && h.Value.Any()) hVal = h.Value.First();
                        headers.Add(h.Key, hVal);
                    }
                }
            }

            //Add content headers
            if (message.Content != null && message.Content.Headers != null)
            {
                //TODO: Consider decoding content body based on content headers

                foreach (var h in message.Content.Headers)
                {
                    if (!String.IsNullOrWhiteSpace(h.Key))
                    {
                        var hVal = String.Empty;
                        if (h.Value != null && h.Value.Any()) hVal = h.Value.First();
                        headers.Add(h.Key, hVal);
                    }
                }
            }

            if (headers.HasValues)
            {
                respObj.Add("headers", headers);
            }

            respObj.Add("body", body);

            return respObj.ToString();
        }

        static string GetJsonFriendlyContent(HttpContent content, out bool isBase64Encoded)
        {
            isBase64Encoded = false;
            if (content == null) return String.Empty;

            var bytes = content.ReadAsByteArrayAsync().Result;
            if (bytes.Length == 0) return String.Empty;

            //Attempt to read bytes as a UTF8 string
            try
            {
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch { }

            //Attempt failed
            //Return string as Base64Encoded string
            isBase64Encoded = true;
            return System.Convert.ToBase64String(bytes);
        }
    }
}
