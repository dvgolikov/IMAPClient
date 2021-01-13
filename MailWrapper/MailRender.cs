using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MailKit;
using MimeKit;
using MimeKit.Text;

namespace MailClient.MailWrapper
{
    class MailRender
	{
		public string RenderMultipartRelated(MultipartRelated related)
		{
			var root = related.Root;
			var text = root as TextPart;

            if (root is Multipart multipart)
            {
                for (int i = multipart.Count; i > 0; i--)
                {
                    if (!(multipart[i - 1] is TextPart body)) continue;

                    if (body.ContentType.IsMimeType("text", "html"))
                    {
                        text = body;
                        break;
                    }

                    if (text == null) text = body;
                }
            }

            if (text != null)
			{
				if (text.ContentType.IsMimeType("text", "html"))
				{
					var ctx = new MultipartRelatedImageContext(related);
					var converter = new HtmlToHtml() { HtmlTagCallback = ctx.HtmlTagCallback };
					return converter.Convert(text.Text);
				}
				else
				{
					return RenderText(text);
				}
			}
			else
			{
				return "Uncknown message type.";
			}
		}

		public async Task<string> RenderMultipartRelatedAsync(IMailFolder folder, UniqueId uid, BodyPartMultipart bodyPart)
		{
			// download the entire multipart/related for simplicity since we'll probably end up needing all of the image attachments anyway...
			var related = await folder.GetBodyPartAsync(uid, bodyPart) as MultipartRelated;

			return RenderMultipartRelated(related);
		}

		string RenderText(TextPart text)
		{
			string html;

			if (text.IsHtml) return text.Text;

			else if (text.IsFlowed)
			{
				var converter = new FlowedToHtml();
				string delsp;

				// the delsp parameter specifies whether or not to delete spaces at the end of flowed lines
				if (!text.ContentType.Parameters.TryGetValue("delsp", out delsp))
					delsp = "no";

				if (string.Compare(delsp, "yes", StringComparison.OrdinalIgnoreCase) == 0)
					converter.DeleteSpace = true;

				html = converter.Convert(text.Text);
			}
			else
			{
				html = new TextToHtml().Convert(text.Text);
			}

			return html;
		}

		async Task<string> RenderTextAsync(IMailFolder folder, UniqueId uid, BodyPartText bodyPart)
		{
			var entity = await folder.GetBodyPartAsync(uid, bodyPart);

			return RenderText((TextPart)entity);
		}

		public async Task<string> RenderAsync(IMailFolder folder, UniqueId uid, BodyPart body)
		{
			var multipart = body as BodyPartMultipart;

			if (multipart != null && body.ContentType.IsMimeType("multipart", "related"))
			{
				return await RenderMultipartRelatedAsync(folder, uid, multipart).ConfigureAwait(false);
			}

			var text = body as BodyPartText;

			if (multipart != null)
			{
				if (multipart.ContentType.IsMimeType("multipart", "alternative"))
				{
					for (int i = multipart.BodyParts.Count; i > 0; i--)
					{
                        if (multipart.BodyParts[i - 1] is BodyPartMultipart multi && multi.ContentType.IsMimeType("multipart", "related"))
                        {
                            if (multi.BodyParts.Count == 0) continue;

                            var start = multi.ContentType.Parameters["start"];
                            var root = multi.BodyParts[0];

                            if (!string.IsNullOrEmpty(start)) root = multi.BodyParts.OfType<BodyPartText>().FirstOrDefault(x => x.ContentId == start);

                            if (root != null && root.ContentType.IsMimeType("text", "html")) return await RenderAsync(folder, uid, multi).ConfigureAwait(false);

                            continue;
                        }

                        text = multipart.BodyParts[i - 1] as BodyPartText;

						if (text != null) return await RenderTextAsync(folder, uid, text).ConfigureAwait(false);
					}
				}
				else if (multipart.BodyParts.Count > 0) return await RenderAsync(folder, uid, multipart.BodyParts[0]).ConfigureAwait(false);
			}
			else if (text != null) return await RenderTextAsync(folder, uid, text).ConfigureAwait(false);

			return "Uncknown message type.";
		}
	}
}
