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
					IntPtr SKey = API.GetSKey(PInvoke.plugin_key, sMsg.ThisQQ);
					IntPtr PSKey = API.GetPSKey(PInvoke.plugin_key, sMsg.ThisQQ, "qun.qq.com");
					CookieContainer mycookiecontainer = new CookieContainer();
					mycookiecontainer.Add(new Cookie("uin", sMsg.ThisQQ.ToString()) { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("skey", Marshal.PtrToStringAnsi(SKey)) { Domain = "qun.qq.com" });
					mycookiecontainer.Add(new Cookie("pskey", Marshal.PtrToStringAnsi(PSKey)) { Domain = "qun.qq.com" });
					WebHeaderCollection myWebHeaderCollection = new WebHeaderCollection();
					var redirect_geturl = string.Empty;
					var head1 = new WebHeaderCollection()
		            {
		              	{"Pragma: no-cache"},
		            	{"DNT:1"},
		               	{"Upgrade-Insecure-Requests:1"}
	             	};
					Dictionary<string, string> Headerdics = new Dictionary<string, string>()
                    {
						{"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3"},
	   			    	{"ContentType", "application/json, text/plain, */*"},
	   			    	{"Referer", "qun.qq.com"},
	   				    {"Host", "qun.qq.com"},
	   			    	{"UserAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4098.3 Safari/537.36"}
   				    };
					string url = "https://qun.qq.com/interactive/honorlist?gc=" + sMsg.MessageGroupQQ.ToString()+"&type=1";
					var Res = HttpHelper.RequestGet(url, Headerdics, head1,ref mycookiecontainer,ref redirect_geturl);
					if (Res != "")
                    {
                        MatchCollection match1 = Regex.Matches(Res, @"\{(?:[^\{\}]|(?<o>\{)|(?<-o>\}))+(?(o)(?!))\}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        foreach (Match match in match1)
                        {
                            try
                            {
								dynamic token  = new JavaScriptSerializer().DeserializeObject(match.Value);
                                if (token["gc"] != null  && (string)token["gc"]== sMsg.MessageGroupQQ.ToString())
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
					if (dragon ==string.Empty)
                    {
						API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + Environment.NewLine + "没有龙王.", false);
					}
					else
                    {
						API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + dragon + "]" + Environment.NewLine+ "你是本群龙王.", false);
					}
				}

			}
			return 0;
		}
		#endregion
	}

}
