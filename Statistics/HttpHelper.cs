//https://github.com/laomms

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Statistics
{
	public static class HttpHelper
    {

		static string[] RestrictedHeaders = new string[] {
			"Accept",
			"Connection",
			"Content-Length",
			"Content-Type",
			"Date",
			"Expect",
			"Host",
			"If-Modified-Since",
			"Keep-Alive",
			"Proxy-Connection",
			"Range",
			"Referer",
			"Transfer-Encoding",
			"User-Agent"
	};

		static Dictionary<string, PropertyInfo> HeaderProperties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

		static HttpHelper()
		{
			Type type = typeof(HttpWebRequest);
			foreach (string header in RestrictedHeaders)
			{
				string propertyName = header.Replace("-", "");
				PropertyInfo headerProperty = type.GetProperty(propertyName);
				HeaderProperties[header] = headerProperty;
			}
		}

		public static void SetRawHeader(this HttpWebRequest request, string name, string value)
		{
			if (HeaderProperties.ContainsKey(name))
			{
				PropertyInfo property = HeaderProperties[name];
				if (property.PropertyType == typeof(DateTime))
					property.SetValue(request, DateTime.Parse(value), null);
				else if (property.PropertyType == typeof(bool))
					property.SetValue(request, Boolean.Parse(value), null);
				else if (property.PropertyType == typeof(long))
					property.SetValue(request, Int64.Parse(value), null);
				else
					property.SetValue(request, value, null);
			}
			else
			{
				request.Headers[name] = value;
			}
		}
		public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
		{
			ArrayList al = new ArrayList();
			CookieCollection cc = new CookieCollection();

			if (!string.IsNullOrEmpty(strHeader))
			{
				al = ConvertCookieHeaderToArrayList(strHeader);
				cc = ConvertCookieArraysToCookieCollection(al, strHost);
			}

			return cc;
		}
		private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
		{
			if (strCookHeader == null)
			{
				return null;
			}

			strCookHeader = strCookHeader.Replace("\r", "");
			strCookHeader = strCookHeader.Replace("\n", "");
			string[] strCookTemp = strCookHeader.Split(',');
			ArrayList al = new ArrayList();
			int i = 0;
			int n = strCookTemp.Length;

			while (i < n)
			{

				if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
				{
					al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
					i = i + 1;
				}
				else
				{
					al.Add(strCookTemp[i]);
				}

				i = i + 1;
			}

			return al;
		}
		private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
		{

			CookieCollection cc = new CookieCollection();
			int alcount =al.Count;
			string strEachCook = null;
			string[] strEachCookParts = null;

			for (int i = 0; i < alcount; i++)
			{
				strEachCook = al[i].ToString();
				strEachCookParts = strEachCook.Split(';');
				int intEachCookPartsCount = strEachCookParts.Length;
				string strCNameAndCValue = string.Empty;
				string strPNameAndPValue = string.Empty;
				string strDNameAndDValue = string.Empty;
				string[] NameValuePairTemp = null;
				Cookie cookTemp = new Cookie();

				for (int j = 0; j < intEachCookPartsCount; j++)
				{

					if (j == 0)
					{
						strCNameAndCValue = strEachCookParts[j];

						if (!string.IsNullOrEmpty(strCNameAndCValue))
						{
							int firstEqual = strCNameAndCValue.IndexOf("=");
							string firstName = strCNameAndCValue.Substring(0, firstEqual);
							string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1)).Trim();
							cookTemp.Name = firstName;
							cookTemp.Value = allValue;
						}

						continue;
					}

					if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						strPNameAndPValue = strEachCookParts[j];

						if (!string.IsNullOrEmpty(strPNameAndPValue))
						{
							NameValuePairTemp = strPNameAndPValue.Split('=');

							if (!string.IsNullOrEmpty(NameValuePairTemp[1]))
							{
								cookTemp.Path = NameValuePairTemp[1];
							}
							else
							{
								cookTemp.Path = "/";
							}
						}

						continue;
					}

					if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						strPNameAndPValue = strEachCookParts[j];

						if (!string.IsNullOrEmpty(strPNameAndPValue))
						{
							NameValuePairTemp = strPNameAndPValue.Split('=');

							if (!string.IsNullOrEmpty(NameValuePairTemp[1]))
							{
								cookTemp.Domain = NameValuePairTemp[1];
							}
							else
							{
								cookTemp.Domain = strHost;
							}
						}

						continue;
					}
				}

				if (string.IsNullOrEmpty(cookTemp.Path))
				{
					cookTemp.Path = "/";
				}

				if (string.IsNullOrEmpty(cookTemp.Domain))
				{
					cookTemp.Domain = strHost;
				}

				if (!string.IsNullOrEmpty(cookTemp.Value))
				{
					cc.Add(cookTemp);
				}
			}

			return cc;
		}

		/// <summary>
		/// 遍歷CookieContainer
		/// </summary>
		/// <param name="cc"></param>
		/// <returns></returns>
		public static List<Cookie> GetAllCookies(CookieContainer cc)
		{
			List<Cookie> lstCookies = new List<Cookie>();
			Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
				System.Reflection.BindingFlags.Instance, null, cc, new object[] { });
			foreach (object pathList in table.Values)
			{
				SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
				foreach (CookieCollection colCookies in lstCookieCol.Values)
					foreach (Cookie c in colCookies) lstCookies.Add(c);
			}
			return lstCookies;
		}

		/// <summary>
		/// Http请求
		/// </summary>
		/// <param name="url">请求网址</param>
		/// <param name="Headerdics">头文件固定KEY值字典类型泛型集合</param>
		/// <param name="heard">头文件集合</param>
		/// <param name="cookieContainers">cookie容器</param>
		/// <param name="redirecturl">头文件中的跳转链接</param>
		/// <returns>返回请求字符串结果</returns>
		public static string RequestGet(string url, Dictionary<string, string> Headerdics, WebHeaderCollection heard, CookieContainer cookieContainers, ref string redirecturl)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>	{return true;};
			//Dim domain = CStr(Regex.Match(url, "^(?:\w+://)?([^/?]*)").Groups(1).Value)
			//If domain.Contains("www.") = True Then
			//    domain = domain.Replace("www.", "")
			//Else
			//    domain = "." & domain
			//End If
			if (string.IsNullOrEmpty(url))
			{
				return "";
			}
			HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
			myRequest.Headers = heard;
			myRequest.Method = "GET";
			foreach (var pair in Headerdics)
			{
				typeof(HttpWebRequest).GetProperty(pair.Key).SetValue(myRequest, pair.Value, null);
			}
			myRequest.CookieContainer = cookieContainers;
			string results = "";

			try
			{
				using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
				{
					if (myResponse.ContentEncoding.ToLower().Contains("gzip"))
					{
						using (Stream stream = new System.IO.Compression.GZipStream(myResponse.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress))
						{
							using (var reader = new StreamReader(stream))
							{
								results = reader.ReadToEnd();
							}
						}
					}
					else if (myResponse.ContentEncoding.ToLower().Contains("deflate"))
					{
						using (Stream stream = new System.IO.Compression.DeflateStream(myResponse.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress))
						{
							using (var reader = new StreamReader(stream))
							{
								results = reader.ReadToEnd();
							}
						}
					}
					else
					{
						using (Stream stream = myResponse.GetResponseStream())
						{
							using (var reader = new StreamReader(stream, Encoding.UTF8))
							{
								results = reader.ReadToEnd();
							}
						}
					}
					if (myResponse.Headers["Location"] != null)
					{
						redirecturl = myResponse.Headers["Location"];
					}
				}

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message.ToString());
			}
			return results;
		}
		/// <summary>
		/// Http响应
		/// </summary>
		/// <param name="url">请求网址</param>
		/// <param name="Headerdics">头文件固定KEY值字典类型泛型集合</param>
		/// <param name="heard">头文件集合</param>
		/// <param name="postdata">提交的字符串型数据</param>
		/// <param name="cookieContainers">cookie容器</param>
		/// <param name="redirecturl">头文件中的跳转链接</param>
		/// <returns>返回响应字符串结果</returns>
		public static string RequestPost(string url, Dictionary<string, string> Headerdics, WebHeaderCollection heard, string postdata, CookieContainer cookieContainers, ref WebHeaderCollection ResponseHeaders, ref string redirecturl)
		{
			if (string.IsNullOrEmpty(url))
			{
				return "";
			}
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>{return true;};
			//Dim domain = CStr(Regex.Match(url, "^(?:\w+://)?([^/?]*)").Groups(1).Value)
			//If domain.Contains("www.") = True Then
			//    domain = domain.Replace("www.", "")
			//Else
			//    domain = domain
			//End If
			string results = "";
			try
			{
				var myRequest = (HttpWebRequest)WebRequest.Create(url);
				var data = Encoding.UTF8.GetBytes(postdata);
				myRequest.Headers = heard;
				myRequest.Method = "POST";
				foreach (var pair in Headerdics)
				{
					typeof(HttpWebRequest).GetProperty(pair.Key).SetValue(myRequest, pair.Value, null);
				}
				myRequest.CookieContainer = cookieContainers;
				myRequest.ContentLength = data.Length;
				using (var stream = myRequest.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}

				using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
				{
					if (myResponse.ContentEncoding.ToLower().Contains("gzip"))
					{
						using (var stream = myResponse.GetResponseStream())
						{
							using (StreamReader reader = new StreamReader(new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress), Encoding.UTF8))
							{
								results = reader.ReadToEnd();
							}
						}
					}
					else if (myResponse.ContentEncoding.ToLower().Contains("deflate"))
					{
						using (var stream = myResponse.GetResponseStream())
						{
							using (StreamReader reader = new StreamReader(new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress), Encoding.UTF8))
							{
								results = reader.ReadToEnd();
							}
						}
					}
					else
					{
						using (Stream stream = myResponse.GetResponseStream())
						{
							using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
							{
								results = reader.ReadToEnd();
							}
						}
					}
					if (myResponse.Headers["Location"] != null)
					{
						redirecturl = myResponse.Headers["Location"];
					}
					ResponseHeaders = myResponse.Headers;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message.ToString());
			}
			return results;
		}

		public static string WebClientPost(string url, Dictionary<string, string> parms, WebHeaderCollection headers)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
			{
				return true;
			};
			string result = "";
			using (WebClient client = new WebClient())
			{
				try
				{
					client.Headers = headers;
					client.Encoding = Encoding.UTF8;
					var reqparm = new System.Collections.Specialized.NameValueCollection();
					foreach (var keyPair in parms)
					{
						reqparm.Add(keyPair.Key, keyPair.Value);
					}
					byte[] responsebytes = client.UploadValues(url, "POST", reqparm);
					result = Encoding.UTF8.GetString(responsebytes);
				}
				catch (WebException ex)
				{
					using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
					{
						string responseContent = r.ReadToEnd();
						Debug.WriteLine(responseContent);
					}
				}
			}
			return result;
		}

		public static string WebClientGet(string url, WebHeaderCollection headers, string host)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
			{
				return true;
			};
			string result = "";
			using (WebClient client = new WebClient())
			{
				try
				{
					client.Headers = headers;
					client.Headers["Host"] = host;
					//client.Headers.Add("Accept-Encoding", "gzip, deflate")
					//client.Headers.Add("Connection", "Keep-Alive")
					client.Encoding = Encoding.UTF8;
					result = client.DownloadString(url);
				}
				catch (WebException ex)
				{
					using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
					{
						string responseContent = r.ReadToEnd();
						Debug.WriteLine(responseContent);
					}
				}
			}
			return result;
		}

	}
}
