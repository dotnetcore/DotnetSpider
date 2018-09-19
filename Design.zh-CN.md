# DESIGN

在此重大更新之前, 下载器 & 选择器 & 数据管理是耦合在一起的, 经过许久思考我决定把它们解耦出来, 这样用户可以自由的选择他们喜爱的组件. 比如说, 下载器使用框架自带的Downloader、WebClientApi、苏菲的HttpHelper等; 解析器可以使用框架自带的Extraction、AngleSharp等;

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

 
