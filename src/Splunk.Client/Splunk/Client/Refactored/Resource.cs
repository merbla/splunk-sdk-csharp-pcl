﻿/*
 * Copyright 2014 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

//// TODO:
//// [O] Contracts
//// [O] Documentation
//// [ ] Resource aggregates Endpoint (?)

namespace Splunk.Client.Refactored
{
    using Splunk.Client;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a base class that represents a Splunk resource as an object.
    /// </summary>
    public class Resource : DynamicObject, IResource<Resource>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        /// <param name="entry">
        /// An object representing a Splunk atom entry response.
        /// </param>
        /// <param name="generatorVersion">
        /// The version of the generator producing the <see cref="AtomFeed"/>
        /// feed containing <paramref name="entry"/>.
        /// </param>
        protected internal Resource(AtomEntry entry, Version generatorVersion)
        {
            this.Initialize(entry, generatorVersion);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        /// <param name="feed">
        /// An object representing a Splunk atom feed response.
        /// </param>
        protected internal Resource(AtomFeed feed)
        {
            this.Initialize(feed);
        }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the <see cref=
        /// "Resource"/> class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code. 
        /// </remarks>
        public Resource()
        { }

        #endregion

        #region Properties backed by a Snapshot

        /// <summary>
        /// Gets the author of the current <see cref="Resource"/>.
        /// </summary>
        public string Author
        { get; private set; }

        /// <summary>
        /// Gets the version of the Atom Feed generator that produced the
        /// current <see cref="Resource"/>.
        /// </summary>
        public Version GeneratorVersion
        { get; private set; }

        /// <summary>
        /// Gets the Splunk management URI for accessing the current <see cref=
        /// "Resource"/>.
        /// </summary>
        public Uri Id
        { get; private set; }

        /// <summary>
        /// Gets the collection of service addresses supported by the current
        /// <see cref="Resource"/>.
        /// </summary>
        public IReadOnlyDictionary<string, Uri> Links
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title
        { get; private set; }

        /// <summary>
        /// Gets the date that the interface to this resource type was introduced
        /// or updated by Splunk.
        /// </summary>
        public DateTime Updated
        { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously creates a <see cref="Resource"/> from a Splunk atom
        /// feed <see cref="Response"/>.
        /// </summary>
        /// <typeparam name="TResource">
        /// The type of <see cref="Resource"/> to be created.
        /// </typeparam>
        /// <param name="response">
        /// An object representing a Splunk atom feed response.
        /// </param>
        /// <returns>
        /// The <see cref="Resource"/> created.
        /// </returns>
        internal static async Task<TResource> CreateAsync<TResource>(Response response) where TResource : Resource, new()
        {
            var feed = new AtomFeed();

            await feed.ReadXmlAsync(response.XmlReader);
            var resource = new TResource();
            resource.Initialize(feed);

            return resource;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Resource"/> refers to 
        /// the same resource as the current one.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Resource"/> to compare with the current one.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the two instances represent the same
        /// <see cref="Resource"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            return this.Equals(other as Resource);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Resource"/> refers to 
        /// the same resource as the current one.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Resource"/> to compare with the current one.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the two instances represent the same
        /// <see cref="Resource"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Resource other)
        {
            if (other == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            bool result = this.Id.Equals(other.Id);
            return result;
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="Resource"/>.
        /// </summary>
        /// <returns>
        /// Hash code for the current <see cref="Resource"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// The list of dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ((IDictionary<string, object>)(this.adapter.ExpandoObject)).Keys;
        }

        /// <summary>
        /// Gets a property value from the current <see cref="Resoure"/>.
        /// </summary>
        /// <param name="name">
        /// The name of a property.
        /// </param>
        /// <returns>
        /// The value or <c>null</c>, if <paramref name=""/> does not exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Use this method to create static properties from the dynamic 
        /// properties exposed by the current <see cref="Resource"/>.
        /// </remarks>
        public dynamic GetValue(string name)
        {
            return this.adapter.GetValue(name);
        }

        /// <summary>
        /// Gets a converted property value from the current <see cref="Resource"/>
        /// </summary>
        /// <param name="name">
        /// The name of a property.
        /// </param>
        /// <param name="valueConverter">
        /// A value converter for converting property <paramref name="name"/>.
        /// </param>
        /// <returns>
        /// The converted value or <paramref name="valueConverter"/><c>.DefaultValue</c>,
        /// if <paramref name="name"/> does not exist.
        /// </returns>
        /// <exception cref="InvalidDataException">
        /// The conversion failed.
        /// </exception>
        /// <remarks>
        /// Use this method to create static properties from the dynamic 
        /// properties exposed by the <see cref="CurrentSnapshot"/>.
        /// </remarks>
        public TValue GetValue<TValue>(string name, ValueConverter<TValue> valueConverter)
        {
            return this.adapter.GetValue(name, valueConverter);
        }

        /// <summary>
        /// Infrastructure. Initializes the current uninitialized <see cref=
        /// "Resource"/>.
        /// class.
        /// </summary>
        /// <param name="entry">
        /// An object representing a Splunk atom entry response.
        /// </param>
        /// <param name="generatorVersion">
        /// The version of the generator producing the <see cref="AtomFeed"/>
        /// feed containing <paramref name="entry"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/>, <paramref name="entry"/>, or <paramref 
        /// name="generatorVersion"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current <see cref="Resource"/> is already initialized.
        /// </exception>
        /// <remarks>
        /// This method may be called once to intialize a <see cref="Resource"/>
        /// instantiated by the default constructor. Override this method to 
        /// provide special initialization code. Call this base method before 
        /// initialization is complete. 
        /// <note type="note">
        /// This method supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </note>
        /// </remarks>
        protected internal virtual void Initialize(AtomEntry entry, Version generatorVersion)
        {
            Contract.Requires<ArgumentNullException>(entry != null);
            Contract.Requires<ArgumentNullException>(generatorVersion != null);

            this.EnsureUninitialized();
            dynamic content;

            if (entry.Content == null)
            {
                content = new ExpandoObject();
            }
            else
            {
                content = entry.Content as ExpandoObject;

                if (content == null)
                {
                    content = new ExpandoObject();
                    content.Value = entry.Content;
                }
            }

            this.adapter = new ExpandoAdapter(content);

            this.Author = entry.Author;
            this.Id = entry.Id;
            this.GeneratorVersion = generatorVersion;
            this.Links = entry.Links;
            this.Title = entry.Title;
            this.Updated = entry.Updated;

            if (entry.Published != DateTime.MinValue)
            {
                content.Published = entry.Published;
            }
        }

        /// <summary>
        /// Infrastructure. Initializes the current uninitialized <see cref=
        /// "Resource"/>.
        /// </summary>
        /// <param name="feed">
        /// An object representing a Splunk atom feed response.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> or <paramref name="feed"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current <see cref="Resource"/> is already initialized.
        /// </exception>
        /// <remarks>
        /// This method may be called once to intialize a <see cref="Resource"/>
        /// instantiated by the default constructor. Override this method to 
        /// provide special initialization code. Call this base method before 
        /// initialization is complete. 
        /// <note type="note">
        /// This method supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </note>
        /// </remarks>
        protected internal virtual void Initialize(AtomFeed feed)
        {
            Contract.Requires<ArgumentNullException>(feed != null);

            this.EnsureUninitialized();

            dynamic content = new ExpandoObject();

            this.adapter = new ExpandoAdapter(content);
            this.Author = feed.Author;
            this.Id = feed.Id;
            this.GeneratorVersion = feed.GeneratorVersion;
            this.Links = feed.Links;
            this.Title = feed.Title;
            this.Updated = feed.Updated;

            content.Messages = feed.Messages;
            content.Pagination = feed.Pagination;

            var resources = new List<Resource>();

            foreach (var entry in feed.Entries)
            {
                var resource = new Resource(entry, feed.GeneratorVersion);
                resources.Add(resource);
            }

            content.Resources = resources;
        }

        /// <summary>
        /// Gets a string identifying the current <see cref="Resource"/>.
        /// </summary>
        /// <returns>
        /// A string representing the identity of the current <see cref=
        /// "Resource"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Id.ToString();
        }

        /// <summary>
        /// Provides the implementation for operations that get dynamic member
        /// values.
        /// </summary>
        /// <param name="binder">
        /// Provides information about the object that called the dynamic 
        /// operation. The <paramref name="binder"/>.Name property provides the
        /// name of the member on which the dynamic operation is performed.
        /// The <paramref name="binder"/>.IgnoreCase property specifies whether
        /// the member name is case-sensitive.
        /// </param>
        /// <param name="result">
        /// The result of the operation.
        /// </param>
        /// <returns>
        /// <c>true</c> if the operation is successful; otherwise, <c>false</c>.
        /// If this method returns <c>false</c>, the run-time binder of the 
        /// language determines the behavior. In most cases, a run-time 
        /// exception is thrown.
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.IgnoreCase)
            {
                throw new NotSupportedException("Case insensitive language bindings are not supported");
            }

            result = this.adapter.GetValue(binder.Name);
            return result != null;
        }

        #endregion

        #region Privates/internals

        internal static readonly Resource Missing = new Resource();
        ExpandoAdapter adapter = ExpandoAdapter.Empty;

        static Resource()
        {
            Missing.adapter = ExpandoAdapter.Empty;
            Missing.Author = null;
            Missing.Id = null;
            Missing.GeneratorVersion = null;
            Missing.Links = new Dictionary<string, Uri>(0);
            Missing.Title = null;
            Missing.Updated = DateTime.MinValue;
        }

        void EnsureUninitialized()
        {
            if (this.adapter != null)
            {
                throw new InvalidOperationException("Resource was intialized; Initialize operation may not execute again.");
            }
        }

        #endregion
    }
}