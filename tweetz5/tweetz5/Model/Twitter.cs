﻿// Copyright (c) 2013 Blue Onion Software - All rights reserved

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable AssignNullToNotNullAttribute

namespace tweetz5.Model
{
    public class Twitter
    {
        private static string Get(string url, IEnumerable<string[]> parameters)
        {
            return WebRequest(url, parameters);
        }

        private static string Post(string url, IEnumerable<string[]> parameters)
        {
            return WebRequest(url, parameters, true);
        }

        private static string WebRequest(string url, IEnumerable<string[]> parameters, bool post = false)
        {
            var oauth = new OAuth();
            var nonce = OAuth.Nonce();
            var timestamp = OAuth.TimeStamp();
            var signature = OAuth.Signature(post ? "POST" : "GET", url, nonce, timestamp, oauth.AccessToken, oauth.AccessTokenSecret, parameters);
            var authorizeHeader = OAuth.AuthorizationHeader(nonce, timestamp, oauth.AccessToken, signature);
            var parameterStrings = parameters.Select(p => string.Format("{0}={1}", OAuth.UrlEncode(p[0]), OAuth.UrlEncode(p[1]))).ToList();
            if (!post) url += "?" + string.Join("&", parameterStrings);

            var request = WebRequestWrapper.Create(new Uri(url));
            request.Headers.Add("Authorization", authorizeHeader);
            request.Method = post ? "POST" : "GET";

            if (post)
            {
                request.ContentType = "application/x-www-form-urlencoded";
                if (parameters != null)
                {
                    var buffer = Encoding.ASCII.GetBytes(string.Join("&", parameterStrings));
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(buffer, 0, buffer.Length);
                    }
                }
            }

