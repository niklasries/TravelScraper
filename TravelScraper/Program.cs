using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.IO;
using System.Threading;

namespace TravelScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Program test = new Program();

            List<string> links = new List<string>();
            List<string> names = new List<string>();


            HtmlDocument htmlDoc = new HtmlDocument();
            //TODO: expose the url to the user as input
            string website = "terratraveller.net/";
            string url = "http://" + website;
            string urlResponse = URLRequest(url);

            htmlDoc.LoadHtml(urlResponse);

            //Find all A tags in the document for hyperlinks
            var anchorNodes = htmlDoc.DocumentNode.SelectNodes("//a");

            if (anchorNodes != null)
            {
                Console.WriteLine(String.Format("We found {0} anchor tags on this page. Here is the text from those tags:", anchorNodes.Count));

                foreach (var anchorNode in anchorNodes)
                {

                    if (!anchorNode.GetAttributeValue("href", "").Contains("http"))
                    {
                        links.Add(url + anchorNode.GetAttributeValue("href", ""));
                        names.Add(anchorNode.GetAttributeValue("href", "").Split('.')[0]);
                        //Console.WriteLine(anchorNode.GetAttributeValue("href", ""));
                    }

                }
            }

            //create folder structure for images

            //create main folder on desktop named after the website to crawl
            string folderName = website.Split('.')[0];
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), folderName);
            Console.WriteLine("creating folder: " + path);
            //path= System.IO.Path.Combine(path,"test");
            test.CreateFolder(path);

            //Directory.SetCurrentDirectory(path);

            WebClient x = new WebClient();
            //TODO: add multi threading for download
            //x.DownloadFile(url,)
            int i = 0;
            foreach (var link in links)
            {
                Directory.SetCurrentDirectory(path);
                Console.WriteLine(link);
                string path1 = System.IO.Path.Combine(path, names[i]);
                test.CreateFolder(path1);
                Directory.SetCurrentDirectory(path1);

                List<string> ImageList = new List<string>();
                string source = x.DownloadString(link);
                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(source);

                var ImageURLs = document.DocumentNode.Descendants("img")
                    .Select(e => e.GetAttributeValue("src", null))
                    .Where(s => !String.IsNullOrEmpty(s));

                foreach (var item in ImageURLs)
                {
                    if (item != null && !item.Contains("http") && !File.Exists(item.ToString().Split('/').Last()))
                    {
                        using (WebClient client = new WebClient())
                        { 
                            try
                            {
                            client.DownloadFileAsync(new Uri(url + item), item.ToString().Split('/').Last());
                            }
                            catch
                            {
                            //Console.WriteLine("didnt download");
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("File: " + item.ToString().Split('/').Last() + " already exists, skipping...");

                    }
                    Thread.Sleep(15);
                }
                i++;

            }

        }

        public void CreateFolder(string path)
        {

            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    Console.WriteLine("That path exists already.");
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }

            return;
        }

        //General Function to request data from a Server
        static string URLRequest(string url)
        {
            // Prepare the Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // Set method to GET to retrieve data
            request.Method = "GET";
            request.Timeout = 6000; //60 second timeout
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)";

            string responseContent = null;

            // Get the Response
            using (WebResponse response = request.GetResponse())
            {
                // Retrieve a handle to the Stream
                using (Stream stream = response.GetResponseStream())
                {
                    // Begin reading the Stream
                    using (StreamReader streamreader = new StreamReader(stream))
                    {
                        // Read the Response Stream to the end
                        responseContent = streamreader.ReadToEnd();
                    }
                }
            }

            return (responseContent);
        }
    }
}
