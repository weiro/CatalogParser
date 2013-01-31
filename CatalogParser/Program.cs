using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace CatalogParser {
    internal class Program {
        private static string ClearString(string str)
        {
            str = str.Trim(' ', '\n', 'r');
            return HttpUtility.HtmlDecode(str);
        }

        private static HtmlDocument GetPage (string url) {
            WebClient wc = new WebClient();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(wc.DownloadString(url));
            return htmlDocument;
        }
        //TODO: Привести в порядок xpath
        private static void Subcategory(string url, string xpath) {
            var doc = GetPage(url).DocumentNode.SelectNodes(xpath);
            var h = GetPage(url).DocumentNode.SelectSingleNode("/html/body/div/div[3]/div/div[2]/h2");
            string subname = h.InnerText;
            foreach (var tbody in doc) {
                foreach (var tr in tbody.ChildNodes) {
                    foreach (var th in tr.ChildNodes) {
                        foreach (var a in th.ChildNodes) {
                            string line = a.GetAttributeValue("href", "");
                            if ((!String.IsNullOrEmpty(line))) {
                                MakeDirs(String.Format("{0}/{1}", subname, a.InnerText));
                                ItemList(url+line);
                                Console.WriteLine(a.InnerText);
                            }
                        }
                    }
                }
            }
        }
        private static void GetImage(string address, string filename) {
            WebClient wc = new WebClient();
            wc.DownloadFile(address, filename);
        }
        private static void ItemList(string url) {                  
            string xpath = @"/html/body/div/div[3]/div/div[2]/ul";
            var doc = GetPage(url).DocumentNode.SelectNodes(xpath);
            foreach (var li in doc) {
                foreach (var divitem in li.ChildNodes) {
                    foreach (var a in divitem.ChildNodes) {
                        foreach (var childNode in a.ChildNodes) {
                            if (childNode.HasAttributes) {
                                string l = childNode.GetAttributeValue("href", "");

                            }
                           
                            foreach (var node in childNode.ChildNodes) {
                            }
                           
                        }
                        
                    }
                }
            }

        }

        private static void MakeDirs(string dirname) {
            const string maindir = @"Каталог/";
            if (!Directory.Exists(maindir + dirname)) {
                Directory.CreateDirectory(maindir + dirname);
            }
        }

        private static void Main(string[] args) {
            string maincatalog = @"http://www.acv-auto.com/catalog/";
            var qwe = GetPage(maincatalog).DocumentNode.SelectNodes(@"/html/body/div/div[3]/div/div[2]/table[2]");
            List<string> links = new List<string>(); //ссылки на категории с главной страницы каталога
            List<string> categoryName = new List<string>(); //имена категорий с главной
            foreach (HtmlNode s in qwe) {
                foreach (var table in s.ChildNodes) {
                    foreach (var tr in table.ChildNodes) {
                        foreach (var a in tr.ChildNodes) {
                            string line = a.GetAttributeValue("href", "");
                            if ((!String.IsNullOrEmpty(line))) {
                                MakeDirs(a.InnerText);
                                links.Add(line);
                            }
                        }
                    }
                }
            }

            const string xpath = @"/html/body/div/div[3]/div/div[2]/table[2]";
            foreach (var link in links) {
                Subcategory(link, xpath);
                //thread.Start();
            }
        }
    }
}