using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Utils
{
    public record HWSocketMessage(string Command, string DeviceCode, string Time, string Content);
    public static class MessageHelper
    {

        //public static readonly HWSocketMessage 


        public static string GetSocketMessage(string command, string deviceCode, string content)
        {
            //if (command.Length != 4) 
            //{
            //    throw new ArgumentException($"command length must equal with 4: {command}");
            //}

            //if (deviceCode.Length != 8)
            //{
            //    throw new ArgumentException($"deviceCode length must equal with 8: {deviceCode}");
            //}

            if (content == null)
            {
                throw new ArgumentException("content cannot be null");
            }
            return $"{command,-4}{deviceCode,-8}{DateTime.Now:yyyyMMddHHmmss}${content}#";
        }


        public static HWSocketMessage Parse(string message)
        {
            return new HWSocketMessage(message[..4], message[4..12], message[12..26], message[27..]);

            //try
            //{
            //}
            //catch (Exception)
            //{

            //    throw;
            //}
        }

    }
}
