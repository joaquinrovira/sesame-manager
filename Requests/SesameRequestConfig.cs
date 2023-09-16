public static class SesameRequestConfig {
    public static void Apply(HttpRequestMessage request) {
        request.Headers.Add("Host","back-eu1.sesametime.com");
        request.Headers.Add("User-Agent","Mozilla/5.0 (X11; Linux x86_64; rv:102.0) Gecko/20100101 Firefox/102.0");
        request.Headers.Add("Accept","application/json");
        request.Headers.Add("Accept-Language","en-GB,en;q=0.5");
        request.Headers.Add("Accept-Encoding","gzip, deflate, br");
        request.Headers.Add("rsrc","31");
        request.Headers.Add("traceparent","00-51f78f4ab59e4e732e2cc5b17b229b4a-fd2784da8dfd35ce-00");
        request.Headers.Add("Origin","https://app.sesametime.com");
        request.Headers.Add("Referer","https://app.sesametime.com/");
        request.Headers.Add("Sec-Fetch-Dest","empty");
        request.Headers.Add("Sec-Fetch-Mode","cors");
        request.Headers.Add("Sec-Fetch-Site","same-site");
        request.Headers.Add("TE","trailers");
    }
}