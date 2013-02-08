using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;

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
            if (!String.IsNullOrEmpty(address)) {
                wc.DownloadFile(address, filename);
            }
        }

        private static void ItemList(string url) {
            string header =
                GetPage(url).DocumentNode.SelectSingleNode("//div[@class='p-body-center']/h2").InnerText.Trim(' ', '\r',
                                                                                                              '\n');
            HtmlNode crumb = GetPage(url).DocumentNode.SelectSingleNode(@"//td[@class='crumbs']");
            if (crumb.ChildNodes.Count > 7) {
                header = String.Format(@"{0}\{1}", crumb.ChildNodes[7].InnerText.Trim(' ', '\r', '\n'), header);
            }
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
            var pager = GetPage(url).DocumentNode.SelectSingleNode(@"//ul[@class='pager']");
            if (pager != null) {
                foreach (var pagernode in pager.SelectNodes(@"li/a")) {
                    foreach (
                        HtmlNode node in
                            GetPage(pagernode.GetAttributeValue("href", "")).DocumentNode.SelectNodes(
                                "//ul[@class=\"itemslist2\"]")) {
                        if (node.SelectNodes("//li[@id]") != null) {
                            foreach (HtmlNode item in node.SelectNodes("//li[@id]")) {
                                HtmlNode image = item.SelectSingleNode("div/a/img[@class='pic']");
                                HtmlNode title = item.SelectSingleNode("div/div/div/a");
                                string code2 =
                                    ClearString(item.SelectSingleNode("div/div/div[@class='code']/code").InnerText);
                                string descr =
                                    ClearString(item.SelectSingleNode("div/div/div[@class='descr']").InnerText);
                                DirectoryInfo dir =
                                    Directory.CreateDirectory(String.Format(@"Catalog\{0}\{1}", header, code2));
                                string savedir = dir.FullName;
                                GetImage(image.GetAttributeValue("src", ""),
                                         String.Format(@"{0}\img-{1}.jpg", savedir, code2));
                                List<string> descriptionList = new List<string> {
                                                                                    title.GetAttributeValue("href", ""),
                                                                                    ClearString(title.InnerText),
                                                                                    "Код товара\n" + code2,
                                                                                    descr
                                                                                };
                                SaveItem(savedir, descriptionList);
                            }
                        }
                    }
                }
            }
        }

        private static void SaveItem(string savedir, List<string> text) {
            using (
                var streamWriter =
                    new StreamWriter(String.Format(@"{0}\desc-{1}.txt", savedir))
                ) {
                foreach (var item in text) {
                    streamWriter.WriteLine(item);
                }
            }
        }

        private static void MakeDirs(string dirname) {
            const string maindir = @"Каталог/";
            if (!Directory.Exists(maindir + dirname)) {
                Directory.CreateDirectory(maindir + dirname);
            }
        }

        private static void Category(string catalogurl) {
            Console.WriteLine("Парсим ссылки с главной");
            HtmlNodeCollection catalog =
                GetPage(catalogurl).DocumentNode.SelectNodes(@"//table[@class='catalogcategories']/tr");
            if (catalog != null) {
                foreach (var trNodes in catalog) {
                    Console.Write("+");
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
        }

        private static void SubCategory(string catalogurl) {
            HtmlNodeCollection catalog =
                GetPage(catalogurl).DocumentNode.SelectNodes(@"//table[@class='catalogcategories']/tr");
            if (catalog != null) {
                Console.WriteLine("Парсим ссылки подкатегорий");
                foreach (var trNodes in catalog) {
                    Console.Write("+");
                    var thNodes = trNodes.ChildNodes.Where(x => x.Name == "th").ToArray();
                    if (thNodes.Count() != 0) {
                        foreach (var aNode in thNodes) {
                            var links = aNode.ChildNodes.Where(x => x.Name == "a").ToArray();
                            foreach (var link in links) {
                                allLinks.Add(String.Format(@"{0}/{1}", catalogurl, link.GetAttributeValue("href", "")));
                            }
                        }
                    }
                }
                Console.WriteLine("Готово");
            }
        }

        private static List<string> allLinks = new List<string>();

        private static void Main(string[] args) {
            Stopwatch sq = new Stopwatch();
            sq.Start();
            const string maincatalog = @"http://www.acv-auto.com/catalog/";
            Category(maincatalog);
            var sublink = allLinks.ToArray();
            foreach (var link in sublink) {
                SubCategory(link);
            }
            if (allLinks.Count != 0) {
                Console.WriteLine("Парсим продукт");
                foreach (var link in allLinks) {
                    ItemList(link);
                    Console.WriteLine("+");
                }
            }
            sq.Stop();
            Console.WriteLine(sq.Elapsed.TotalSeconds);
        }
    }
}