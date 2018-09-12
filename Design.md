# DESIGN

Before this re-factory, downloader & selector & pipeline are coupling  together, and now i think i should split them. If some one want to use downloader only and use AngleSharp to extract data.

###  Dowloader

Downloader is a independent module to help user to download data from target website. There are a lot of details, see below:

1. HttpWebRequestDownloader is suggest use in NET4.0, after NET4.0 you should use HttpClientDownloader. And i will not spend too much words to talk about HttpWebRequestDownloader, next are about HttpClientDownloader.
2. 
