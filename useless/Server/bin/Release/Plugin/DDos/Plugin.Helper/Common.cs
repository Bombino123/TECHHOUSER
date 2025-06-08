using System.Text;
using System.Threading;
using Plugin.Methods;

namespace Plugin.Helper;

internal class Common
{
	public static int[] ports = new int[21]
	{
		21, 22, 23, 25, 53, 80, 110, 115, 143, 161,
		179, 220, 389, 443, 993, 3306, 3389, 8000, 8008, 8080,
		8081
	};

	public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

	public static string[] UserAgentsList = new string[73]
	{
		"Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0", "Mozilla/5.0 (iPhone; CPU iPhone OS 11_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.0 Mobile/15E148 Safari/604.1", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36", "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:106.0) Gecko/20100101 Firefox/106.0", "Mozilla/5.0 (Macintosh; Intel Mac OS X 13.0; rv:106.0) Gecko/20100101 Firefox/106.0",
		"Mozilla/5.0 (X11; Linux i686; rv:106.0) Gecko/20100101 Firefox/106.0", "Mozilla/5.0 (X11; Linux x86_64; rv:106.0) Gecko/20100101 Firefox/106.0", "Mozilla/5.0 (X11; Ubuntu; Linux i686; rv:106.0) Gecko/20100101 Firefox/106.0", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:106.0) Gecko/20100101 Firefox/106.0", "Mozilla/5.0 (X11; Fedora; Linux x86_64; rv:106.0) Gecko/20100101 Firefox/106.0", "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_0) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Safari/605.1.15", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0)", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)", "Mozilla/4.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)",
		"Mozilla/4.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko", "Mozilla/5.0 (Windows NT 6.2; Trident/7.0; rv:11.0) like Gecko", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko", "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/106.0.1370.52", "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/106.0.1370.52", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Vivaldi/5.5.2805.38",
		"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Vivaldi/5.5.2805.38", "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Vivaldi/5.5.2805.38", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Vivaldi/5.5.2805.38", "Mozilla/5.0 (X11; Linux i686) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Vivaldi/5.5.2805.38", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 OPR/92.0.4561.21", "Mozilla/5.0 (Windows NT 10.0; WOW64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 OPR/92.0.4561.21", "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 OPR/92.0.4561.21", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 OPR/92.0.4561.21", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 YaBrowser/22.9.1 Yowser/2.5 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 YaBrowser/22.9.1 Yowser/2.5 Safari/537.36",
		"Mozilla/5.0 (Macintosh; Intel Mac OS X 13_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 YaBrowser/22.9.1 Yowser/2.5 Safari/537.36", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/107.0.5304.66 Mobile/15E148 Safari/604.1", "Mozilla/5.0 (iPad; CPU OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/107.0.5304.66 Mobile/15E148 Safari/604.1", "Mozilla/5.0 (iPod; CPU iPhone OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/107.0.5304.66 Mobile/15E148 Safari/604.1", "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36", "Mozilla/5.0 (Linux; Android 10; SM-A205U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36", "Mozilla/5.0 (Linux; Android 10; SM-A102U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36", "Mozilla/5.0 (Linux; Android 10; SM-G960U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36", "Mozilla/5.0 (Linux; Android 10; SM-N960U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36", "Mozilla/5.0 (Linux; Android 10; LM-Q720) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36",
		"Mozilla/5.0 (Linux; Android 10; LM-X420) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36", "Mozilla/5.0 (iPhone; CPU iPhone OS 13_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/106.0 Mobile/15E148 Safari/605.1.15", "Mozilla/5.0 (iPad; CPU OS 13_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/106.0 Mobile/15E148 Safari/605.1.15", "Mozilla/5.0 (iPod touch; CPU iPhone OS 13_0 like Mac OS X) AppleWebKit/604.5.6 (KHTML, like Gecko) FxiOS/106.0 Mobile/15E148 Safari/605.1.15", "Mozilla/5.0 (Linux; Android 10; LM-Q710(FGN)) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36", "Mozilla/5.0 (Android 13; Mobile; rv:68.0) Gecko/68.0 Firefox/106.0", "Mozilla/5.0 (Android 13; Mobile; LG-M255; rv:106.0) Gecko/106.0 Firefox/106.0", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Mobile/15E148 Safari/604.1", "Mozilla/5.0 (iPad; CPU OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Mobile/15E148 Safari/604.1", "Mozilla/5.0 (iPod touch; CPU iPhone 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Mobile/15E148 Safari/604.1",
		"Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36 EdgA/106.0.1370.47", "Mozilla/5.0 (Linux; Android 10; SM-G973F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36 EdgA/106.0.1370.47", "Mozilla/5.0 (Linux; Android 10; Pixel 3 XL) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36 EdgA/106.0.1370.47", "Mozilla/5.0 (Linux; Android 10; ONEPLUS A6003) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36 EdgA/106.0.1370.47", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 EdgiOS/106.1370.52 Mobile/15E148 Safari/605.1.15", "Mozilla/5.0 (Windows Mobile 10; Android 10.0; Microsoft; Lumia 950XL) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Mobile Safari/537.36 Edge/40.15254.603", "Mozilla/5.0 (Linux; Android 10; VOG-L29) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36 OPR/63.3.3216.58675", "Mozilla/5.0 (Linux; Android 10; SM-G970F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36 OPR/63.3.3216.58675", "Mozilla/5.0 (Linux; Android 10; SM-N975F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 Mobile Safari/537.36 OPR/63.3.3216.58675", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 YaBrowser/22.9.7.126 Mobile/15E148 Safari/604.1",
		"Mozilla/5.0 (iPad; CPU OS 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 YaBrowser/22.9.7.126 Mobile/15E148 Safari/605.1", "Mozilla/5.0 (iPod touch; CPU iPhone 16_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 YaBrowser/22.9.7.126 Mobile/15E148 Safari/605.1", "Mozilla/5.0 (Linux; arm_64; Android 13; SM-G965F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.54 YaBrowser/21.3.4.59 Mobile Safari/537.36"
	};

	public static string[] subexiteons = new string[12]
	{
		"php", "html", "txt", "png", "zip", "jpg", "mp3", "mp4", "docs", "docx",
		"doc", "pdf"
	};

	public static string[] subhtml = new string[21]
	{
		"index", "login", "register", "signup", "loginin", "ru", "en", "uk", "article", "basic",
		"docs", "resource", "javascript", "css", "js", "forum", "course", "student", "user", "client",
		"robots"
	};

	public static string[] HeaderReferers = new string[74]
	{
		"http://www.google.com/?q=", "http://www.usatoday.com/search/results?q=", "http://engadget.search.aol.com/search?q=", "http://www.google.com/?q=", "http://www.usatoday.com/search/results?q=", "http://engadget.search.aol.com/search?q=", "http://www.bing.com/search?q=", "http://search.yahoo.com/search?p=", "http://www.ask.com/web?q=", "http://search.lycos.com/web/?q=",
		"http://busca.uol.com.br/web/?q=", "http://us.yhs4.search.yahoo.com/yhs/search?p=", "http://www.dmoz.org/search/search?q=", "http://www.baidu.com.br/s?usm=1&rn=100&wd=", "http://yandex.ru/yandsearch?text=", "http://www.zhongsou.com/third?w=", "http://hksearch.timway.com/search.php?query=", "http://find.ezilon.com/search.php?q=", "http://www.sogou.com/web?query=", "http://api.duckduckgo.com/html/?q=",
		"http://boorow.com/Pages/site_br_aspx?query=", "http://validator.w3.org/check?uri=", "http://validator.w3.org/checklink?uri=", "http://validator.w3.org/unicorn/check?ucn_task=conformance&ucn_uri=", "http://validator.w3.org/nu/?doc=", "http://validator.w3.org/mobile/check?docAddr=", "http://validator.w3.org/p3p/20020128/p3p.pl?uri=", "http://www.icap2014.com/cms/sites/all/modules/ckeditor_link/proxy.php?url=", "http://www.rssboard.org/rss-validator/check.cgi?url=", "http://www2.ogs.state.ny.us/help/urlstatusgo.html?url=",
		"http://prodvigator.bg/redirect.php?url=", "http://validator.w3.org/feed/check.cgi?url=", "http://www.ccm.edu/redirect/goto.asp?myURL=", "http://forum.buffed.de/redirect.php?url=", "http://rissa.kommune.no/engine/redirect.php?url=", "http://www.sadsong.net/redirect.php?url=", "https://www.fvsbank.com/redirect.php?url=", "http://www.jerrywho.de/?s=/redirect.php?url=", "http://www.inow.co.nz/redirect.php?url=", "http://www.automation-drive.com/redirect.php?url=",
		"http://mytinyfile.com/redirect.php?url=", "http://ruforum.mt5.com/redirect.php?url=", "http://www.websiteperformance.info/redirect.php?url=", "http://www.airberlin.com/site/redirect.php?url=", "http://www.rpz-ekhn.de/mail2date/ServiceCenter/redirect.php?url=", "http://evoec.com/review/redirect.php?url=", "http://www.crystalxp.net/redirect.php?url=", "http://watchmovies.cba.pl/articles/includes/redirect.php?url=", "http://www.seowizard.ir/redirect.php?url=", "http://apke.ru/redirect.php?url=",
		"http://seodrum.com/redirect.php?url=", "http://redrool.com/redirect.php?url=", "http://blog.eduzones.com/redirect.php?url=", "http://www.onlineseoreportcard.com/redirect.php?url=", "http://www.wickedfire.com/redirect.php?url=", "http://searchtoday.info/redirect.php?url=", "http://www.bobsoccer.ru/redirect.php?url=", "http://newsdiffs.org/article-history/iowaairs.org/redirect.php?url=", "http://seo.qalebfa.ir/%D8%B3%D8%A6%D9%88%DA%A9%D8%A7%D8%B1/redirect.php?url=", "http://www.firmia.cz/redirect.php?url=",
		"http://www.e39-forum.de/redir.php?url=", "http://www.wopus.org/wp-content/themes/begin/inc/go.php?url=", "http://www.selectsmart.com/plus/select.php?url=", "http://www.taichinh2a.com/forum/links.php?url=", "http://facenama.com/go.php?url=", "http://www.internet-abc.de/eltern/118732.php?url=", "http://g.makebd.com/index.php?url=", "https://blog.eduzones.com/redirect.php?url=", "http://www.mientay24h.vn/redirector.php?url=", "http://www.kapook.com/webout.php?url=",
		"http://lue4.ddns.name/pk/index.php?url=", "http://747.ddns.ms/pk/index.php?url=", "http://737.ddns.us/pk/index.php?url=", "http://a30.m1.4irc.com/pk/index.php?url="
	};

	public static Encoding[] Encodings = new Encoding[8]
	{
		Encoding.ASCII,
		Encoding.UTF8,
		Encoding.UTF32,
		Encoding.UTF7,
		Encoding.Unicode,
		Encoding.Unicode,
		Encoding.BigEndianUnicode,
		Encoding.Default
	};

	public static Method[] Methods = new Method[13]
	{
		new IcmpFlood(),
		new IPv4Flood(),
		new IPv6Flood(),
		new NullHttp(),
		new PostScanner(),
		new PpsFlood(),
		new GetScanner(),
		new GetFlood(),
		new SlowLoris(),
		new TcpConnectFlood(),
		new TcpConnectWaitFlood(),
		new TcpFlood(),
		new UdpFlood()
	};
}
