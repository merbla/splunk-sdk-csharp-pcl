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
//// [X] Contracts
//// [O] Documentation
//// [ ] Seal all Args types
//// [ ] Eliminate EntityCollection<TEntityCollection, TEntity>.Args and settle on a pagination strategy, the whole 
////     point to this property.

namespace Splunk.Client
{
    /// <summary>
    /// Provides a class that represents a collection of Splunk <see cref=
    /// "StoragePassword"/> resources.
    /// </summary>
    public class StoragePasswordCollection : EntityCollection<StoragePasswordCollection, StoragePassword>
    {
        #region Constructors

        /// <summary>
        /// Intializes an new instance of the <see cref="StoragePasswordCollection"/>
        /// class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="namespace">
        /// An object identifying a Splunk service namespace.
        /// </param>
        /// <param name="args">
        /// </param>
        internal StoragePasswordCollection(Context context, Namespace @namespace, StoragePasswordCollectionArgs args = null)
            : base(context, @namespace, ClassResourceName, args)
        { }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the <see cref=
        /// "StoragePasswordCollection"/> class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        public StoragePasswordCollection()
        { }

        #endregion

        #region Privates/internals

        internal static readonly ResourceName ClassResourceName = new ResourceName("storage", "passwords");

        #endregion
    }
}
