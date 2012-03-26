using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace Stupeflix
{
    public class StupeflixBase
    {
        protected static String HEADER_CONTENT_TYPE = "Content-Type";
        protected static String HEADER_CONTENT_LENGTH = "Content-Length";
        protected static String HEADER_CONTENT_MD5 = "Content-MD5";
        protected static String ACCESS_KEY_PARAMETER = "AccessKey";
        protected static String SIGNATURE_PARAMETER = "Signature";
        protected static String DATE_PARAMETER = "Date";
        protected static String PROFILE_PARAMETER = "Profiles";
        protected static String XML_PARAMETER = "ProfilesXML";
        protected static String HMAC_SHA1_ALGORITHM = "HmacSHA1";

        protected String service = "stupeflix-1.0";
        private String accessKey;
        private String secretKey;

        protected bool debug = false;
        protected Dictionary<string, string> parametersToAdd = new Dictionary<string, string>();

        public StupeflixBase(String accessKey, String secretKey, bool debug)
        {
            this.accessKey = accessKey;
            this.secretKey = secretKey;
            this.debug = debug;
            this.parametersToAdd.Add("Marker", "true");
            this.parametersToAdd.Add("MaxKeys", "true");
        }


        public static long fileSize(String filename)
        {
            return new FileInfo(filename).Length;
        }

        // Build the canonical string containing all parameters for signing
        // @param parameters : The additional parameters to be added to the string to sign
        // @return : The canonical string for parameters
        public String paramString(Dictionary<string, string> parameters)
        {
            String paramStr = "";
            if (parameters != null)
                foreach (var itm in parametersToAdd)
                {
                    if (parameters.ContainsKey(itm.Key))
                    {
                        paramStr += parameters[itm.Key];
                    }
                }
            return paramStr;
        }

        // Build the string to sign for a query
        // @param method     : HTTP Method (should be "GET", "PUT" or "POST" )
        // @param url        : base url to be signed
        // @param md5        : content md5
        // @param mime       : mime type ("" for GET query)
        // @param datestr    : date  (seconds since epoch)
        // @param parameters : additional url parameters
        // @return : The string to sign
        public String strToSign(String method, String url, String md5, String mime, long datestr, Dictionary<string, string> parameters)
        {
            // Build the canonical parameter string
            String paramStr = this.paramString(parameters);
            // Build the full service path
            String path = "/" + this.service + url;
            // Build the full string to be signed
            String stringToSign = method + "\n" + md5 + "\n" + mime + "\n" + datestr + "\n" + path + "\n" + paramStr;
            return stringToSign;
        }

        // Sign a request
        // @param string     : The String to be signed
        // @param secretKey  : The secretKey to be user
        // @return : The hmac signature for the request
        public String sign(String str, String secretKey)
        {

            using (HMACSHA1 hmac = new HMACSHA1(ASCIIEncoding.UTF8.GetBytes(secretKey)))
            {
                var ba = hmac.ComputeHash(ASCIIEncoding.UTF8.GetBytes(str));
                return bin2hex(ba);
            }

            //SecretKeySpec signingKey = new SecretKeySpec(secretKey.getBytes(), HMAC_SHA1_ALGORITHM);

            //// Acquire the MAC instance and initialize with the signing key.
            //Mac mac = null;
            //try {
            //    mac = Mac.getInstance(HMAC_SHA1_ALGORITHM);
            //} catch (NoSuchAlgorithmException e) {
            //    // should not happen
            //    throw new RuntimeException("Could not find sha1 algorithm", e);
            //}
            //try {
            //    mac.init(signingKey);
            //} catch (InvalidKeyException e) {
            //    // also should not happen
            //    throw new RuntimeException("Could not initialize the MAC algorithm", e);
            //}

            //byte[] bytes = mac.doFinal(string.getBytes());

            //String form = "";
            //for (int i = 0; i < bytes.Length; i++) {            
            //    String str = Integer.toHexString(((int)bytes[i]) & 0xff);
            //    if (str.Length() == 1) {
            //        str = "0" + str;
            //    }                                    

            //    form = form + str;                
            //}
            //return form;
        }


        // Sign an request, using url, method body ...
        // @param url        : The url to be signed
        // @param method     : The HTTP method to be used for request
        // @param md5        : The md5 of the body, or "" for "GET" requests
        // @param mime       : The mime type of the request
        // @param parameters : Some optional additional parameters
        // @return the hmac signature of the request
        public String signUrl(String url, String method, String md5, String mime, Dictionary<string, string> parameters, bool inlineAuth)
        {
            // Get seconds since epoch, integer type
            long now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            // Build the string to be signed
            String strToSign = this.strToSign(method, url, md5, mime, now, parameters);
            // Build the signature
            String signature = this.sign(strToSign, this.secretKey);
            // Build the signed url
            String accessKey = this.accessKey;
            String accessKeyParam = ACCESS_KEY_PARAMETER;
            String dateParam = DATE_PARAMETER;
            String signParam = SIGNATURE_PARAMETER;
            // Add date, public accesskey, and signature to the url

            if (inlineAuth)
            {
                url = url + accessKey + "/" + signature + "/" + now + "/";
            }
            else
            {
                url = url + "?" + dateParam + "=" + now + "&" + accessKeyParam + "=" + accessKey + "&" + signParam + "=" + signature;
            }

            //// Finally add, if needed, additional parameters
            //if (parameters != null)
            //{

            //    Iterator p = parameters.keySet().iterator();
            //    while (p.hasNext()) 
            //    {
            //        Object key = p.next();
            //        Object value = parameters.get(key);
            //        url = url + "&" + key.toString() + "=" + value.toString();
            //    }
            //}
            if (parameters != null)
                url = string.Format("{0}{1}", url, string.Join("", parameters.Select(r => string.Format("&{0}={1}", r.Key, r.Value)).ToArray()));
            return url;
        }


        public static String bin2hex(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }


        // Build a (md5, md5 hexadecimal, md5 base 64) array for a given md5
        // @return : The array
        public static Dictionary<string, string> md5triplet(byte[] md5)
        {
            Dictionary<string, string> hashMap = new Dictionary<string, string>();
            String md5hex = StupeflixBase.bin2hex(md5);
            String md5base64 = Convert.ToBase64String(md5);

            //hashMap.Add("std", md5);
            hashMap.Add("hex", md5hex);
            hashMap.Add("base64", md5base64);
            return hashMap;
        }



        // Compute the (md5, md5 hexadecimal, md5 base 64) triplet for of a file
        // @param filename : The file to be hashed
        // @return         : The md5 array


        public static string UpperCaseUrlEncode(string s)
        {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }



        public static Dictionary<string, string> md5file(String filename)
        {
            using(StreamReader sr = new StreamReader(filename))
			{
            MD5CryptoServiceProvider md5h = new MD5CryptoServiceProvider();
            var md5 = md5h.ComputeHash(sr.BaseStream);
            return StupeflixBase.md5triplet(md5);
			}
        }

        // Compute the (md5, md5 hexadecimal, md5 base 64) triplet for of a file
        // @param filename : The file to be hashed
        // @return         : The md5 array
        public static Dictionary<string, string> md5string(String str)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
            var md5 = x.ComputeHash(bs);
            return StupeflixBase.md5triplet(md5);
        }

        // Check if a filename is a zip
        // @param filename : The file name
        // @return         : A bool : true if it is a zip, false otherwise
        public static bool isZip(String filename, String str)
        {
            // Buffer to read the file
            byte[] buffer = new byte[4];
            if (filename != null)
            {
                using (var file = File.OpenRead(filename))
                {
                    file.Read(buffer, 0, 4);
                }

            }
            else
            {
                byte[] buf = ASCIIEncoding.UTF8.GetBytes(str);
                for (int i = 0; i < 4; i++)
                {
                    buffer[i] = buf[i];
                }
            }
            byte[] zipMagic = ASCIIEncoding.UTF8.GetBytes("PK\x03\x04");
            return Array.Equals(zipMagic, buffer);
        }
    }
}
