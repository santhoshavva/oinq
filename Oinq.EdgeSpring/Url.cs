﻿using System;
using System.Collections.Generic;

namespace Oinq.EdgeSpring
{
    /// <summary>
    /// Represents a URL to an EdgeSpring server.
    /// </summary>
    public class Url : IEquatable<Url>
    {
        // private static fields
        private static readonly object StaticLock = new Object();
        private static readonly Dictionary<String, Url> Cache = new Dictionary<string, Url>();

        // private fields
        private readonly String _url;

        // constructor
        /// <summary>
        /// Initializes a new Url.
        /// </summary>
        /// <param name="url">String containing the url.</param>
        public Url(String url)
        {
            var builder = new UrlBuilder(url);
            EdgeMartName = builder.EdgeMartName;
            Server = builder.Server;
            _url = builder.ToString();
        }

        /// <summary>
        /// Gets the name of the EdgeMart.
        /// </summary>
        public String EdgeMartName { get; private set; }

        /// <summary>
        /// Gets the address of the server.
        /// </summary>
        public ServerAddress Server { get; private set; }

        #region IEquatable<Url> Members

        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are equal.</returns>
        public Boolean Equals(Url rhs)
        {
            if (ReferenceEquals(rhs, null) || GetType() != rhs.GetType())
            {
                return false;
            }
            return _url == rhs._url; // this works because URL is in canonical form
        }

        #endregion

        // public operators
        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="lhs">The first URL.</param>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are equal (or both null).</returns>
        public static Boolean operator ==(Url lhs, Url rhs)
        {
            return Equals(lhs, rhs);
        }

        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="lhs">The first URL.</param>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are not equal (or one is null and the other is not).</returns>
        public static Boolean operator !=(Url lhs, Url rhs)
        {
            return !(lhs == rhs);
        }

        // public static methods
        /// <summary>
        /// Clears the URL cache. When a URL is parsed it is stored in the cache so that it doesn't have to be
        /// parsed again. There is rarely a need to call this method.
        /// </summary>
        public static void ClearCache()
        {
            Cache.Clear();
        }

        /// <summary>
        /// Creates an member of Url (might be an existing member if the same URL has been used before).
        /// </summary>
        /// <param name="url">The URL containing the settings.</param>
        /// <returns>An member of Url.</returns>
        public static Url Create(String url)
        {
            // cache previously seen urls to avoid repeated parsing
            lock (StaticLock)
            {
                Url esUrl;
                if (!Cache.TryGetValue(url, out esUrl))
                {
                    esUrl = new Url(url);
                    string canonicalUrl = esUrl.ToString();
                    if (canonicalUrl != url)
                    {
                        if (Cache.ContainsKey(canonicalUrl))
                        {
                            esUrl = Cache[canonicalUrl]; // use existing Url
                        }
                        else
                        {
                            Cache[canonicalUrl] = esUrl; // cache under canonicalUrl also
                        }
                    }
                    Cache[url] = esUrl;
                }
                return esUrl;
            }
        }

        // public methods

        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="obj">The other URL.</param>
        /// <returns>True if the two URLs are equal.</returns>
        public override Boolean Equals(Object obj)
        {
            return Equals(obj as Url); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override Int32 GetHashCode()
        {
            return _url.GetHashCode(); // this works because URL is in canonical form
        }

        /// <summary>
        /// Returns the canonical URL based on the settings in this UrlBuilder.
        /// </summary>
        /// <returns>The canonical URL.</returns>
        public override String ToString()
        {
            return _url;
        }
    }
}