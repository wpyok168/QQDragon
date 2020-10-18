using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Statistics
{
    class HttpHelper
    {
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
		public static string RequestGet(string url, string headeraccept, string contentype, string referer, WebHeaderCollection heard, ref CookieContainer cookieContainers, ref string redirecturl)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
			{
				return true;
			};

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
			//myRequest.KeepAlive = True
			myRequest.Accept = headeraccept;
			myRequest.ContentType = contentype;
			myRequest.Referer = referer;
			myRequest.AllowAutoRedirect = false;
			myRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4098.3 Safari/537.36";
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
			catch (Exception exp)
			{
				Debug.WriteLine(exp.ToString());
			}
			redirecturl = redirecturl;
			return results;
		}
		public static string RequestPost(string url, string headeraccept, string contentype, string referer, WebHeaderCollection heard, string postdata, ref CookieContainer cookieContainers, ref WebHeaderCollection myWebHeaderCollection, ref string redirecturl)
		{
			if (string.IsNullOrEmpty(url))
			{
				return "";
			}
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
			{
				return true;
			};

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
				//myRequest.KeepAlive = True
				myRequest.Accept = headeraccept;
				myRequest.ContentType = contentype;
				myRequest.Referer = referer;
				myRequest.AllowAutoRedirect = false;
				//myRequest.Headers.Add("Upgrade-Insecure-szRequests", "1")
				myRequest.Headers.Add("Cache-Control", "max-age=0");
				myRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4098.3 Safari/537.36";
				//myRequest.CookieContainer = cookieContainers
				myRequest.Headers.Add(HttpRequestHeader.Cookie, "os=pc;osver=Microsoft-Windows-10-Professional-build-16299.125-64bit;appver=2.0.3.131777;channel=netease;__remember_me=true");
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
					myWebHeaderCollection = myResponse.Headers;
				}

			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp.ToString());
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
