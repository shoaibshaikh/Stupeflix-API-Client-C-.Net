---------------------------------------------------------------------------------
Stupeflix web services client libraries & examples.
http://www.stupeflix.com

This project is just a java conversion of stupeflix API client in c# .net.
There is a class library for stupeflix and a TestApplication that contains the sample code.

The examples will use the movie.xml example file in the current directory.
The movie.xml file is commented, and will give you both simple and advanced tricks to use 
the movie description xml language.

To run this code, you will need to change the access key and private key 
in the "keys.XXX" file  (keys.php, keys.py, key.rb ...) .
To get your accessKey / secret Key pair, 
go to http://accounts.stupeflix.com/ .

The XML file format is described in the stupeflix wiki: http://wiki.stupeflix.com

Notes on the API:

The web services is REST based, this library is a simple wrapper for
calling HTTP methods with proper signature based authentication.

The signature scheme is heavily inspired from Amazon S3.
The main difference is that only a minimum set of headers must be included,
other parameters are included in the url itself.
This is intended to simplify the process on the client side : read-only requests 
are totally url contained.
