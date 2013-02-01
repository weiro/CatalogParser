using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using System.Linq;
using System.Linq.Expressions;

namespace CatalogParser {
    internal class Program {
        private static string ClearString(string str) {
            str = str.Trim(' ', '\n', 'r');
            return HttpUtility.HtmlDecode(str);
        }

        private static HtmlDocument GetPage(string url) {
            var wc = new WebClient();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(wc.DownloadString(url));
            return htmlDocument;
        }

        private static void GetImage(string address, string filename) {
            var wc = new WebClient();
            wc.DownloadFile(address, filename);
        }

        private static void ItemList(string url) {
            string header =
                GetPage(url).DocumentNode.SelectSingleNode("//div[@class='p-body-center']/h2").InnerText.Trim(' ', '\r',
                                                                                                              '\n');
            foreach (HtmlNode node in GetPage(url).DocumentNode.SelectNodes("//ul[@class=\"itemslist2\"]")) {
                if (node.SelectNodes("//li[@id]") != null) {
                    foreach (HtmlNode item in node.SelectNodes("//li[@id]")) {
                        HtmlNode image = item.SelectSingleNode("div/a/img[@class='pic']");
                        HtmlNode title = item.SelectSingleNode("div/div/div/a");
                        string code = ClearString(item.SelectSingleNode("div/div/div[@class='code']").InnerText);
                        string code2 = ClearString(item.SelectSingleNode("div/div/div[@class='code']/code").InnerText);
                        string descr = ClearString(item.SelectSingleNode("div/div/div[@class='descr']").InnerText);
                        DirectoryInfo dir = Directory.CreateDirectory(String.Format(@"Catalog\{0}\{1}", header, code2));
                        string savedir = dir.FullName;
                        GetImage(image.GetAttributeValue("src", ""),
                                 String.Format(@"{0}\img-{1}.jpg", savedir, code2));
                        SaveItem(savedir, code2, title, code, descr);
                    }
                }
            }
        }

        private static void SaveItem(string savedir, string code2, HtmlNode title, string code, string descr) {
            using (
                var streamWriter =
                    new StreamWriter(String.Format(@"{0}\desc-{1}.txt", savedir, code2))
                ) {
                streamWriter.WriteLine("link");
                streamWriter.WriteLine(title.GetAttributeValue("href", ""));
                streamWriter.WriteLine("title");
                streamWriter.WriteLine(ClearString(title.InnerText));
                streamWriter.WriteLine("code");
                streamWriter.WriteLine(code);
                streamWriter.WriteLine("description");
                streamWriter.WriteLine(descr);
            }
        }

        private static void MakeDirs(string dirname) {
            const string maindir = @"Каталог/";
            if (!Directory.Exists(maindir + dirname)) {
                Directory.CreateDirectory(maindir + dirname);
            }
        }

        private static void Category(string catalogurl) {
            HtmlNodeCollection catalog =
                GetPage(catalogurl).DocumentNode.SelectNodes(@"//table[@class='catalogcategories']/tr");
            foreach (var trNodes in catalog) {
                var thNodes = trNodes.ChildNodes.Where(x => x.Name == "th").ToArray();
                if (thNodes.Count() != 0) {
                    foreach (var aNode in thNodes) {
                        var links = aNode.ChildNodes.Where(x => x.Name == "a").ToArray();
                        foreach (var link in links) {
                            allLinks.Add(link.GetAttributeValue("href", ""));
                        }
                    }
                }
            }
        }

        private static List<string> allLinks = new List<string>();

        private static void Main(string[] args) {
            string maincatalog = @"http://www.acv-auto.com/catalog/";
            Category(maincatalog);

            if (allLinks.Count != 0) {
                foreach (var link in allLinks) {
                    Thread thread = new Thread(() => ItemList(link));
                    thread.Start();
                }
            }
        }
    }
}