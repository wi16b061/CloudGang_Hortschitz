using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CloudGang_Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using CloudGangClient;

namespace CloudGang_Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private readonly IHostingEnvironment he;

        public HomeController(IHostingEnvironment e)
        {
            he = e;
        }

        public async Task<IActionResult> ShowFieldsAsync(IFormFile pic)
        {
            
            if (pic != null)
            {
                var fileName = Path.Combine(he.WebRootPath, Path.GetFileName(pic.FileName));
               // FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
                pic.CopyTo(new FileStream(fileName, FileMode.Create));
               // pic.
                ViewData["fileLocation"] = "/" + Path.GetFileName(pic.FileName);
                

                //TODO: ENTER your visionApiKey from Microsoft Azure
                string visionApiKey = "";

                string visionApiEndPoint = "https://northeurope.api.cognitive.microsoft.com/vision/v2.0";
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", visionApiKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "visualFeatures=Categories,Description,Color&language=en";

                // Assemble the URI for the REST API Call.
                string uri = visionApiEndPoint + "/analyze" + "?" + requestParameters;

                HttpResponseMessage response;

                // Request body. Posts an image you've added to your site's images folder. 
                var fileInfo = he.WebRootFileProvider.GetFileInfo("/" + Path.GetFileName(pic.FileName));
                byte[] byteData = GetImageAsByteArray(fileInfo.PhysicalPath, pic.OpenReadStream());

                string contentString = string.Empty;
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // Execute the REST API call.
                    response = client.PostAsync(uri, content).Result;

                    // Get the JSON response.
                    contentString = response.Content.ReadAsStringAsync().Result;
                }

                JObject json = JObject.Parse(contentString);
                ViewData["azureResult"] = contentString;

                
            }
            return View();
        }

            /// <summary>
            /// Returns the contents of the specified file as a byte array.
            /// </summary>
            /// <param name="imageFilePath">The image file to read.</param>
            /// <returns>The byte array of the image data.</returns>
            static byte[] GetImageAsByteArray(string imageFilePath, Stream stream)
            {
               // FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
               // fileStream.
            BinaryReader binaryReader = new BinaryReader(stream);
                return binaryReader.ReadBytes((int)stream.Length);
            }

            /// <summary>
            /// Formats the given JSON string by adding line breaks and indents.
            /// </summary>
            /// <param name="json">The raw JSON string to format.</param>
            /// <returns>The formatted JSON string.</returns>
            static string JsonPrettyPrint(string json)
            {
                if (string.IsNullOrEmpty(json))
                    return string.Empty;

                json = json.Replace(Environment.NewLine, "").Replace("\t", "");

                string INDENT_STRING = "    ";
                var indent = 0;
                var quoted = false;
                var sb = new StringBuilder();
                for (var i = 0; i < json.Length; i++)
                {
                    var ch = json[i];
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            if (!quoted)
                            {
                                sb.AppendLine();
                            }
                            break;
                        case '}':
                        case ']':
                            if (!quoted)
                            {
                                sb.AppendLine();
                            }
                            sb.Append(ch);
                            break;
                        case '"':
                            sb.Append(ch);
                            bool escaped = false;
                            var index = i;
                            while (index > 0 && json[--index] == '\\')
                                escaped = !escaped;
                            if (!escaped)
                                quoted = !quoted;
                            break;
                        case ',':
                            sb.Append(ch);
                            if (!quoted)
                            {
                                sb.AppendLine();
                            }
                            break;
                        case ':':
                            sb.Append(ch);
                            if (!quoted)
                                sb.Append(" ");
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }
                }
                return sb.ToString();
            }
        }
}
