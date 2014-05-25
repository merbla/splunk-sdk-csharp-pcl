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
//// [ ] Check for HTTP Status Code 204 (No Content) and empty atoms in 
////     Entity<TEntity>.UpdateAsync.
////
//// [O] Contracts
////
//// [O] Documentation
////
//// [X] Pick up standard properties from AtomEntry on Update, not just AtomEntry.Content
////     See [Splunk responses to REST operations](http://goo.gl/tyXDfs).
////
//// [X] Remove Entity<TEntity>.Invalidate method
////     FJR: This gets called when we set the record value. Add a comment saying what it's
////     supposed to do when it's overridden.
////     DSN: I've adopted an alternative method for getting strongly-typed values. See, for
////     example, Job.DispatchState or ServerInfo.Guid.

namespace Splunk.Client.Refactored
{
    using Splunk.Client;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Xml;

    /// <summary>
    /// Provides an object representation of a Splunk entity.
    /// </summary>
    /// <remarks>
    /// This is the base class for all Splunk entities.
    /// </remarks>
    public abstract class ResourceEndpoint : Endpoint
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ResourceEndpoint"/> instance.
        /// </summary>
        /// <param name="service">
        /// An object representing a Splunk service endpoint.
        /// <param name="name">
        /// An object identifying a Splunk resource within <paramref name=
        /// "service"/>.<see cref="Namespace"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> or <paramref name="name"/> are <c>null</c>.
        protected internal ResourceEndpoint(Service service, ResourceName name)
            : base(service.Context, service.Namespace, name)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceEndpoint"/> 
        /// class as specified by <paramref name="context"/>, <paramref name="ns"/>
        /// and "<paramref name="resourceName"/>.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="ns">
        /// An object identifying a Splunk services namespace.
        /// </param>
        /// <param name="resourceName">
        /// An object identifying a Splunk resource within <paramref name="ns"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/>, <paramref name="ns"/>, or <paramref name=
        /// "resourceName"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="ns"/> is not specific.
        /// </exception>
        public ResourceEndpoint(Context context, Namespace ns, ResourceName resourceName)
            : base(context, ns, resourceName)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceEndpoint"/> class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="ns">
        /// An object identifying a Splunk services namespace.
        /// </param>
        /// <param name="collection">
        /// The <see cref="ResourceName"/> of an <see cref="EntityCollection&lt;TEntity&gt;"/>.
        /// </param>
        /// <param name="entity">
        /// The name of an entity within <paramref name="collection"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="entity"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/>, <paramref name="ns"/>, or <paramref 
        /// name="collection"/>, or <paramref name="entity"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="ns"/> is not specific.
        /// </exception>
        public ResourceEndpoint(Context context, Namespace ns, ResourceName collection, string name)
            : base(context, ns, new ResourceName(collection, name))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceEndpoint"/> class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="feed">
        /// A Splunk response atom feed.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="entity"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/>, <paramref name="ns"/>, or <paramref 
        /// name="collection"/>, or <paramref name="entity"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="ns"/> is not specific.
        /// </exception>
        public ResourceEndpoint(Context context, AtomFeed feed)
        {
            this.Initialize(context, feed);
        }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the <see cref=
        /// "ResourceEndpoint"/> class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        public ResourceEndpoint()
        { }

        #endregion

        #region Properties (guaranteed to be present in every snapshot)

        /// <inheritdoc cref="Resource.Author"/>
        public string Author
        {
            get { return this.snapshot.Author; }
        }

        /// <inheritdoc cref="Resource.Id"/>
        public Uri Id
        {
            get { return this.snapshot.Id; }
        }

        /// <inheritdoc cref="Resource.GeneratorVersion"/>
        public Version GeneratorVersion
        {
            get { return this.snapshot.GeneratorVersion; }
        }

        /// <inheritdoc cref="Resource.Links"/>

        public IReadOnlyDictionary<string, Uri> Links
        {
            get { return this.snapshot.Links; }
        }

        /// <inheritdoc cref="Resource.Title"/>
        public string Title
        {
            get { return this.snapshot.Title; }
        }

        /// <inheritdoc cref="Resource.Updated"/>
        public DateTime Updated
        {
            get { return this.snapshot.Updated; }
        }

        #endregion

        #region Methods

        #region Infrastructure methods

