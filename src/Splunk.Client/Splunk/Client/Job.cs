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

// TODO:
//
// [ ] Parameterize Job.Transition strategy. (It's primitive at present)
//     It's the usual sort of thing:
//     + Linear or non-linear time between retries? 
//     + How long before first retry? 
//     + How many retries?
//     Should we cancel the job, if it times out?
//
// [O] Contracts
//
// [O] Documentation
//
// [ ] Trace messages (e.g., when there are no observers)

namespace Splunk.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Xml;

    /// <summary>
    /// 
    /// </summary>
    public sealed class Job : Entity<Job>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="namespace">
        /// An object identifying a Splunk service namespace.
        /// </param>
        /// <param name="name">
        /// Name of the search <see cref="Job"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <see cref="name"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <see cref="context"/> or <see cref="namespace"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="namespace"/> is not specific.
        /// </exception>
        internal Job(Context context, Namespace @namespace, string name)
            : base(context, @namespace, JobCollection.ClassResourceName, name)
        { }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the <see cref=
        /// "Job"/> class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        public Job()
        { }

        #endregion

        #region Properties

        public bool CanSummarize
        {
            get { return this.GetValue("CanSummarize", BooleanConverter.Instance); }
        }

        public DateTime CursorTime
        {
            get { return this.GetValue("CursorTime", DateTimeConverter.Instance); }
        }

        public int DefaultSaveTTL
        {
            get { return this.GetValue("DefaultSaveTTL", Int32Converter.Instance); }
        }

        public int DefaultTTL
        {
            get { return this.GetValue("DefaultTTL", Int32Converter.Instance); }
        }

        public long DiskUsage
        {
            get { return this.GetValue("DiskUsage", Int64Converter.Instance); } // sample value: "86016"
        }

        /// <summary>
        /// Gets a value that indicates the current <see cref="Job"/> dispatch
        /// state.
        /// </summary>
        /// <returns>
        /// A <see cref="DispatchState"/> value.
        /// </returns>
        /// <remarks>
        /// Clients that call <see cref="Job.Update"/> to poll for job status
        /// use this property to determine the state of the current search job.
        /// </remarks>
        public DispatchState DispatchState
        {
            get { return this.GetValue("DispatchState", EnumConverter<DispatchState>.Instance); }
        }

        public double DoneProgress
        {
            get { return this.GetValue("DoneProgress", DoubleConverter.Instance); }
        }

        public long DropCount
        {
            get { return this.GetValue("DropCount", Int64Converter.Instance); }
        }

        /// <summary>
        /// Gets the access control lists for the current <see cref="Job"/>.
        /// </summary>
        public Eai Eai
        {
            get { return this.GetValue("Eai", Eai.Converter.Instance); }
        }

        public DateTime EarliestTime
        {
            get { return this.GetValue("EarliestTime", DateTimeConverter.Instance); }
        }

        public long EventAvailableCount
        {
            get { return this.GetValue("EventAvailableCount", Int64Converter.Instance); }
        }

        public long EventCount
        {
            get { return this.GetValue("EventCount", Int64Converter.Instance); }
        }

        public int EventFieldCount
        {
            get { return this.GetValue("EventFieldCount", Int32Converter.Instance); }
        }

        public bool EventIsStreaming
        {
            get { return this.GetValue("EventIsStreaming", BooleanConverter.Instance); } // sample value: "1"
        }

        public bool EventIsTruncated
        {
            get { return this.GetValue("EventIsTruncated", BooleanConverter.Instance); } // sample value: "0"
        }

        public string EventSearch
        {
            get { return this.GetValue("EventSearch", StringConverter.Instance); } // sample value: "search index=_internal  | head 10"
        }

        public SortDirection EventSorting
        {
            get { return this.GetValue("EventSorting", EnumConverter<SortDirection>.Instance); }
        }

        public long IndexEarliestTime
        {
            get { return this.GetValue("IndexEarliestTime", Int64Converter.Instance); } // sample value: "1396566178"
        }

        public long IndexLatestTime
        {
            get { return this.GetValue("IndexLatestTime", Int64Converter.Instance); } // sample value: "1396566183"
        }

        public bool IsBatchModeSearch
        {
            get { return this.GetValue("IsBatchModeSearch", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsDone
        {
            get { return this.GetValue("IsDone", BooleanConverter.Instance); } // sample value: "1"
        }

        public bool IsFailed
        {
            get { return this.GetValue("IsFailed", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsFinalized
        {
            get { return this.GetValue("IsFinalized", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsPaused
        {
            get { return this.GetValue("IsPaused", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsPreviewEnabled
        {
            get { return this.GetValue("IsPreviewEnabled", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsRealTimeSearch
        {
            get { return this.GetValue("IsRealTimeSearch", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsRemoteTimeline
        {
            get { return this.GetValue("IsRemoteTimeline", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsSaved
        {
            get { return this.GetValue("IsSaved", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsSavedSearch
        {
            get { return this.GetValue("IsSavedSearch", BooleanConverter.Instance); } // sample value: "0"
        }

        public bool IsZombie
        {
            get { return this.GetValue("IsZombie", BooleanConverter.Instance); } // sample value: "0"
        }

        public string Keywords
        {
            get { return this.GetValue("Keywords", StringConverter.Instance); } // sample value: "index::_internal"
        }

        public DateTime LatestTime
        {
            get { return this.GetValue("LatestTime", DateTimeConverter.Instance); }
        }

        // Messages	{System.Dynamic.ExpandoObject}	System.Dynamic.ExpandoObject

        public string NormalizedSearch
        {
            get { return this.GetValue("NormalizedSearch", StringConverter.Instance); } 
        }

        public int NumPreviews
        {
            get { return this.GetValue("NumPreviews", Int32Converter.Instance); }
        }

        // Performance	{System.Dynamic.ExpandoObject}	System.Dynamic.ExpandoObject

        public int Pid
        {
            get { return this.GetValue("Pid", Int32Converter.Instance); } // sample value: "1692"
        }

        public int Priority
        {
            get { return this.GetValue("Priority", Int32Converter.Instance); } // sample value: "5"
        }

        public string RemoteSearch
        {
            get { return this.GetValue("RemoteSearch", StringConverter.Instance); }
        }

        // Request	{System.Dynamic.ExpandoObject}	System.Dynamic.ExpandoObject

        public long ResultCount
        {
            get { return this.GetValue("ResultCount", Int64Converter.Instance); }
        }

        public bool ResultIsStreaming
        {
            get { return this.GetValue("ResultIsStreaming", BooleanConverter.Instance); }
        }

        public long ResultPreviewCount
        {
            get { return this.GetValue("ResultPreviewCount", Int64Converter.Instance); }
        }

        public double RunDuration
        {
            get { return this.GetValue("RunDuration", DoubleConverter.Instance); } // sample value: "0.220000"
        }

        // Runtime	{System.Dynamic.ExpandoObject}	System.Dynamic.ExpandoObject

        public long ScanCount
        {
            get { return this.GetValue("ScanCount", Int64Converter.Instance); }
        }

        //	SearchProviders	{System.Collections.Generic.List<object>}	System.Collections.Generic.List`1[System.Object]

        public string Sid
        {
            get { return this.Name; }
        }

        public int StatusBuckets
        {
            get { return this.GetValue("StatusBuckets", Int32Converter.Instance); }
        }

        public long Ttl
        {
            get { return this.GetValue("Ttl", Int64Converter.Instance); }
        }

        #endregion

        /// <summary>
        /// Refreshes the cached state of the current <see cref="Entity<TEntity>"/>.
        /// </summary>
        public override async Task GetAsync()
        {
            //// TODO: This retry logic is for jobs. Parmeterize it and move it into the Job class

            // FJR: I assume the retry logic is for jobs, since nothing else requires this. I suggest moving it
            // into Job. Also, it's insufficient. If you're just trying to get some state, this will do it, but
            // as of Splunk 6, getting a 200 and content back does not imply you have all the fields. For pivot
            // support, they're now shoving fields in as they become ready, so you have to wait until the dispatchState
            // field of the Atom entry reaches a certain point.

            RequestException requestException = null;

            for (int i = 3; i > 0; --i)
            {
                try
                {
                    //// Guarantee: unique result because entities have specific namespaces

                    using (var response = await this.Context.GetAsync(this.Namespace, this.ResourceName))
                    {
                        //// TODO: Use Response.EnsureStatusCode. Is it true that gets always return HttpStatusCode.OK?

                        if (response.Message.StatusCode == HttpStatusCode.NoContent)
                        {
                            throw new RequestException(response.Message, new Message(MessageType.Warning, string.Format("Resource '{0}/{1}' is not ready.", this.Namespace, this.ResourceName)));
                        }

                        if (!response.Message.IsSuccessStatusCode)
                        {
                            throw new RequestException(response.Message, await Message.ReadMessagesAsync(response.XmlReader));
                        }

                        var reader = response.XmlReader;
                        await reader.ReadAsync();

                        if (reader.NodeType == XmlNodeType.XmlDeclaration)
                        {
                            await response.XmlReader.ReadAsync();
                        }

                        if (reader.NodeType != XmlNodeType.Element)
                        {
                            throw new InvalidDataException(); // TODO: Diagnostics
                        }

                        AtomEntry entry;

                        if (reader.Name == "feed")
                        {
                            AtomFeed feed = new AtomFeed();

                            await feed.ReadXmlAsync(reader);
                            int count = feed.Entries.Count;

                            foreach (var feedEntry in feed.Entries)
                            {
                                string id = feedEntry.Title;
                                id.Trim();
                            }

                            if (feed.Entries.Count != 1)
                            {
                                throw new InvalidDataException(); // TODO: Diagnostics
                            }

                            entry = feed.Entries[0];
                        }
                        else
                        {
                            entry = new AtomEntry();
                            await entry.ReadXmlAsync(reader);
                        }

                        this.Snapshot = new EntitySnapshot(entry);
                    }

                    return;
                }
                catch (RequestException e)
                {
                    if (e.StatusCode != System.Net.HttpStatusCode.NoContent)
                    {
                        throw;
                    }
                    requestException = e;
                }
                await Task.Delay(500);
            }

            throw requestException;
        }

        public async Task RemoveAsync()
        {
            using (var response = await this.Context.DeleteAsync(this.Namespace, this.ResourceName))
            {
                await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
            }
        }

        #region Methods for retrieving search results

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<SearchResults> GetSearchResultsAsync(SearchResultsArgs args = null)
        {
            await this.TransitionAsync(DispatchState.Done);

            var searchResults = await this.GetSearchResultsAsync("results", args);
            return searchResults;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<SearchResults> GetSearchResultsEventsAsync(SearchEventArgs args = null)
        {
            await this.TransitionAsync(DispatchState.Done);

            var searchResults = await this.GetSearchResultsAsync("events", args);
            return searchResults;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<SearchResults> GetSearchResultsPreviewAsync(SearchResultsArgs args = null)
        {
            await this.TransitionAsync(DispatchState.Running);

            var searchResults = await this.GetSearchResultsAsync("results_preview", args);
            return searchResults;
        }

        #endregion

        #region Job Control methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task CancelAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "cancel")
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task DisablePreviewAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "disable_preview") 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task EnablePreviewAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "enable_preview") 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task FinalizeAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "finalize") 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task PauseAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "pause") 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "save") 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public async Task SetPriorityAsync(int priority)
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "priority"),
                new Argument("priority", priority.ToString())
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public async Task SetTtlAsync(int ttl)
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[]
            { 
                new Argument("action", "setttl"),
                new Argument("ttl", ttl.ToString())
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public async Task TouchAsync(int ttl)
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[]
            { 
                new Argument("action", "touch"),
                new Argument("ttl", ttl.ToString())
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task UnpauseAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "unpause") 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task UnsaveAsync()
        {
            await this.TransitionAsync(DispatchState.Running);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "unsave") 
            });
        }

        public async Task UpdateJobArgs(JobArgs args)
        {
            using (var response = await this.Context.PostAsync(this.Namespace, this.ResourceName, args))
            {
                await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
            }
        }

        #endregion

        #region Methods used by our base class, Entity<TEntity>

        #endregion

        #region Privates

        async Task<SearchResults> GetSearchResultsAsync(string endpoint, IEnumerable<Argument> args)
        {
            var resourceName = new ResourceName(this.ResourceName, endpoint);
            var response = await this.Context.GetAsync(this.Namespace, resourceName, args);

            try
            {
                var searchResults = await SearchResults.CreateAsync(response, leaveOpen: false);
                return searchResults;
            }
            catch 
            {
                response.Dispose();
                throw;
            }
        }

        async Task PostControlCommandAsync(IEnumerable<Argument> args)
        {
            var resourceName = new ResourceName(this.ResourceName, "control");

            using (var response = await this.Context.PostAsync(this.Namespace, resourceName, args))
            {
                await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
            }
        }

        async Task TransitionAsync(DispatchState requiredState)
        {
            while (this.DispatchState < requiredState)
            {
                await Task.Delay(500);
                await this.GetAsync();
            }
        }

        #endregion
    }
}
