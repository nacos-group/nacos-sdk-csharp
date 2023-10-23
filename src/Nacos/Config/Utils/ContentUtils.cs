namespace Nacos.Config.Utils
{
    using Nacos.Common;
    using Nacos.Utils;
    using System;

    public static class ContentUtils
    {
        /// <summary>
        /// Verify increment pub content.
        /// </summary>
        /// <param name="content">content</param>
        public static void VerifyIncrementPubContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("publish/delete content can not be null");
            }

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                if (c == '\r' || c == '\n')
                {
                    throw new ArgumentException("publish/delete content can not contain return and linefeed");
                }

                if (c == Constants.WORD_SEPARATOR[0])
                {
                    throw new ArgumentException("publish/delete content can not contain(char)2");
                }
            }
        }

        /// <summary>
        /// Truncate content.
        /// </summary>
        /// <param name="content">content</param>
        /// <returns>truncated content</returns>
        public static string TruncateContent(string content)
        {
            if (content.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }
            else if (content.Length <= SHOW_CONTENT_SIZE)
            {
                return content;
            }
            else
            {
                return content.Substring(0, SHOW_CONTENT_SIZE) + "...";
            }
        }

        private static readonly int SHOW_CONTENT_SIZE = 100;
    }
}
