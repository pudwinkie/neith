using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.CoreLibrary;
using my = Neith.Growl.Connector;

namespace Neith.Growl.Connector
{
    /// <summary>Header関係の拡張</summary>
    public static class HeaderExtensions
    {
        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this IApplication obj)
        {
            var headers = new HeaderCollection();
            obj.AddIconAttributesToHeaders(headers);
            obj.AddInheritedAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this INotification obj)
        {
            var headers = new HeaderCollection();
            obj.AddNotificationAttributesToHeaders(headers);
            obj.AddIconAttributesToHeaders(headers);
            obj.AddInheritedAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this INotificationType obj)
        {
            var headers = new HeaderCollection();
            obj.AddNotificationTypeAttributesToHeaders(headers);
            obj.AddIconAttributesToHeaders(headers);
            obj.AddCustomAttributesToHeaders(headers);
            return headers;
        }


        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this IDictionary<string, string> obj)
        {
            var headers = new HeaderCollection();
            obj.AddDictionaryAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this IResponse obj)
        {
            var headers = new HeaderCollection();
            obj.AddResponseAttributesToHeaders(headers);
            obj.AddInheritedAttributesToHeaders(headers);
            return headers;
        }


        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this IError obj)
        {
            var headers = new HeaderCollection();
            obj.AddErrorAttributesToHeaders(headers);
            obj.AddInheritedAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// IResponse情報をヘッダに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers"></param>
        private static void AddResponseAttributesToHeaders(this IResponse obj, HeaderCollection headers)
        {
            if (obj.IsOK && !obj.IsCallback) {
                var hResponseAction = new Header(HeaderKeys.RESPONSE_ACTION, obj.InResponseTo);
                headers.AddHeader(hResponseAction);
            }

            if (obj.IsError) {
                var hErrorCode = new Header(HeaderKeys.ERROR_CODE, obj.ErrorCode.ToString());
                var hDescription = new Header(HeaderKeys.ERROR_DESCRIPTION, obj.ErrorDescription);
                headers.AddHeader(hErrorCode);
                headers.AddHeader(hDescription);
            }

            if (obj.IsCallback) {
                var hNotificationID = new Header(HeaderKeys.NOTIFICATION_ID, obj.CallbackData.NotificationID);
                var hCallbackResult = new Header(HeaderKeys.NOTIFICATION_CALLBACK_RESULT, Enum.GetName(typeof(CallbackResult), obj.CallbackData.Result));
                var hCallbackContext = new Header(HeaderKeys.NOTIFICATION_CALLBACK_CONTEXT, obj.CallbackData.Data);
                var hCallbackContextType = new Header(HeaderKeys.NOTIFICATION_CALLBACK_CONTEXT_TYPE, obj.CallbackData.Type);
                var hCallbackTimestamp = new Header(HeaderKeys.NOTIFICATION_CALLBACK_TIMESTAMP, DateTime.UtcNow.ToString("u"));
                headers.AddHeader(hNotificationID);
                headers.AddHeader(hCallbackResult);
                headers.AddHeader(hCallbackContext);
                headers.AddHeader(hCallbackContextType);
                headers.AddHeader(hCallbackTimestamp);
            }
        }

        /// <summary>
        /// IError情報をヘッダに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers"></param>
        private static void AddErrorAttributesToHeaders(this IError obj, HeaderCollection headers)
        {
            var hErrorCode = new Header(HeaderKeys.ERROR_CODE, obj.ErrorCode.ToString());
            var hDescription = new Header(HeaderKeys.ERROR_DESCRIPTION, obj.ErrorDescription);
            headers.AddHeader(hErrorCode);
            headers.AddHeader(hDescription);
        }

        /// <summary>
        /// IDictionary of (string, string)情報をヘッダに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers"></param>
        private static void AddDictionaryAttributesToHeaders(this IDictionary<string, string> obj, HeaderCollection headers)
        {
            if (headers != null) {
                foreach (var item in obj) {
                    var dataHeader = new DataHeader(item.Key, item.Value);
                    headers.AddHeader(dataHeader);
                }
            }
        }




        /// <summary>
        /// INotificationType情報をヘッダに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers"></param>
        private static void AddNotificationTypeAttributesToHeaders(this INotificationType obj, HeaderCollection headers)
        {
            var hDisplayName = new Header(HeaderKeys.NOTIFICATION_DISPLAY_NAME, obj.DisplayName);
            var hEnabled = new Header(HeaderKeys.NOTIFICATION_ENABLED, obj.Enabled.ToString());

            if (obj.DisplayName != null)
                headers.AddHeader(hDisplayName);
            headers.AddHeader(hEnabled);
        }

        /// <summary>
        /// INotification情報をヘッダに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers"></param>
        private static void AddNotificationAttributesToHeaders(this INotification obj, HeaderCollection headers)
        {
            var hAppName = new Header(HeaderKeys.APPLICATION_NAME, obj.ApplicationName);
            var hID = new Header(HeaderKeys.NOTIFICATION_ID, obj.ID);
            var hTitle = new Header(HeaderKeys.NOTIFICATION_TITLE, obj.Title);
            var hText = new Header(HeaderKeys.NOTIFICATION_TEXT, obj.Text);
            var hSticky = new Header(HeaderKeys.NOTIFICATION_STICKY, obj.Sticky);
            var hPriority = new Header(HeaderKeys.NOTIFICATION_PRIORITY, ((int)obj.Priority).ToString());
            var hCoalescingID = new Header(HeaderKeys.NOTIFICATION_COALESCING_ID, obj.CoalescingID);

            headers.AddHeader(hAppName);
            headers.AddHeader(hID);
            headers.AddHeader(hTitle);
            headers.AddHeader(hText);
            headers.AddHeader(hSticky);
            headers.AddHeader(hPriority);
            headers.AddHeader(hCoalescingID);
        }

        /// <summary>
        /// IIcon情報をヘッダに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers"></param>
        private static void AddIconAttributesToHeaders(this IIcon obj, HeaderCollection headers)
        {
            var hName = new Header(HeaderKeys.NOTIFICATION_NAME, obj.Name);
            var hIcon = new Header(HeaderKeys.NOTIFICATION_ICON, obj.Icon);
            headers.AddHeader(hName);
            if (obj.Icon != null && obj.Icon.IsSet) {
                headers.AddHeader(hIcon);
                headers.AssociateBinaryData(obj.Icon);
            }
        }

        /// <summary>
        /// Adds any inherited headers to the end of the header collection
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers">The <see cref="HeaderCollection"/> to append the headers to</param>
        /// <remarks>
        /// This method should only be called from a derived class' .ToHeaders() method.
        /// It takes care of adding the Origin-* headers as well as any X-* custom headers.
        /// 
        /// This method is the same as calling both AddCommonAttributesToHeaders and 
        /// AddCustomAttributesToHeaders.
        /// </remarks>
        public static void AddInheritedAttributesToHeaders(this IExtensibleObject obj, HeaderCollection headers)
        {
            obj.AddCommonAttributesToHeaders(headers);
            obj.AddCustomAttributesToHeaders(headers);
        }

        /// <summary>
        /// When converting an <see cref="ExtensibleObject"/> to a list of headers,
        /// this method adds the common attributes to the list of headers.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers">The <see cref="HeaderCollection"/> to add the custom headers to</param>
        public static void AddCommonAttributesToHeaders(this IExtensibleObject obj, HeaderCollection headers)
        {
            if (headers != null) {
                //Header hRequestID = new Header("RequestID", requestID);
                var hMachineName = new Header(HeaderKeys.ORIGIN_MACHINE_NAME, obj.MachineName);
                var hSoftwareName = new Header(HeaderKeys.ORIGIN_SOFTWARE_NAME, obj.SoftwareName);
                var hSoftwareVersion = new Header(HeaderKeys.ORIGIN_SOFTWARE_VERSION, obj.SoftwareVersion);
                var hPlatformName = new Header(HeaderKeys.ORIGIN_PLATFORM_NAME, obj.PlatformName);
                var hPlatformVersion = new Header(HeaderKeys.ORIGIN_PLATFORM_VERSION, obj.PlatformVersion);

                //headers.Add(hRequestID);
                headers.AddHeader(hMachineName);
                headers.AddHeader(hSoftwareName);
                headers.AddHeader(hSoftwareVersion);
                headers.AddHeader(hPlatformName);
                headers.AddHeader(hPlatformVersion);
            }
        }

        /// <summary>
        /// When converting an <see cref="ExtensibleObject"/> to a list of headers,
        /// this method adds the custom attributes (both text and binary) to the
        /// list of headers.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers">The <see cref="HeaderCollection"/> to add the custom attributes to</param>
        public static void AddCustomAttributesToHeaders(this IExtensibleObject obj, HeaderCollection headers)
        {
            if (headers != null) {
                foreach (var pair in obj.CustomTextAttributes) {
                    var customHeader = new CustomHeader(pair.Key, pair.Value);
                    headers.AddHeader(customHeader);
                }
                foreach (var pair in obj.CustomBinaryAttributes) {
                    var customHeader = new CustomHeader(pair.Key, pair.Value.ToString());
                    headers.AddHeader(customHeader);
                    headers.AssociateBinaryData(pair.Value);
                }
            }
        }

        /// <summary>
        /// Sets any properties from a collection of header values
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="headers">The <see cref="HeaderCollection"/> of header values</param>
        /// <param name="isCallback">Indicates if this is a callback response</param>
        public static void SetAttributesFromHeaders(this IResponse obj, HeaderCollection headers, bool isCallback)
        {
            if (isCallback) {
                var callbackData = my::CallbackData.FromHeaders(headers);
                obj.CallbackData = callbackData;
            }

            obj.RequestData = RequestData.FromHeaders(headers);

            obj.SetInhertiedAttributesFromHeaders(headers);
        }


        /// <summary>
        /// Sets the object's base class properties from the supplied header list
        /// </summary>
        /// <param name="obj">The <see cref="ExtensibleObject"/> being rehydrated</param>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the parsed header values</param>
        /// <remarks>
        /// This method should only be called from a derived class' .FromHeaders() method.
        /// It takes care of setting the Origin-* related properties, as well as any custom attributes.
        /// 
        /// This method is the same as calling both SetCommonAttributesFromHeaders and
        /// SetCustomAttributesFromHeaders.
        /// </remarks>
        public static void SetInhertiedAttributesFromHeaders(this IExtensibleObject obj, HeaderCollection headers)
        {
            obj.SetCommonAttributesFromHeaders(headers);
            obj.SetCustomAttributesFromHeaders(headers);
        }


        /// <summary>
        /// When converting a list of headers to an <see cref="ExtensibleObject"/>, this
        /// method sets the common attributes on the object.
        /// </summary>
        /// <param name="obj">The <see cref="ExtensibleObject"/> to be populated</param>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the list of headers</param>
        public static void SetCommonAttributesFromHeaders(this IExtensibleObject obj, HeaderCollection headers)
        {
            if (obj != null && headers != null) {
                var hMachineName = headers.Get(HeaderKeys.ORIGIN_MACHINE_NAME);
                if (hMachineName != null && !String.IsNullOrEmpty(hMachineName.Value)) obj.MachineName = hMachineName.Value;

                var hSoftwareName = headers.Get(HeaderKeys.ORIGIN_SOFTWARE_NAME);
                if (hSoftwareName != null && !String.IsNullOrEmpty(hSoftwareName.Value)) obj.SoftwareName = hSoftwareName.Value;

                var hSoftwareVersion = headers.Get(HeaderKeys.ORIGIN_SOFTWARE_VERSION);
                if (hSoftwareVersion != null && !String.IsNullOrEmpty(hSoftwareVersion.Value)) obj.SoftwareVersion = hSoftwareVersion.Value;

                var hPlatformName = headers.Get(HeaderKeys.ORIGIN_PLATFORM_NAME);
                if (hPlatformName != null && !String.IsNullOrEmpty(hPlatformName.Value)) obj.PlatformName = hPlatformName.Value;

                var hPlatoformVersion = headers.Get(HeaderKeys.ORIGIN_PLATFORM_VERSION);
                if (hPlatoformVersion != null && !String.IsNullOrEmpty(hPlatoformVersion.Value)) obj.PlatformVersion = hPlatoformVersion.Value;
            }
        }


        /// <summary>
        /// When converting a list of headers to an <see cref="ExtensibleObject"/>, this
        /// method sets the custom attributes (both text and binary) on the object.
        /// </summary>
        /// <param name="obj">The <see cref="ExtensibleObject"/> to be populated</param>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the list of headers</param>
        public static void SetCustomAttributesFromHeaders(this IExtensibleObject obj, HeaderCollection headers)
        {
            if (obj != null && headers != null) {
                foreach (var header in headers.CustomHeaders) {
                    if (header != null) {
                        if (header.IsGrowlResourcePointer) {
                            obj.CustomBinaryAttributes.Add(header.ActualName, header.GrowlResource);
                        }
                        else {
                            obj.CustomTextAttributes.Add(header.ActualName, header.Value);
                        }
                    }
                }
            }
        }



    }
}