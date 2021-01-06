namespace Nacos.V2.Config.Utils
{
    using Nacos.V2.Common;
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
                throw new ArgumentException("发布/删除内容不能为空");
            }

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                if (c == '\r' || c == '\n')
                {
                    throw new ArgumentException("发布/删除内容不能包含回车和换行");
                }

                if (c == Constants.WORD_SEPARATOR[0])
                {
                    throw new ArgumentException("发布/删除内容不能包含(char)2");
                }
            }
        }

        public static string GetContentIdentity(string content)
        {
            int index = content.IndexOf(Constants.WORD_SEPARATOR);
            if (index == -1)
            {
                throw new ArgumentException("内容没有包含分隔符");
            }

            return content.Substring(0, index);
        }

        public static string GetContent(string content)
        {
            int index = content.IndexOf(Constants.WORD_SEPARATOR);
            if (index == -1)
            {
                throw new ArgumentException("内容没有包含分隔符");
            }

            return content.Substring(index + 1);
        }

        /// <summary>
        /// Truncate content.
        /// </summary>
        /// <param name="content">content</param>
        /// <returns>truncated content</returns>
        public static string TruncateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
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
