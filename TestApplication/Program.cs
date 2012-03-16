using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stupeflix;
using System.Threading;

namespace TestApplication
{
    class Program
    {
        static String stupeflixAccessKey = "xxxxxxxxxxxxxxxxxxxxxxxxx";
        static String stupeflixSecretKey = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        static String stupeflixHost = "services.stupeflix.com";

        static void Main(string[] args)
        {

            // Create the stupeflix client
            Stupeflix.Stupeflix stupeflix = new Stupeflix.Stupeflix(stupeflixAccessKey, stupeflixSecretKey, stupeflixHost, null, false);

            // Choose some identifiers (you can select whatever you want, provided it is alphanumerical)
            String user = "name";
            String resource = "resource120";

            // Send the definition file
            stupeflix.sendDefinition(user, resource, "movie.xml", null);

            // Build the profile set
            String[] profiles = new String[1];
            profiles[0] = "iphone";
            //profiles[1] = "youtube";
            //profiles[1] = "youtube";
            StupeflixProfileSet profileSet = new StupeflixProfileSet(profiles);

            // Create the profiles
            stupeflix.createProfiles(user, resource, profileSet);

            // Loop while the profile is not available
            while (true)
            {
                // Retrieve the status for all the profiles
                String status = stupeflix.getProfileStatus(user, resource, "iphone");
                int donecounter = 0;
                var allprofiles = status.Split(new string[] { "}," }, StringSplitOptions.None);
                foreach (var profile in profiles)
                {
                    var profilestatus = allprofiles.Where(r => r.Contains(string.Format("\"profile\":\"{0}\"", profile))).FirstOrDefault();
                    if (profilestatus != null)
                    {
                        Console.WriteLine(profilestatus);
                        Console.WriteLine();
                        Console.WriteLine();
                        // Here you can use the XML lib you want to parse status String as an XML file.
                        // We will only check that the status is available or not
                        if (profilestatus.Contains("\"status\":\"available\""))
                        {
                            donecounter++;
                            continue;
                        }
                        // We encountered an error : we will stop too
                        if (profilestatus.Contains("\"status\":\"error\""))
                        {
                            break;
                        }
                        // Sleep for 5 seconds

                    }
                }

                if (donecounter == profiles.Length)
                    break;

                Thread.Sleep(5000);
            }


            // Retrieve the video for the first profile (you may have to change the file extension on some systems to play the video)
            for (int i = 0; i < profiles.Length; i++)
            {
                Console.WriteLine(profiles[i]);
                Console.WriteLine("Downloading OUTPUT_" + i + ".mp4");
                stupeflix.getProfile(user, resource, profiles[i], "OUTPUT_" + i + ".mp4");
                Console.WriteLine("Thumbnail url !");
                Console.WriteLine(stupeflix.getProfileThumbURL(user, resource, profiles[i]));
                Console.WriteLine("Video url !");
                Console.WriteLine(stupeflix.getProfilePreviewURL(user, resource, profiles[i]));
                Console.WriteLine("Done !");
            }
        }
    }
}