        /// <summary>
        /// Asynchronously creates a <see cref="ResourceEndpoint"/> from a
        /// Splunk atom feed <see cref="Response"/>.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of <see cref="ResourceEndpoint"/> to be created.
        /// </typeparam>
        /// <param name="response">
        /// An object representing a Splunk atom feed response.
        /// </param>
        /// <returns>
        /// The <see cref="ResourceEndpoint"/> created.
        /// </returns>
        internal static async Task<TEntity> CreateAsync<TEntity>(Context context, Response response) where TEntity : ResourceEndpoint, new()
        {
            var feed = new AtomFeed();

            await feed.ReadXmlAsync(response.XmlReader);
            var resourceEndpoint = new TEntity();
            resourceEndpoint.Initialize(context, feed);

            return resourceEndpoint;
        }

        /// <summary>
        /// Infrastructure. Initializes the current uninitialized <see cref=
        /// "ResourceEndpoint"/>.
        /// class.
        /// </summary>
        /// <param name="context">
        /// /// An object representing a Splunk server session.
        /// </param>
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
        /// The current <see cref="ResourceEndpoint"/> is already initialized.
        /// </exception>
        /// <remarks>
        /// This method may be called once to intialize a <see cref="ResourceEndpoint"/>
        /// instantiated by the default constructor. Override this method to 
        /// provide special initialization code. Call this base method before 
        /// initialization is complete. 
        /// <note type="note">
        /// This method supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </note>
        /// </remarks>
        protected internal virtual void Initialize(Context context, AtomEntry entry, Version generatorVersion)
        {
            this.Initialize(context, entry.Id);
            this.ReconstructSnapshot(entry, generatorVersion);
        }

        /// <summary>
        /// Infrastructure. Initializes the current uninitialized <see cref=
        /// "ResourceEndpoint"/>.
        /// class.
        /// </summary>
        /// <param name="context">
        /// /// An object representing a Splunk server session.
        /// </param>
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
        /// The current <see cref="ResourceEndpoint"/> is already initialized.
        /// </exception>
        /// <remarks>
        /// This method may be called once to intialize a <see cref="ResourceEndpoint"/>
        /// instantiated by the default constructor. Override this method to 
        /// provide special initialization code. Call this base method before 
        /// initialization is complete. 
        /// <note type="note">
        /// This method supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </note>
        /// </remarks>
        protected internal abstract void Initialize(Context context, AtomFeed feed);

        /// <summary>
        /// Infrastructure. Initializes the current uninitialized <see cref=
        /// "ResourceEndpoint"/>.
        /// class.
        /// </summary>
        /// <param name="context">
        /// /// An object representing a Splunk server session.
        /// </param>
        /// <param name="resource">
        /// An object representing a Splunk resource.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> or <paramref name="resource"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current <see cref="ResourceEndpoint"/> is already initialized.
        /// </exception>
        /// <remarks>
        /// This method may be called once to intialize a <see cref="ResourceEndpoint"/>
        /// instantiated by the default constructor. Override this method to 
        /// provide special initialization code. Call this base method before 
        /// initialization is complete. 
        /// <note type="note">
        /// This method supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </note>
        /// </remarks>
        protected internal abstract void Initialize(Context context, Resource resource);

        /// <summary>
	    /// Asynchronously updates the <see cref="Content"/> of the current 
        /// <see cref="ResourceEndpoint"/>
	    /// </summary>
	    /// <param name="entry">
	    /// A Splunk <see cref="AtomEntry"/>.
	    /// </param>
        protected abstract void ReconstructSnapshot(AtomEntry entry, Version generatorVersion);

        /// <summary>
        /// Asynchronously updates the <see cref="Content"/> of the current 
        /// <see cref="ResourceEndpoint"/>
        /// </summary>
        /// <param name="feed">
        /// A Splunk <see cref="AtomFeed"/>.
        /// </param>
        protected abstract void ReconstructSnapshot(AtomFeed feed);

        /// <summary>
        /// Asynchronously updates the <see cref="Content"/> of the current <see
        /// cref="ResourceEndpoint"/>
        /// </summary>
        /// <param name="resource">
        /// An object representing a Splunk resource.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the operation.
        /// </returns>
        protected abstract void ReconstructSnapshot(Resource resource);

        /// <summary>
        /// Asynchronously updates the <see cref="Content"/> of the current <see
        /// cref="ResourceEndpoint"/>
        /// </summary>
        /// <param name="response">
        /// A Splunk atom feed <see cref="Response"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the operation.
        /// </returns>
        protected internal abstract Task<bool> ReconstructSnapshotAsync(Response response);

        #endregion

        #endregion

        #region Privates/internals

        volatile Resource snapshot = Resource.Missing;

        #endregion
    }
}