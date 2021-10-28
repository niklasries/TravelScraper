using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.IO;
using System.Text;

namespace TravelScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Program test = new Program();

            
            List<string> url = new List<string>();
            List<string> websites = new List<string>();

            

            if (File.Exists("websites.txt"))
            {
                int counter = 0;
                foreach (string line in System.IO.File.ReadLines("websites.txt"))
                {
                    if (line.StartsWith("#"))
                    {
                        //commented lines will be skipped
                    }
                    else
                    {
                        System.Console.WriteLine(line);
                        websites.Add(line);
                        counter++;
                    }

                    
                }

                System.Console.WriteLine("{0} Website(s) detected. Press any button to continue", counter);
                // Suspend the screen.  
                System.Console.ReadLine();
            }

            else
            {
                //fallback to only terratraveller
                using (FileStream fs = File.Create("websites.txt"))
                {
                    // Add some text to file
                    Byte[] title1 = new UTF8Encoding(true).GetBytes("#add websites like this : google.com/\n#each use a new line for each website. The # at the start of a line makes it a comment\n");
                    fs.Write(title1, 0, title1.Length);
                    Byte[] title = new UTF8Encoding(true).GetBytes("terratraveller.net/\n");
                    fs.Write(title, 0, title.Length);
                }

                System.Console.WriteLine("No website list detected. File: websites.txt missing! Press any button to contine scraping terratraveller as fallback or close the program");
                System.Console.ReadLine();
            }
           
            foreach (var website in websites)
            {
                url.Add("http://" + website);
            }

            int websiteCounter = 0;

            foreach (var u in url)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                List<string> links = new List<string>();
                List<string> names = new List<string>();
                List<string> downloaded = new List<string>();

                string urlResponse = URLRequest(u);

                htmlDoc.LoadHtml(urlResponse);

                names.Add(websites[websiteCounter]);
                //Find all A tags in the document for hyperlinks
                var anchorNodes = htmlDoc.DocumentNode.SelectNodes("//a");

                if (anchorNodes != null)
                {
                    Console.WriteLine(String.Format("\nWe found {0} anchor tags on this page. Here is the text from those tags:", anchorNodes.Count));

                    foreach (var anchorNode in anchorNodes)
                    {
                        //Console.WriteLine(anchorNode.GetAttributeValue("href", ""));
                        if (!links.Contains(anchorNode.GetAttributeValue("href", "").Contains("http") ? anchorNode.GetAttributeValue("href", "") : u + anchorNode.GetAttributeValue("href", ""))&& !(u + anchorNode.GetAttributeValue("href", "")).Contains("mailto"))
                        {
                            if (!anchorNode.GetAttributeValue("href", "").Contains("http")|| anchorNode.GetAttributeValue("href", "").Contains(websites[websiteCounter]))
                            {

                                links.Add(anchorNode.GetAttributeValue("href", "").Contains("http") ? anchorNode.GetAttributeValue("href", "") : u + anchorNode.GetAttributeValue("href", ""));
                                names.Add(anchorNode.GetAttributeValue("href", "").Replace("http://","").Replace(".php",""));
                                //Console.WriteLine(names.Last()+"-name added");
                            }
                            
                        }

                    }
                }

              

                int page=0;
                int pageMin = 0;
                int pageMax = 0;
                string start="";
                foreach (var l in links)
                {
                    if (l.Contains("page"))
                    {
                       var words = l.Split('/');
                       int.TryParse(words[words.Length-2],out page);
                        //Console.WriteLine(page);

                        if (page >0)
                        {

                            
                            pageMin = page;

                            if (pageMax < pageMin)
                            {
                                page = pageMax;
                                pageMax = pageMin;
                                pageMin = page;
                                start = l;
                                                           
                            }
                            
                        }
                        
                       
                    }
                    //Console.WriteLine(l);


                }

                if (start.Length != 0)
                {
                    for (int k = pageMin+1; k < pageMax;k++)
                    {
                        links.Add(start.Replace(pageMax.ToString(), (k).ToString()));
                        names.Add(start.Replace(pageMax.ToString(), (k).ToString()).Replace("http://",""));
                        //Console.WriteLine(names.Last());
                    }
                }
                Console.WriteLine("\npress any button to continue...");
                Console.ReadLine();

                //create folder structure for images

                //create main folder on desktop named after the website to crawl
                string folderName = websites[websiteCounter].Split('.')[0];
                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), folderName);
                //Console.WriteLine("creating folder: " + websites[websiteCounter]);
                                
                test.CreateFolder(path);

               WebClient x = new WebClient();
                //TODO: add multi threading for download
                int i = 0;
                foreach (var link in links)
                {
                    Directory.SetCurrentDirectory(path);
                    
                    string path1 = System.IO.Path.Combine(path, names[i]);
                    if (!path1.StartsWith("C"))
                    {
                        path1 = path + path1;
                    }
                    test.CreateFolder(path1);
                    Directory.SetCurrentDirectory(path1);
                    //Console.WriteLine(path);

                    List<string> ImageList = new List<string>();
                    Console.WriteLine(link);
                    string source = x.DownloadString(link);
                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(source);
                    
                    var ImageURLs = document.DocumentNode.Descendants("img")
                        .Select(e => e.GetAttributeValue("src", null))
                        .Where(s => !String.IsNullOrEmpty(s));

                    foreach (var item in ImageURLs)
                    {
                        if (item != null && !item.Contains("http") && !File.Exists(item.ToString().Split('/').Last())&&!downloaded.Contains(item.ToString().Split('/').Last()))
                        {
                            
                            {
                                using (WebClient client = new WebClient())
                                    try
                                {
                                    client.DownloadFileAsync(new Uri(link.Replace(link.Split('/').Last(), "") + item), item.ToString().Split('/').Last());
                                        downloaded.Add(item.ToString().Split('/').Last());
                                    Console.WriteLine("downloading: "+ new Uri(link.Replace(link.Split('/').Last(),"")+item));
                                }
                                catch
                                {
                                    Console.WriteLine("didnt download; error with: "+ item);
                                    
                                }
                            }

                        }
                        else
                        {
                            using (WebClient client = new WebClient())
                                if (item.Contains("http") && !File.Exists(item.ToString().Split('/').Last())&& item!=null && !downloaded.Contains(item.ToString().Split('/').Last()))
                            {
                                Console.WriteLine("fallback http donwload: "+ item.ToString().Split('/').Last());
                                client.DownloadFileAsync(new Uri(item), item.ToString().Split('/').Last());
                                downloaded.Add(item.ToString().Split('/').Last());
                                }
                            else
                                { 

                            //Console.WriteLine(Directory.GetCurrentDirectory());
                            Console.WriteLine("File: " + item.ToString().Split('/').Last() + " already exists, skipping...");
                                }
                        }
                        //Thread.Sleep(15);
                    }
                    i++;

                }
                websiteCounter++;
            }

            Console.WriteLine("\ndownload fisnished. Press anything to continue...");
            System.Console.ReadLine();
        }

        public void CreateFolder(string path)
        {
            

            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    //Console.WriteLine("That path exists already." + path);
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
