# DESIGN

Before this re-factory, downloader & selector & pipeline are coupling  together, and now i think i should split them. If some one want to use downloader only and use AngleSharp to extract data.

###  Dowloader

Downloader is a independent module to help user to download data from target website. There are a lot of details, see below:

1. Two ways to set cookie, one is call the AddCookie method in downloader, it add cookie to CookieContainer so impact every request.
Set cookie header in request, the result is combine you cookie header and cookies in CookieContainer. 
2. CookieInjector in downloader is invoked one time, and inject cookies to CookieContainer.

### Scheduler

#### Request hash

1. Same url different headers are different requests, so headers are a factor  
2. There is a CycleRetryTimes property in a request, if value are different, then requests are different. Depth property is not
a factor.

 
