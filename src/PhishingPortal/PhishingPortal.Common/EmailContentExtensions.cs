using EASendMail;
using HtmlAgilityPack;
using System.Net.Mail;
using System.Net.Mime;

namespace PhishingPortal.Common
{
    public static class EmailContentExtensions
    {
        public static AlternateView ToMultipartMailBody(this string content, string imageRoot)
        {
            var linkedResources = new List<LinkedResource>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            foreach (var childNode in htmlDoc.DocumentNode.Descendants(2))
            {
                if (childNode.Name != "img")
                    continue;

                var src = childNode.Attributes["src"].Value;

                var alt = childNode.Attributes["alt"]?.Value;

                var p = Path.Combine(imageRoot, src);

                var ext = Path.GetExtension(src).Replace("."," ").Trim();

                var img = new LinkedResource(p, $"image/{ext}");
                img.ContentId = Guid.NewGuid().ToString();
                
                linkedResources.Add(img);
                childNode.SetAttributeValue("src", $"cid:{img.ContentId}");
            }

            var htmlContent = htmlDoc.DocumentNode.WriteTo();
            AlternateView AV = AlternateView.CreateAlternateViewFromString(htmlContent, System.Text.Encoding.UTF8, MediaTypeNames.Text.Html);
            foreach (var linkedResource in linkedResources)
            {
                AV.LinkedResources.Add(linkedResource);
            }

            return AV;

        }

        public static Tuple<MimePartCollection, string> ToMimePartCollection(this string content, string imageRoot)
        {
            var linkedResources = new List<LinkedResource>();
            var mimeParts = new MimePartCollection();

            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(content);

            foreach (var childNode in htmlDoc.DocumentNode.Descendants(2))
            {
                if (childNode.Name != "img")
                    continue;

                var src = childNode.Attributes["src"].Value;

                var alt = childNode.Attributes["alt"]?.Value;

                var p = Path.Combine(imageRoot, src);

                var ext = Path.GetExtension(src).Replace(".", " ").Trim();

                // linked resources
                var img = new LinkedResource(p, $"image/{ext}");
                img.ContentId = Guid.NewGuid().ToString().ToLower();

                // mime part
                var mimePart = new MimePart();
                mimePart.UnencodeContent = File.ReadAllBytes(p);
                mimePart.ContentID = img.ContentId;
                mimePart.Headers.Insert(0, new HeaderItem($"Content-Type: image/{ext};"));

                linkedResources.Add(img);
                mimeParts.Add(mimePart);

                childNode.SetAttributeValue("src", $"cid:{img.ContentId}");
            }
            var htmlContent = htmlDoc.DocumentNode.WriteTo();
            var result = new Tuple<MimePartCollection, string>(mimeParts, htmlContent);
            return result;
        }



    }
}