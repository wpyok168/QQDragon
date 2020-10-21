using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Statistics
{
	public class Main
	{

		#region 收到私聊消息
		public static Delegate funRecvicePrivateMsg = new RecvicePrivateMsg(RecvicetPrivateMessage);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int RecvicePrivateMsg(ref PInvoke.PrivateMessageEvent sMsg);
		public static int RecvicetPrivateMessage(ref PInvoke.PrivateMessageEvent sMsg)
		{			
			return 0;
		}
		#endregion
		#region 收到群聊消息
		public static RecviceGroupMsg funRecviceGroupMsg = new RecviceGroupMsg(RecvicetGroupMessage);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int RecviceGroupMsg(ref PInvoke.GroupMessageEvent sMsg);
		public static int RecvicetGroupMessage(ref PInvoke.GroupMessageEvent sMsg)
		{

			if (sMsg.SenderQQ != sMsg.ThisQQ)
			{
				if (sMsg.MessageContent == "谁是龙王")
				{
					//方法1：用本地请求取cookie
					CookieContainer mycookiecontainer = GetQQGroupCookie(sMsg.ThisQQ, sMsg.MessageGroupQQ);

					//方法2：用官方的api取cookie
					//CookieContainer mycookiecontainer = new CookieContainer();
					//bool sucess = API.GetCookie(PInvoke.plugin_key, sMsg.ThisQQ, "https://qun.qq.com", "715030901", "73",ref cookie);
					//if (sucess)
					//{
					//	...
					// }; 


					var head = new WebHeaderCollection()
					{
						{"sec-fetch-mode:navigate"},
						{"sec-fetch-site:none"},
						{"sec-fetch-user:?1"},
						{"Upgrade-Insecure-Requests:1"}
					 };
					Dictionary<string, string> Headerdics = new Dictionary<string, string>()
					{
						{"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3"},
						{"ContentType", "application/json, text/plain, */*"},
						{"Host", "qun.qq.com"},
						{"UserAgent", "Mozilla/5.0 (Linux; Android 10; MI 9 Build/QKQ1.190825.002; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/77.0.3865.120 MQQBrowser/6.2 TBS/045410 Mobile Safari/537.36 V1_AND_SQ_8.4.8_1492_YYB_D QQ/8.4.8.4810 NetType/WIFI WebP/0.3.0 Pixel/1080 StatusBarHeight/75 SimpleUISwitch/0 QQTheme/1000 InMagicWin/0"}
					};
					string url = "https://qun.qq.com/interactive/honorlist?gc=" + sMsg.MessageGroupQQ.ToString() + "&type=1";
					var redirect_geturl = string.Empty;
					string Res = HttpHelper.RequestGet(url, Headerdics, head, mycookiecontainer, ref redirect_geturl);
					if (Res != "")
					{
						MatchCollection match1 = Regex.Matches(Res, @"\{(?:[^\{\}]|(?<o>\{)|(?<-o>\}))+(?(o)(?!))\}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
						foreach (Match match in match1)
						{
							try
							{		
								dynamic token = new JavaScriptSerializer().Deserialize<dynamic>(match.Value);
								Dictionary<string, object> json = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(match.Value);
								int count = ((ArrayList)json["talkativeList"]).Count;
								if (token["gc"] != null && (string)token["gc"] == sMsg.MessageGroupQQ.ToString())
								{
									API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "现任龙王: " + (string)token["talkativeList"][0]["name"] + "(" + Convert.ToString(token["talkativeList"][0]["uin"]) + ")"  + " 蝉联天数: " + (string)token["talkativeList"][0]["desc"], false);
									if (count >1)
                                    {
										List<string> list = new List<string>();
										for (int i = 1; i < count; i++)
                                        {
											list.Add((string)token["talkativeList"][i]["name"] + "(" + Convert.ToString(token["talkativeList"][i]["uin"]) + ")"  + " 蝉联天数: " + (string)token["talkativeList"][i]["desc"]);
											if (i > 5)
												break;
										}
										API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "历史龙王: " + Environment.NewLine + string.Join(Environment.NewLine, list), false);
									}
								}

							}
							catch (Exception e)
							{
								Debug.WriteLine(e.Message.ToString());
							}
						}
					}
					else
                    {
						API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + Environment.NewLine + "没有龙王.", false);
					}
					
				}				
			}
			return 0;
		}
		#endregion

	//static CookieContainer GetQQZoneCookie(long thisqq)
    //     {
	//		CookieContainer mycookiecontainer = new CookieContainer();
	//		WebHeaderCollection myWebHeaderCollection = new WebHeaderCollection();
	//		var redirect_geturl = string.Empty;
	//		var head1 = new WebHeaderCollection();
	//		Dictionary<string, string> Headerdics = new Dictionary<string, string>()
	//		{
	//		    {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3"},
	//		    {"ContentType", "application/json, text/plain, */*"},
	//		    {"UserAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4098.3 Safari/537.36"}
	//		};
	//		string url = "https://ui.ptlogin2.qq.com/cgi-bin/login?pt_hide_ad=1&style=9&appid=549000929&pt_no_auth=1&pt_wxtest=1&daid=5& s_url= " + HttpUtility.UrlDecode("https://h5.qzone.qq.com/mqzone/index");
	//		var Res = HttpHelper.RequestGet(url, Headerdics, head1,  mycookiecontainer, ref redirect_geturl);
	//		url = "https://ssl.ptlogin2.qq.com/jump?u1=" + HttpUtility.UrlDecode("https://h5.qzone.qq.com/mqzone/index") + "&pt_report=1&daid=5&style=9&keyindex=19&clientuin=" + thisqq.ToString() + "&clientkey=" +  API.GetClientKey(PInvoke.plugin_key, thisqq); ;
	//		Res = HttpHelper.RequestGet(url, Headerdics, head1,  mycookiecontainer, ref redirect_geturl);
	//		if (Res != "")
	//		{
	//			return mycookiecontainer;
	//		};
	//		return mycookiecontainer;
	//	}

		static CookieContainer GetQQGroupCookie(long thisqq,long groupid)
		{
			string SKey = Marshal.PtrToStringAnsi(API.GetSKey(PInvoke.plugin_key, thisqq));
			string PSKey = Marshal.PtrToStringAnsi(API.GetPSKey(PInvoke.plugin_key, thisqq, "qun.qq.com"));
			CookieContainer mycookiecontainer = new CookieContainer();
            mycookiecontainer.Add(new Cookie("ptcz","07503c81ec2987e0f6715c9c9df2813df5d06298cd3ae185886aec2c6e20e998") { Domain = "qun.qq.com" });
            mycookiecontainer.Add(new Cookie("RK","wSpAjA + HFJ") { Domain = "qun.qq.com" });
			mycookiecontainer.Add(new Cookie("uin", "o" + thisqq.ToString()) { Domain = "qun.qq.com" });
			mycookiecontainer.Add(new Cookie("p_uin", "o" + thisqq.ToString()) { Domain = "qun.qq.com" });
			mycookiecontainer.Add(new Cookie("skey", SKey) { Domain = "qun.qq.com" });
			mycookiecontainer.Add(new Cookie("p_skey", PSKey) { Domain = "qun.qq.com" });

			int t = 5381;
			for (int r = 0, n = SKey.Length; r < n; ++r)
			{
				t += (t << 5) + (CharAt(SKey, r).ToCharArray()[0] & 0xff);
			}
			string bkn = (2147483647 & t).ToString();

			WebHeaderCollection myWebHeaderCollection = new WebHeaderCollection();
			var redirect_geturl = string.Empty;
			var head1 = new WebHeaderCollection();
			Dictionary<string, string> Headerdics = new Dictionary<string, string>()
			{
				{"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3"},
				{"ContentType", "application/json, text/plain, */*"},
				{"Referer", "http://qun.qq.com/member.html"},
				{"UserAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4098.3 Safari/537.36"}
			};
			string url = "http://qun.qq.com/cgi-bin/qun_mgr/search_group_members?gc="+ groupid + "&st=0&end=40&sort=0&bkn="+bkn;
			var Res = HttpHelper.RequestGet(url, Headerdics, head1, mycookiecontainer, ref redirect_geturl);
			if (Res != "")
            {
				return mycookiecontainer;
			};			
			return mycookiecontainer;
		}

		/// <summary>
		/// 計算gtk
		/// </summary>
		/// <returns></returns>
		//public static Int32 GetGTK(List<Cookie> cookies)
		//{
		//	int gtk = 0;
		//	foreach (var item in cookies)
		//	{
		//		if (item.Name == "skey")
		//		{
		//			int hash = 5381;
		//			string str = item.Value;
		//			for (int i = 0, len = str.Length; i < len; ++i)
		//			{
		//				hash += (hash << 5) + str.ElementAt(i);
		//			}
		//			gtk = hash & 0x7fffffff;
		//		}
		//	}
		//	return gtk;
		//}
		private static string CharAt(string s, int index)
		{
			if ((index >= s.Length) || (index < 0))
				return "";
			return s.Substring(index, 1);
		}
	}

}


