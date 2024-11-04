using AngleSharp.Html.Parser;

namespace JmaAccCsvGetter
{
    internal class Program
    {

        public static readonly HttpClient client = new();
        public static readonly HtmlParser parser = new();

        static void Main(string[] args)
        {
            Console.WriteLine("URLを入力してください。");//https://www.data.jma.go.jp/eqev/data/kyoshin/jishin/2401011610_noto/index.html
            var url = Console.ReadLine();
            var response = client.GetStringAsync(url).Result;
            var document = parser.ParseDocument(response);
            var table = document.QuerySelector("table[summary='強震波形']");
            var rows = table.QuerySelectorAll("tr").Skip(2);

            var p = url.Split('/');
            var id = p[^2];
            var dir = "output\\" + id;
            Directory.CreateDirectory("output");
            Directory.CreateDirectory(dir);
            List<string> ignoreChoice = ["＊", "-*", "(注)"];
            List<string> ignoreList = [];
            foreach (var ignore in ignoreChoice)
            {
                if (ignore == "-*")
                {
                    if (ignoreList.Contains("＊"))
                        continue;
                    Console.WriteLine("＊ 付きのもののみにしますか？(y/n)");
                    if (Console.ReadLine() == "y")
                        ignoreList.Add("-*");
                }
                else
                {
                    Console.WriteLine(ignore + " 付きのものを除外しますか？(y/n)");
                    if (Console.ReadLine() == "y")
                        ignoreList.Add(ignore);
                }
            }
            foreach (var row in rows)
            {
                var cell = row.QuerySelectorAll("td");
                foreach (var ign in ignoreList)
                {
                    if (cell[0].TextContent.Contains(ign))
                        goto res;
                }
                if (ignoreList.Contains("-*") && !cell[0].TextContent.Contains('＊'))
                    goto res;
                var a = cell.Last().QuerySelector("a");
                if (a == null)
                    continue;
                var url2 = url.Replace("index.html", "") + a.GetAttribute("href");
                Console.WriteLine(url2);
                var r2 = client.GetByteArrayAsync(url2).Result;
                var p2 = url2.Split("/");
                File.WriteAllBytes(dir + "\\" + p2.Last(), r2);//こうじゃないと文字化けする
            res:
                Thread.Sleep(1000);
            }
        }
    }
}