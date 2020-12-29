using ExtECRMainLogic.Classes.Helpers;
using System;
using System.Runtime.Serialization;

namespace ExtECRMainLogic.Exceptions
{
    public class CustomException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CustomException() : base()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorCode"></param>
        public CustomException(int errorCode) : base(ExtcerErrorHelper.GetErrorDescription(errorCode))
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public CustomException(string message) : base(message)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CustomException(string message, Exception innerException) : base(message, innerException)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public CustomException(string format, params object[] args) : base(string.Format(format, args))
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="format"></param>
        /// <param name="innerException"></param>
        /// <param name="args"></param>
        public CustomException(string format, Exception innerException, params object[] args) : base(string.Format(format, args), innerException)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public CustomException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}