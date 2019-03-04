﻿using System;
using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Represents the default storage to save the captcha tokens.
    /// </summary>
    public class CookieCaptchaStorageProvider : ICaptchaStorageProvider
    {
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ILogger<CookieCaptchaStorageProvider> _logger;
        private const int CookieLifeTimeInMinutes = 7;

        /// <summary>
        /// Represents the storage to save the captcha tokens.
        /// </summary>
        public CookieCaptchaStorageProvider(
            ICaptchaProtectionProvider captchaProtectionProvider,
            ILogger<CookieCaptchaStorageProvider> logger)
        {
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            logger.CheckArgumentNull(nameof(logger));

            _captchaProtectionProvider = captchaProtectionProvider;
            _logger = logger;
            
            _logger.LogInformation("Using the CookieCaptchaStorageProvider.");
        }

        /// <summary>
        /// Adds the specified token and its value to the storage.
        /// </summary>
        public void Add(HttpContext context, string token, string value)
        {
            value = _captchaProtectionProvider.Encrypt($"{value}{context.GetSalt(_captchaProtectionProvider)}");
            context.Response.Cookies.Append(token, value, getCookieOptions(context));
        }

        /// <summary>
        /// Determines whether the <see cref="ICaptchaStorageProvider" /> contains a specific token.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token">The specified token.</param>
        /// <returns>
        /// <c>True</c> if the value is found in the <see cref="ICaptchaStorageProvider" />; otherwise <c>false</c>.
        /// </returns>
        public bool Contains(HttpContext context, string token)
        {
            return context.Request.Cookies.ContainsKey(token);
        }

        /// <summary>
        /// Gets the value associated with the specified token.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token">The specified token.</param>
        public string GetValue(HttpContext context, string token)
        {
            if (!context.Request.Cookies.TryGetValue(token, out var cookieValue))
            {
                _logger.LogInformation("Couldn't find the captcha cookie in the request.");
                return null;
            }

            //Geter can't change anything
            //Remove(context, token);

            var decryptedValue = _captchaProtectionProvider.Decrypt(cookieValue);
            return decryptedValue?.Replace(context.GetSalt(_captchaProtectionProvider), string.Empty);
        }

        /// <summary>
        /// Removes the specified token from the storage.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token">The specified token.</param>
        public void Remove(HttpContext context, string token)
        {
            if (Contains(context, token))
            {
                context.Response.Cookies.Delete(token, getCookieOptions(context));
            }
        }

        /// <summary>
        /// Expires at the end of the browser's session.
        /// </summary>
        private CookieOptions getCookieOptions(HttpContext context)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Path = context.Request.PathBase.HasValue ? context.Request.PathBase.ToString() : "/",
                Secure = context.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddMinutes(CookieLifeTimeInMinutes),
            };
        }
    }
}