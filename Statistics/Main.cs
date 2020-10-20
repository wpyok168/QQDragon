using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
				if (sMsg.MessageContent=="谁是龙王")
				{

					string dragon = string.Empty;
					//方法一：通过API获取cookie
					//IntPtr SKey = API.GetSKey(PInvoke.plugin_key, sMsg.ThisQQ);
					//IntPtr PSKey = API.GetPSKey(PInvoke.plugin_key, sMsg.ThisQQ, "qun.qq.com");					
					//CookieContainer mycookiecontainer = new CookieContainer();
                    //mycookiecontainer.Add(new Cookie("uin", "o" + sMsg.ThisQQ.ToString()) { Domain = "qun.qq.com" });
                    //mycookiecontainer.Add(new Cookie("p_uin", "o" + sMsg.ThisQQ.ToString()) { Domain = "qun.qq.com" });
                    //mycookiecontainer.Add(new Cookie("skey", Marshal.PtrToStringAnsi(SKey)) { Domain = "qun.qq.com" });
                    //mycookiecontainer.Add(new Cookie("p_skey", Marshal.PtrToStringAnsi(PSKey)) { Domain = "qun.qq.com" });
                    //WebHeaderCollection myWebHeaderCollection = new WebHeaderCollection();

                    //方法二：通过httqrequest获取cookie
                    IntPtr clientkey = API.GetClientKey(PInvoke.plugin_key, sMsg.ThisQQ);
                    CookieContainer mycookiecontainer = GetCookie(Marshal.PtrToStringAnsi(clientkey), "https://h5.qzone.qq.com/mqzone/index", sMsg.ThisQQ.ToString(), "549000929", "5");
					mycookiecontainer.Add(new Cookie("p_uin", "o"+ sMsg.MessageGroupQQ.ToString()) { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("pgv_info", "ssid=s2222582605") { Domain = "qun.qq.com" });
                    mycookiecontainer.Add(new Cookie("pgv_pvid", "816636090") { Domain = "qun.qq.com" });
                    mycookiecontainer.Add(new Cookie("pgv_pvi", "9521545216") { Domain = "qun.qq.com" });
                    mycookiecontainer.Add(new Cookie("pgv_si", "s5320108032") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("RK", "jSpA3A+mUp") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("ptcz", "a2b57ca5a31032e46f16bea1389d5ee36a1188cb67bf24f26e7aa308955e5d1b") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("ts_uid", "8346773800") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("qq_locale_id", "2052") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("imei", "861305042712345") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("idt", "1601010784") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("qz_gdt", "4slh2xyiaaacwxqgry2q") { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("pt4_token", "mMaie70GRthuLhJUgr1CUqYhZifuUblBA1IVf2ylxTI_") { Domain = "qun.qq.com" });
					WebHeaderCollection myWebHeaderCollection = new WebHeaderCollection();

                    var redirect_geturl = string.Empty;
                    var head1 = new WebHeaderCollection()
                    {
                        {"sec-fetch-mode:no-cors"},
						{"sec-fetch-site:same-origin"},
						{"sec-fetch-user:?1"},
						{"if-none-match:525521a2-b2e"},						
						{"q-ua2:QV=3&PL=ADR&PR=QQ&PP=com.tencent.mobileqq&PPVN=8.4.8&TBSVC=43957&CO=BK&COVC=045410&PB=GE&VE=GA&DE=PHONE&CHID=0&LCID=9422&MO= MI9 &RL=1080*2135&OS=10&API=29"},
						{"q-guid:3ccf05a2bbd6d729e43aa79c13b788cb"},
						{"q-qimei:861305042743253"},
						{"q-auth:31045b957cf33acf31e40be2f3e71c5217597676a9729f1b"},
						{"Upgrade-Insecure-Requests:1"},
						{"x-requested-with:com.tencent.mobileqq"}
					 };
                    string url = "https://qun.qq.com/interactive/honorlist?gc=" + sMsg.MessageGroupQQ.ToString() + "&type=1&_wv=3&_wwv=129";
					Dictionary<string, string> Headerdics = new Dictionary<string, string>()  //
					{
						{"Accept", "image/webp,image/sharpp,image/apng,image/tpg,image/*,*/*;q=0.8"},
						{"ContentType", "application/json, text/plain, */*"},
						{"Referer", url},
						{"Host", "qun.qq.com"},
						{"UserAgent", "Mozilla/5.0 (Linux; Android 10; MI 9 Build/QKQ1.190825.002; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/77.0.3865.120 MQQBrowser/6.2 TBS/045410 Mobile Safari/537.36 V1_AND_SQ_8.4.8_1492_YYB_D QQ/8.4.8.4810 NetType/WIFI WebP/0.3.0 Pixel/1080 StatusBarHeight/75 SimpleUISwitch/0 QQTheme/1000 InMagicWin/0"}
					};

					var Res = HttpHelper.RequestGet("https://qun.qq.com/favicon.ico", Headerdics, head1, mycookiecontainer, ref redirect_geturl);
					if (Res != "")
                    {

                    };

					var head2 = new WebHeaderCollection()
					{
						{"sec-fetch-mode:navigate"},
						{"sec-fetch-site:none"},
						{"sec-fetch-user:?1"},
						{"Upgrade-Insecure-Requests:1"},
						{"q-ua2:QV=3&PL=ADR&PR=QQ&PP=com.tencent.mobileqq&PPVN=8.4.8&TBSVC=43957&CO=BK&COVC=045410&PB=GE&VE=GA&DE=PHONE&CHID=0&LCID=9422&MO= MI9 &RL=1080*2135&OS=10&API=29"},
						{"q-guid:3ccf05a2bbd6d729e43aa79c13b788cb"},
						{"q-qimei:861305042743253"},
						{"q-auth:31045b957cf33acf31e40be2f3e71c5217597676a9729f1b"},
						{"x-requested-with:com.tencent.mobileqq"}
					 };
						Dictionary<string, string> Headerdics2 = new Dictionary<string, string>()
					{
						{"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3"},
						{"ContentType", "application/json, text/plain, */*"},
						{"Host", "qun.qq.com"},
						{"UserAgent", "Mozilla/5.0 (Linux; Android 10; MI 9 Build/QKQ1.190825.002; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/77.0.3865.120 MQQBrowser/6.2 TBS/045410 Mobile Safari/537.36 V1_AND_SQ_8.4.8_1492_YYB_D QQ/8.4.8.4810 NetType/WIFI WebP/0.3.0 Pixel/1080 StatusBarHeight/75 SimpleUISwitch/0 QQTheme/1000 InMagicWin/0"}
					};
					Res = HttpHelper.RequestGet(url, Headerdics2, head2,mycookiecontainer, ref redirect_geturl);
                    if (Res != "")
                    {
                        MatchCollection match1 = Regex.Matches(Res, @"\{(?:[^\{\}]|(?<o>\{)|(?<-o>\}))+(?(o)(?!))\}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        foreach (Match match in match1)
                        {
                            try
                            {
                                dynamic token = new JavaScriptSerializer().DeserializeObject(match.Value);
                                if (token["gc"] != null && (string)token["gc"] == sMsg.MessageGroupQQ.ToString())
                                {
                                    dragon = token["talkativeList"][0]["uin"];
                                }

                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message.ToString());
                            }
                        }
                    }
                    if (dragon == string.Empty)
                    {
                        API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + Environment.NewLine + "没有龙王.", false);
                    }
                    else
                    {
                        API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + dragon + "]" + Environment.NewLine + "你是本群龙王.", false);
                    }
                }

			}
			return 0;
		}
		#endregion

	 static CookieContainer GetCookie(string clientkey,string jumpurl,string uin,string appid,string daid)
        {
			CookieContainer mycookiecontainer = new CookieContainer();
			WebHeaderCollection myWebHeaderCollection = new WebHeaderCollection();
			var redirect_geturl = string.Empty;
			var head1 = new WebHeaderCollection();
			Dictionary<string, string> Headerdics = new Dictionary<string, string>()
			{
			    {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3"},
			    {"ContentType", "application/json, text/plain, */*"},
			    {"UserAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4098.3 Safari/537.36"}
			};
			string url = "https://ui.ptlogin2.qq.com/cgi-bin/login?pt_hide_ad=1&style=9&appid=" + appid + "&pt_no_auth=1&pt_wxtest=1&daid= " + daid + " & s_url= " + HttpUtility.UrlDecode(jumpurl);
			var Res = HttpHelper.RequestGet(url, Headerdics, head1,  mycookiecontainer, ref redirect_geturl);
			url = "https://ssl.ptlogin2.qq.com/jump?u1=" + HttpUtility.UrlDecode(jumpurl) + "&pt_report=1&daid=" + daid + "&style=9&keyindex=19&clientuin=" + uin + "&clientkey=" + clientkey;
			Res = HttpHelper.RequestGet(url, Headerdics, head1,  mycookiecontainer, ref redirect_geturl);
			if (Res != "")
			{
				return mycookiecontainer;
			};
			return mycookiecontainer;
		}
	}

}
