using PayPal.Api;
using System;
using System.Collections.Generic;

namespace WebBanMyPham.Models
{
    public static class PaypalConfiguration
    {
        private static readonly string ClientId;
        private static readonly string ClientSecret;

        static PaypalConfiguration()
        {
            try
            {
                Dictionary<string, string> config = null;
                try
                {
                    config = GetConfig();
                }
                catch (Exception ex)
                {
                    throw new Exception("Không thể đọc cấu hình PayPal: " + ex.Message);
                }

                if (config == null)
                {
                    throw new Exception("Cấu hình PayPal trả về null");
                }

                if (!config.ContainsKey("clientId") || string.IsNullOrEmpty(config["clientId"]))
                {
                    throw new Exception("Không tìm thấy clientId trong cấu hình PayPal");
                }

                if (!config.ContainsKey("clientSecret") || string.IsNullOrEmpty(config["clientSecret"]))
                {
                    throw new Exception("Không tìm thấy clientSecret trong cấu hình PayPal");
                }

                ClientId = config["clientId"];
                ClientSecret = config["clientSecret"];
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                System.Diagnostics.Debug.WriteLine($"PayPal Configuration Error: {ex.Message}");
                throw;
            }
        }

        public static Dictionary<string, string> GetConfig()
        {
            try
            {
                var config = ConfigManager.Instance.GetProperties();
                if (config == null)
                {
                    throw new Exception("ConfigManager trả về null");
                }
                return config;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy cấu hình PayPal: {ex.Message}");
            }
        }

        private static string GetAccessToken()
        {
            try
            {
                var tokenCredential = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig());
                var accessToken = tokenCredential.GetAccessToken();
                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy access token: {ex.Message}");
            }
        }

        public static APIContext GetAPIContext()
        {
            try
            {
                var apiContext = new APIContext(GetAccessToken());
                apiContext.Config = GetConfig();
                return apiContext;
            }
            catch (Exception ex)
            {
                 throw new Exception($"Lỗi khi tạo API context: {ex.Message}", ex); // Truyền ex như tham số thứ 2
            }
        }
    }
}