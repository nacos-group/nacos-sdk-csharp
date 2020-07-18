namespace Nacos.Exceptions
{
    using System;

    public class NacosException : Exception
    {
        public NacosException(string message)
            : base(message)
        {
            this.ErrorMsg = message;
        }

        public NacosException(int code, string message)
            : base(message)
        {
            this.ErrorCode = code;
            this.ErrorMsg = message;
        }

        public int ErrorCode { get; set; }

        public string ErrorMsg { get; set; }
    }
}