            using (var response = request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var result = stream.ReadToEnd();
                    return result;
                }
            }
        }

        public Status[] HomeTimeline(ulong sinceId)
        {
            var parameters = new[]
            {
                new[] {"count", "50"},
                new[] {"include_rts", "true"},
                new[] {"include_entities", "true"},
                new[] {"since_id", sinceId.ToString(CultureInfo.InvariantCulture)}
            };
            return GetTimeline("https://api.twitter.com/1.1/statuses/home_timeline.json", parameters);
        }

        public Status[] MentionsTimeline(ulong sinceId)
        {
            var parameters = new[]
            {
                new[] {"count", "50"},
                new[] {"include_entities", "true"},
                new[] {"since_id", sinceId.ToString(CultureInfo.InvariantCulture)}
            };
            return GetTimeline("https://api.twitter.com/1.1/statuses/mentions_timeline.json", parameters);
        }

        public Status[] DirectMessagesTimeline(ulong sinceId)
        {
            var parameters = new[]
            {
                new[] {"include_entities", "true"},
                new[] {"since_id", sinceId.ToString(CultureInfo.InvariantCulture)}
            };
            return GetTimeline("https://api.twitter.com/1.1/direct_messages.json", parameters);
        }

        public Status[] FavoritesTimeline(ulong sinceId)
        {
            var parameters = new[]
            {
                new[] {"count", "50"},
                new[] {"since_id", sinceId.ToString(CultureInfo.InvariantCulture)}
            };
            return GetTimeline("https://api.twitter.com/1.1/favorites/list.json", parameters);
        }

        private static Status[] GetTimeline(string url, IEnumerable<string[]> parameters)
        {
            try
            {
                var json = Get(url, parameters);
                var statuses = Status.ParseJson(json);
                return statuses;
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                return new Status[0];
            }
        }

        public static string UpdateStatus(string message, string replyToStatusId = null)
        {
            var parameters = string.IsNullOrWhiteSpace(replyToStatusId)
                                 ? new[] {new[] {"status", message}}
                                 : new[] {new[] {"status", message}, new[] {"in_reply_to_status_id", replyToStatusId}};

            try
            {
                return Post("https://api.twitter.com/1.1/statuses/update.json", parameters);
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public static string CreateFavorite(string id)
        {
            try
            {
                var parameters = new[] {new[] {"id", id}};
                return Post("https://api.twitter.com/1.1/favorites/create.json", parameters);
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public static string DestroyFavorite(string id)
        {
            try
            {
                var parameters = new[] {new[] {"id", id}};
                return Post("https://api.twitter.com/1.1/favorites/destroy.json", parameters);
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public static string RetweetStatus(string id)
        {
            try
            {
                return Post(string.Format("https://api.twitter.com/1.1/statuses/retweet/{0}.json", id), new string[0][]);
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public static string DestroyStatus(string id)
        {
            try
            {
                return Post(string.Format("https://api.twitter.com/1.1/statuses/destroy/{0}.json", id), new string[0][]);
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public static string GetTweet(string id)
        {
            var parameters = new[]
            {
                new[] {"id", id},
                new[] {"include_my_retweet", "true"}
            };
            try
            {
                return Get("https://api.twitter.com/1.1/statuses/show.json", parameters);
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public static User GetUserInformation(string screenName)
        {
            try
            {
                var parameters = new[]
                {
                    new[] {"screen_name", screenName},
                    new[] {"include_entities", "true"}
                };
                var json = Get("https://api.twitter.com/1.1/users/show.json", parameters);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var serializer = new DataContractJsonSerializer(typeof (User));
                    return (User) serializer.ReadObject(stream);
                }
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return new User {Name = "Error!"};
            }
        }

        public static bool Following(string screenName)
        {
            try
            {
                var parameters = new[] {new[] {"screen_name", screenName}};
                var json = Get("https://api.twitter.com/1.1/friendships/lookup.json", parameters);
                return json.Contains("\"following\"");
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool Follow(string screenName)
        {
            try
            {
                var parameters = new[]
                {
                    new[] {"screen_name", screenName},
                    new[] {"following", "true"}
                };
                try
                {
                    var json = Post("https://api.twitter.com/1.1/friendships/create.json", parameters);
                    return json.Contains(screenName);
                }
                catch (WebException e)
                {
                    ShowAlert(e.Message);
                    return false;
                }
            }
            catch (Exception e)
            {
                ShowAlert(e.Message);
                return false;
            }
        }

        public static bool Unfollow(string screenName)
        {
            try
            {
                var parameters = new[] {new[] {"screen_name", screenName}};
                var json = Post("https://api.twitter.com/1.1/friendships/destroy.json", parameters);
                return json.Contains(screenName);
            }
            catch (Exception e)
            {
                ShowAlert(e.Message);
                return false;
            }
        }

        public static string SendDirectMessage(string screenName, string text)
        {
            try
            {
                var parameters = new[]
                {
                    new[] { "screen_name", screenName },
                    new[] { "text", text}
                };

                var json = Post("https://api.twitter.com/1.1/direct_messages/new.json", parameters);
                return json;
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public static string Search(string query, string sinceId = "1")
        {
            var parameters = new[]
            {
                new[] {"q", query},
                new[] {"count", "50"},
                new[] {"since_id", sinceId}
            };
            try
            {
                return Get("https://api.twitter.com/1.1/search/tweets.json", parameters);
            }
            catch (WebException e)
            {
                ShowAlert(e.Message);
                return string.Empty;
            }
        }

        public class OAuthTokens
        {
            public string OAuthToken { get; set; }
            public string OAuthSecret { get; set; }
            public string UserId { get; set; }
            public string ScreenName { get; set; }
        }

        public static OAuthTokens GetRequestToken()
        {
            const string requestTokenUrl = "https://api.twitter.com/oauth/request_token";
            var nonce = OAuth.Nonce();
            var timestamp = OAuth.TimeStamp();
            var parameters = new[] {new[] {"oauth_callback", "oob"}};
            var signature = OAuth.Signature("POST", requestTokenUrl, nonce, timestamp, "", "", parameters);
            var authorizationHeader = OAuth.AuthorizationHeader(nonce, timestamp, null, signature, parameters);

            var request = System.Net.WebRequest.Create(requestTokenUrl);
            request.Method = "POST";
            request.Headers.Add("Authorization", authorizationHeader);
            using (var response = request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var body = stream.ReadToEnd();
                    var tokens = body.Split('&');
                    var oauthToken = Token(tokens[0]);
                    var oauthSecret = Token(tokens[1]);
                    var callbackConfirmed = Token(tokens[2]);

                    if (callbackConfirmed != "true") throw new InvalidProgramException("callback token not confirmed");
                    return new OAuthTokens {OAuthToken = oauthToken, OAuthSecret = oauthSecret};
                }
            }
        }

        public static OAuthTokens GetAccessToken(string accessToken, string accessTokenSecret, string oauthVerifier)
        {
            const string requestTokenUrl = "https://api.twitter.com/oauth/access_token";
            var nonce = OAuth.Nonce();
            var timestamp = OAuth.TimeStamp();
            var parameters = new[] {new[] {"oauth_verifier", oauthVerifier}};
            var signature = OAuth.Signature("POST", requestTokenUrl, nonce, timestamp, accessToken, accessTokenSecret, parameters);
            var authorizationHeader = OAuth.AuthorizationHeader(nonce, timestamp, accessToken, signature, parameters);

            var request = System.Net.WebRequest.Create(requestTokenUrl);
            request.Method = "POST";
            request.Headers.Add("Authorization", authorizationHeader);

            using (var response = request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var tokens = stream.ReadToEnd().Split('&');
                    var oauthTokens = new OAuthTokens
                    {
                        OAuthToken = Token(tokens[0]),
                        OAuthSecret = Token(tokens[1]),
                        UserId = Token(tokens[2]),
                        ScreenName = Token(tokens[3])
                    };
                    return oauthTokens;
                }
            }
        }

        public static string UpdateStatusWithMedia(string message, byte[] media, string mediaName)
        {
            const string url = "https://api.twitter.com/1.1/statuses/update_with_media.json";
            var oauth = new OAuth();
            var nonce = OAuth.Nonce();
            var timestamp = OAuth.TimeStamp();
            var signature = OAuth.Signature("POST", url, nonce, timestamp, oauth.AccessToken, oauth.AccessTokenSecret, new string[0][]);
            var authorizeHeader = OAuth.AuthorizationHeader(nonce, timestamp, oauth.AccessToken, signature);

            var request = WebRequestWrapper.Create(new Uri(url));
            request.Headers.Add("Authorization", authorizeHeader);
            request.Method = "POST";

            var formDataBoundary = String.Format("{0:N}", Guid.NewGuid());
            var contentType = "multipart/form-data; boundary=" + formDataBoundary;
            request.ContentType = contentType;

            using (var requestStream = request.GetRequestStream())
            {
                var header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n", formDataBoundary, "status");
                var footer = string.Format("\r\n--{0}--\r\n", formDataBoundary);
                WriteStream(requestStream, header);
                WriteStream(requestStream, message);
                WriteStream(requestStream, footer);

                header = string.Format(
                    "--{0}\r\nContent-Type: application/octet-stream\r\n" +
                    "Content-Disposition: form-data; name=\"media[]\"; filename=\"{1}\"\r\n\r\n",
                    formDataBoundary, mediaName);
                WriteStream(requestStream, header);
                requestStream.Write(media, 0, media.Length);
                WriteStream(requestStream, footer);
            }

            using (var response = request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var result = stream.ReadToEnd();
                    return result;
                }
            }
        }

        private static void WriteStream(Stream stream, string text)
        {
            var buffer = Encoding.ASCII.GetBytes(text);
            stream.Write(buffer, 0, buffer.Length);
        }

        private static string Token(string pair)
        {
            return pair.Substring(pair.IndexOf('=') + 1);
        }

        private static void ShowAlert(string message)
        {
            Application.Current.Dispatcher.Invoke(() => MainWindow.AlertCommand.Execute(message, Application.Current.MainWindow));
        }
    }
}