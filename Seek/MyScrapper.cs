using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Seek
{
    public class MyScrapper
    {
        static HtmlNodeCollection GetNodes(HtmlDocument doc)
        {
            var xPaths = new List<string>
            {
                @"/html/body/pre/a",
                @"/html/body/table/tbody/tr/td/a",
                @"/html/body/main/div/div[2]/div[4]/table/tbody/tr/td/a",
                @"/html/body/div[1]/table/tbody/tr/td/a"
            };

            HtmlNodeCollection nodes = null;
            var index = 0;

            while (nodes == null)
            {
                if (index >= xPaths.Count)
                {
                    break;
                }

                nodes = doc.DocumentNode.SelectNodes(xPaths[index]);
                index++;
            }

            return nodes;
        }

        public static List<string> GetGoogleLinks(string URl, out bool done)
        {
                List<string> rtn = new List<string>();

            try
            {
                var web = new HtmlWeb();
                var xPath = @"//div[contains(concat(' ', normalize-space(@class), ' '), ' r ')]";

                var doc = web.LoadFromBrowser(URl);

                var nodes = doc.DocumentNode.SelectNodes(xPath);

                foreach (var item in nodes)
                {
                    var url = item.ChildNodes.FindFirst("a").Attributes["href"].Value;

                    if (url.StartsWith("http"))
                    {
                        try
                        {
                            doc = web.Load(url);
                            var Nodes = GetNodes(doc);

                            if (Nodes != null)
                            {
                                rtn.Add(url);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("An error occured. \n Please check your internet connection and try again.\n" + ex.Message);
            }
            finally
            {
                done = true;
            }
            return rtn;
        }

        public static List<string> Webscraper(string URL)
        {
            List<string> downloadLinks = new List<string>();
            List<string> folderLinks = new List<string>();

            var web = new HtmlWeb();
            folderLinks.Add(URL);

            var curIndex = 0;

            while (folderLinks.Count > curIndex)
            {
                var url = folderLinks[curIndex];

                var doc = web.Load(url);

                var Nodes = GetNodes(doc);
                if (Nodes == null)
                {

                }
                else
                {
                    foreach (var node in Nodes)
                    {
                        var href = node.Attributes["href"].Value;

                        bool? isfile = isFile(href);

                        if (isfile == null)
                        {

                        }
                        else if (isfile == true)
                        {
                            if (href.StartsWith("http")) { downloadLinks.Add(href); }
                            else { downloadLinks.Add(Path.Combine(url, href)); }
                        }
                        else
                        {
                            folderLinks.Add(Path.Combine(url, href));
                        }
                    }
                }
                curIndex++;
            }

            return downloadLinks;

        }

        private static bool? isFile(string href)
        {
            if (href.StartsWith(".."))
            {
                return null;
            }
            else
            {
                if (href.Contains("."))
                {
                    var extensionRange = href.ToLower().Substring(href.Length - 5);

                    return extensionRange.Contains(".");
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
