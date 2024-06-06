using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class NetworkRequest
{
    public static async Task<string> PostHttpResponse(string url, Dictionary<string, string> formData)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                var content = new FormUrlEncodedContent(formData);
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, content);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await httpResponseMessage.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"Failed with status code: {httpResponseMessage.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"HTTP request failed. Exception: {ex.Message}");
            }
        }
    }



}
