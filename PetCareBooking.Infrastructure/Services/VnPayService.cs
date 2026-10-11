using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PetCareBooking.Application.DTOs.Payment;
using PetCareBooking.Application.Interfaces;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PetCareBooking.Infrastructure.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(Guid bookingId, decimal amount, string orderDescription, string ipAddress, string? returnUrl = null)
        {
            var tmnCode = _configuration["VnPayConfig:TmnCode"] ?? _configuration["VnPaySettings:TmnCode"] ?? "APCVE5A9";
            var hashSecret = _configuration["VnPayConfig:HashSecret"] ?? _configuration["VnPaySettings:HashSecret"] ?? "1TPARAEFOBL2HNHOD4S95PQC530T1TO1";
            var baseUrl = _configuration["VnPayConfig:BaseUrl"] ?? _configuration["VnPaySettings:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var defaultReturnUrl = _configuration["VnPayConfig:ReturnUrl"] ?? _configuration["VnPaySettings:ReturnUrl"] ?? "http://localhost:5173/payment-result";

            var timeNow = DateTime.UtcNow.AddHours(7); // Vietnam timezone UTC+7
            var payDate = timeNow.ToString("yyyyMMddHHmmss");
            var expireDate = timeNow.AddMinutes(15).ToString("yyyyMMddHHmmss");

            // Sanitize IP: VNPay rejects IPv6 / ::1, requires standard IPv4
            var validIp = (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Contains(":")) ? "127.0.0.1" : ipAddress;

            // Make TxnRef unique per payment attempt to prevent "Transaction already exists" error
            var txnRef = $"{bookingId}_{timeNow:yyyyMMddHHmmss}";

            var cleanOrderInfo = string.IsNullOrWhiteSpace(orderDescription)
                ? $"Thanh toan don {bookingId}"
                : orderDescription.Trim();

            var requestData = new SortedList<string, string>(new VnPayCompare())
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() },
                { "vnp_CreateDate", payDate },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", validIp },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", cleanOrderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", !string.IsNullOrWhiteSpace(returnUrl) ? returnUrl : defaultReturnUrl },
                { "vnp_TxnRef", txnRef },
                { "vnp_ExpireDate", expireDate }
            };

            var data = new StringBuilder();
            foreach (var kv in requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            var queryString = data.ToString().TrimEnd('&');
            var vnpSecureHash = HmacSha512(hashSecret, queryString);
            return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
        }

        public VnPayCallbackResult ProcessPaymentCallback(IQueryCollection query)
        {
            var hashSecret = _configuration["VnPayConfig:HashSecret"] ?? _configuration["VnPaySettings:HashSecret"] ?? "1TPARAEFOBL2HNHOD4S95PQC530T1TO1";

            var responseData = new SortedList<string, string>(new VnPayCompare());
            foreach (var item in query)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Key.StartsWith("vnp_"))
                {
                    responseData.Add(item.Key, item.Value.ToString());
                }
            }

            var vnpSecureHash = responseData.ContainsKey("vnp_SecureHash") ? responseData["vnp_SecureHash"] : string.Empty;
            responseData.Remove("vnp_SecureHash");
            responseData.Remove("vnp_SecureHashType");

            var data = new StringBuilder();
            foreach (var kv in responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            var rawData = data.ToString().TrimEnd('&');
            var calculatedHash = HmacSha512(hashSecret, rawData);

            var isValidSignature = calculatedHash.Equals(vnpSecureHash, StringComparison.InvariantCultureIgnoreCase);

            var responseCode = query["vnp_ResponseCode"].ToString();
            var transactionNo = query["vnp_TransactionNo"].ToString();
            var bankCode = query["vnp_BankCode"].ToString();
            var amountStr = query["vnp_Amount"].ToString();
            var txnRef = query["vnp_TxnRef"].ToString();
            var payDateStr = query["vnp_PayDate"].ToString();

            decimal amount = 0;
            if (long.TryParse(amountStr, out var rawAmount))
            {
                amount = rawAmount / 100m;
            }

            // Extract bookingId from txnRef (formatted as {bookingId}_{timestamp})
            var bookingIdStr = txnRef.Contains('_') ? txnRef.Split('_')[0] : txnRef;
            Guid.TryParse(bookingIdStr, out var bookingId);

            DateTime? payDate = null;
            if (DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                payDate = parsedDate;
            }

            if (!isValidSignature)
            {
                return new VnPayCallbackResult
                {
                    IsSuccess = false,
                    Message = "Invalid signature from VNPay.",
                    BookingId = bookingId,
                    Amount = amount,
                    ResponseCode = responseCode,
                    TransactionNo = transactionNo,
                    BankCode = bankCode,
                    PayDate = payDate
                };
            }

            var isSuccess = responseCode == "00";
            var message = isSuccess
                ? "VNPay payment successful."
                : $"VNPay payment failed with response code {responseCode}.";

            return new VnPayCallbackResult
            {
                IsSuccess = isSuccess,
                Message = message,
                BookingId = bookingId,
                Amount = amount,
                ResponseCode = responseCode,
                TransactionNo = transactionNo,
                BankCode = bankCode,
                PayDate = payDate
            };
        }

        private static string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}

