using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Stupeflix
{
    public class Stupeflix : StupeflixBase
    {
        protected static String TEXT_XML_CONTENT_TYPE = "text/xml";
        protected static String APPLICATION_ZIP_CONTENT_TYPE = "application/zip";
        protected static String APPLICATION_URLENCODED_CONTENT_TYPE = "application/x-www-form-urlencoded";
        protected String host;
        protected static String service;
        protected static String base_url;

        // Class constructor
        // @param accessKey : User access Key
        // @param secretKey : User secret Key
        // @param service   : Name of the service
        // @param debug     : Debug mode or not
        public Stupeflix(String accessKey, String secretKey, String host, String service, bool debug)
            : base(accessKey, secretKey, debug)
        {

            this.host = (host == null) ? "services.stupeflix.com" : host;
            Stupeflix.service = (service == null) ? "stupeflix-1.0" : service;
            base_url = this.host + "/" + service;
        }

        // Create a new connection object
        // @return : A new connection object
        public StupeflixConnection connectionGet()
        {
            return new StupeflixConnection(this.host, service, this.debug);
        }


        // Build the url for access to the definition of a user/resource
        // @param user     : The user
        // @param resource : The resource
        // @return         : The url for the request
        public static String definitionUrl(String user, String resource)
        {
            return "/" + user + "/" + resource + "/definition/";
        }

        // Build the base url to launch the generation of a set of profiles
        // @param user     : The user name
        // @param resource : The resource name
        // @return         : The base url
        public String createProfilesUrl(String user, String resource)
        {
            return "/" + user + "/" + resource + "/";
        }

        // Build the base url to access the video generated from user/resource with the given profile
        // @param user     : The user name
        // @param resource : The resource name
        // @param profile  : The profile name
        // @return         : The base url
        public String profileUrl(String user, String resource, String profile)
        {
            return "/" + user + "/" + resource + "/" + profile + "/";
        }

        // Build url for profile status querying.
        // The status can be asked on every user, user alone, user/resource or
        // user/resource/profile.
        // @param user     : The user name
        // @param resource : The resource name
        // @param profile  : The profile name
        // @return         : The built url
        public String profileStatusUrl(String user, String resource, String profile)
        {
            String str = "/";
            if (user != null)
            {
                str += user + "/";
                if (resource != null)
                {
                    str += resource + "/";
                    if (profile != null)
                    {
                        str += profile + "/";
                    }
                }
            }

            return str + "status/";
        }


        // Send a definition to the service
        // The file to be sent can be a zip file containing a xml description file movie.xml and other assets, or simply movie.xml.
        // In the first case, all images should be in "images" zip sub-directory, music under "music" sub-directory etc.
        // In the latter case, all assets reference should be urls (images, music ...)
        // See wiki.stupeflix.com for more information
        // @param user     : The user name
        // @param resource : The resource name
        // @param filename : Name of the file to be uploaded
        // @return         : True in case of success, will throw an exception otherwise
        public void sendDefinition(String user, String resource, String filename, String body)
        {
            // Check parameters
            if (user == null || resource == null || (filename == null && body == null))
            {
                throw new Exception("Stupeflix sendDefinition : user, resource and (filename or body) must be defined.");
            }

            // Build the request url
            String url = Stupeflix.definitionUrl(user, resource);

            String contentType;
            // Set the content type according to upload type
            if (Stupeflix.isZip(filename, body))
            {
                contentType = APPLICATION_ZIP_CONTENT_TYPE;
            }
            else
            {
                contentType = TEXT_XML_CONTENT_TYPE;
            }

            // Finally send the content through a HTTP PUT
            this.sendContent("PUT", url, filename, body, contentType);
        }

        // Get a definition from the service
        // @param user     : The user name
        // @param resource : The resource name
        // @param filename : Name of the file where data will be downloaded
        // @return         : True in case of success, will throw an exception otherwise
        public void getDefinition(String user, String resource, String filename)
        {
            // Check parameters
            if (user == null || resource == null || filename == null)
            {
                throw new Exception("Stupeflix getDefinition : user, resource and filename must be defined.");
            }
            // Build the request url
            String url = definitionUrl(user, resource);
            // Get the content
            this.getContent(url, filename);
        }

        // Launch the generation of a set of profiles for a given user/resource
        // @param user     : The user name
        // @param resource : The resource name
        // @param profiles : An array of string containing the names of the profiles to be generated.
        // @return         : true upon success, otherwise an exception will be raised
        public void createProfiles(String user, String resource, StupeflixProfileSet profiles)
        {
            // Create the base url
            String url = this.createProfilesUrl(user, resource);
            String xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + profiles.ToString();
            String body = XML_PARAMETER + "=" + UpperCaseUrlEncode(xml);

            this.sendContent("POST", url, null, body, APPLICATION_URLENCODED_CONTENT_TYPE);
        }


        // Build the signed url to access a video generated from user/resource with the given profile
        // @param user     : The user name
        // @param resource : The resource name
        // @param profile  : The profile name
        // @return         : True on success, otherwise an exception will be raised
        public void getProfile(String user, String resource, String profile, String filename)
        {
            // Check parameters
            if (user == null || resource == null || profile == null || filename == null)
            {
                throw new Exception("Stupeflix getProfile: all parameters should be defined");
            }
            // Get the profile url
            String url = this.profileUrl(user, resource, profile);
            // Retrieve the content from the profile url
            this.getContent(url, filename);
        }

        public String getProfileURL(String user, String resource, String profile)
        {
            String url = this.profileUrl(user, resource, profile);
            url = this.signUrl(url, "GET", "", "", null, false);
            return "http://" + base_url + url;
        }

        // Build the url for access to the definition of a user/resource/profile
        // @param user     : The user
        // @param resource : The resource
        // @param profile  : The profile
        // @return         : The url for the request
        public static String profilePreviewUrl(String user, String resource, String profile)
        {
            return "/" + user + "/" + resource + "/" + profile + "/preview.flv/";
        }

        public String getProfilePreviewURL(String user, String resource, String profile)
        {
            String url = profilePreviewUrl(user, resource, profile);
            // Sign the url
            url = this.signUrl(url, "GET", "", "", null, true);
            // Add the base url
            return "http://" + base_url + url;
        }

        // Get Profile status
        // The status can be asked on every user, user alone, user/resource or
        // user/resource/profile.
        // The returned object is an array of dictionaries, one for each matching profiles.
        // Each dictionary contains the user, resource and profiles names, and the status for this specific profile.
        // The status itself is a dictionary with following keys
        //      - status   : always present : general status : queued, generating, available, or error
        //      - complete : appear after generating has started : gives the percentage done for profile
        //      - error    : if status is error : give the error string
        // @param user     : The user name
        // @param resource : The resource name
        // @param profile  : The profile name
        // @return         : The response xml
        public String getProfileStatus(String user, String resource, String profile)
        {
            String url = this.profileStatusUrl(user, resource, profile);
            StupeflixConnection connection = this.getContent(url, null);
            String body = ASCIIEncoding.UTF8.GetString(connection.getResponseBodyBytes());

            return body;
        }


        // Build the url for access to the definition of a user/resource
        // @param user     : The user
        // @param resource : The resource
        // @return         : The url for the request
        public static String profileThumbUrl(String user, String resource, String profile, String thumbUrl)
        {
            return "/" + user + "/" + resource + "/" + profile + "/" + thumbUrl + "/";
        }


        public String getProfileThumbURL(String user, String resource, String profile)
        {
            String url = profileThumbUrl(user, resource, profile, "thumb.jpg");
            // Sign the url
            url = this.signUrl(url, "GET", "", "", null, false);
            // Add the base url
            return "http://" + base_url + url;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        //
        // Implementation
        //
        //////////////////////////////////////////////////////////////////////////////////////

        public void answer_error(StupeflixConnection connection, String message)
        {
            String body = ASCIIEncoding.UTF8.GetString(connection.getResponseBodyBytes());
            throw new Exception(message + "\nERROR: " + body);
        }

        public String etagFix(String etag)
        {
            if (etag[0] == '"')
            {
                return etag.Substring(1, etag.Length - 2);
            }
            else
            {
                return etag;
            }
        }


        // Helper function to send some content.
        // @param method      : HTTP method, "PUT" or "POST"
        // @param url         : The base url to be used (will be signed by this function)
        // @param filename    : Optional filename containing the body data to be sent
        // @param body        : Optional string containing the body data to be sent (one of body or filename must be defined)
        // @param contentType : Content type to be used in headers
        // @return            : true on success, will throw an exception on error
        public void sendContent(String method, String url, String filename, String body, String contentType)
        {
            // Check parameters
            if (url == null)
            {
                throw new Exception("Stupeflix sendContent: url should be defined");
            }

            if ((filename == null) && (body == null) || ((filename != null) && (body != null)))
            {
                throw new Exception("Stupeflix sendContent: exactly one of filename and body should be defined");
            }

            Dictionary<string, string> md5hashes;
            long size;
            if (body != null)
            {
                // If body is defined, hash the body
                md5hashes = Stupeflix.md5string(body);
                size = ASCIIEncoding.UTF8.GetBytes(body).Length;
            }
            else
            {
                // Otherwise, hash the file
                md5hashes = Stupeflix.md5file(filename);
                size = Stupeflix.fileSize(filename);
            }

            // Get the right version of md5 : base64 one
            String md5base64 = (String)md5hashes["base64"];

            // Build the headers array
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(HEADER_CONTENT_MD5, md5base64);
            headers.Add(HEADER_CONTENT_LENGTH, size.ToString());
            headers.Add(HEADER_CONTENT_TYPE, contentType);

            // Sign the url
            url = this.signUrl(url, method, md5base64, contentType, null, false);

            // Execute the request
            StupeflixConnection connection = this.connectionGet();
            connection.execute(method, url, filename, body, headers);

            // Check the return status
            int status = connection.getResponseCode();
            if (status != 200)
            {
                this.answer_error(connection, "Stupeflix::sendContent: bad status: " + status);
            }

            // Check the returned etag (hex form of md5): should be the same as the sent one
            String refEtag = (String)md5hashes["hex"];
            String etag = (String)connection.getResponseHeaders()["ETag"];
            etag = this.etagFix(etag);

            if (!etag.Equals(refEtag))
            {
                this.answer_error(connection, "Stupeflix::sendContent: bad etag " + etag + " != " + refEtag + " (ref)");
            }
        }

        // Retrieve content from an url. Content will be put in a file or returned.
        // @param url      : The url to be queried
        // @param filename : The optional file where to put the data
        // @return         : The StupeflixConnection that was used to send the request
        public StupeflixConnection getContent(String url, String filename)
        {
            // Check parameters
            if (url == null)
            {
                throw new Exception("Stupeflix getContent: url should be defined");
            }

            // Method is always "GET"
            String method = "GET";
            // Sign the url
            String signedUrl = this.signUrl(url, method, "", "", null, false);

            // Send the request
            StupeflixConnection connection = this.connectionGet();
            connection.execute("GET", signedUrl, filename, null, null);

            // Check the status code
            int status = connection.getResponseCode();
            if (status != 200)
            {
                this.answer_error(connection, "Stupeflix::getContent: bad status: " + status);
            }

            // Get the returned etag
            String etag = (String)connection.getResponseHeaders()["ETag"];

            Dictionary<string, string> md5hashes;
            // Hash the result (from file or body)
            if (filename != null)
            {
                md5hashes = md5file(filename);
            }
            else
            {
                md5hashes = md5string(ASCIIEncoding.UTF8.GetString(connection.getResponseBodyBytes()));
            }

            // Get the right version of md5 (hex one)
            String refEtag = (String)md5hashes["hex"];

            etag = this.etagFix(etag);

            // Check that etag matches.
            if (!etag.Equals(refEtag))
            {
                this.answer_error(connection, "Stupeflix::getContent: bad etag " + etag + " != " + refEtag + " (ref)");
            }

            return connection;
        }
    }



}
