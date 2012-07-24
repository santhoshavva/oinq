﻿using System;
using Oinq;

namespace Oinq.EdgeSpring
{
    /// <summary>
    /// Represents an EdgeSpring EdgeMart and the settings used to access it.
    /// </summary>
    public abstract class EdgeMart : IDataFile
    {
        // private fields
        private Url _url;

        // constructors
        /// <summary>
        /// Creates a new member of EdgeMart.
        /// </summary>
        /// <param name="url">Server and EdgeMart settings in the form of a Url.</param>
        protected EdgeMart(Url url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("server");
            }
            if (String.IsNullOrEmpty(url.EdgeMartName))
            {
                throw new ArgumentException("The name of the EdgeMart is required.");
            }
            _url = url;
        }

        /// <summary>
        /// Creates a new member of EdgeMart.
        /// </summary>
        /// <param name="connectionString">Server and EdgeMart settings in the form of a connection string.</param>
        protected EdgeMart (String connectionString)
            : this(Url.Create(connectionString))
        {
        }

        /// <summary>
        /// Creates a new member of EdgeMart.
        /// </summary>
        /// <param name="uri">Server and EdgeMart settings in the form of a Uri.</param>
        protected EdgeMart (Uri uri)
            : this (Url.Create(uri.ToString()))
        {
        }

        // public properties
        /// <summary>
        /// Gets the name of this EdgeMart.
        /// </summary>
        public virtual String Name
        {
            get { return _url.EdgeMartName; }
        }

        /// <summary>
        /// Gets the path to the EdgeMart.
        /// </summary>
        public virtual string AbsolutePath
        {
            get { return _url.EdgeMartName; }
        }

        /// <summary>
        /// Gets the url string for this EdgeMart.
        /// </summary>
        public virtual ServerAddress Server
        {
            get { return _url.Server; }
        }

        /// <summary>
        /// Gets the Uri for the server hosting the EdgeMart.
        /// </summary>
        public virtual Uri AbsoluteUri
        {
            get { return new Uri(String.Format("http://{0}:{1}/remote", Server.Host, Server.Port)); }
        }

        /// <summary>
        /// Gets the url string for this EdgeMart.
        /// </summary>
        public virtual String UrlString
        {
            get { return _url.ToString(); }
        }
    }

    /// <summary>
    /// Represents an EdgeMart and the default record type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EdgeMart<T> : EdgeMart
    {
        /// <summary>
        /// Creates a new member of EdgeMart.
        /// </summary>
        /// <param name="url">Server and EdgeMart settings in the form of a Url.</param>
        public EdgeMart(Url url)
            : base(url)
        {
        }

        /// <summary>
        /// Creates a new member of EdgeMart.
        /// </summary>
        /// <param name="connectionString">Server and EdgeMart settings in the form of a connection string.</param>
        public EdgeMart(String connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Creates a new member of EdgeMart.
        /// </summary>
        /// <param name="uri">Server and EdgeMart settings in the form of a Uri.</param>
        public EdgeMart(Uri uri)
            : base(uri)
        {
        }

        /// <summary>
        /// Creates a new EdgeMart.
        /// </summary>
        /// <param name="baseConnectionString">Base connection string.</param>
        /// <returns>EdgeMart{{T}}</returns>
        public static EdgeMart<T> Create(String baseConnectionString)
        {
            if (baseConnectionString.IndexOf("edgemart", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return new EdgeMart<T>(baseConnectionString);
            }

            String martName;
            var objectType = typeof(T);
            var sourceAttributes = objectType.GetCustomAttributes(typeof(PigSourceMapping), true);
            if (sourceAttributes.Length > 0)
            {
                martName = ((PigSourceMapping)sourceAttributes[0]).Path;
            }
            else
            {
                martName = objectType.Name;
            }
            return new EdgeMart<T>(baseConnectionString + "?edgemart=" +martName);
        }
    }
}