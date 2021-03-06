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

namespace Splunk.Client.Examples
{
    using System;
    using System.Net;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Starts a normal search and polls for completion to find out when the search has finished.
    /// </summary>
    class Program
    {
        static Program()
        {
            // TODO: Use WebRequestHandler.ServerCertificateValidationCallback instead
            // 1. Instantiate a WebRequestHandler
            // 2. Set its ServerCertificateValidationCallback
            // 3. Instantiate a Splunk.Client.Context with the WebRequestHandler

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                return true;
            };
        }

        static void Main(string[] args)
        {
            using (var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search")))
            {
                SearchRealTime(service).Wait();
                OneshotSearch(service).Wait();
            }
        }


        /// <summary>
        /// Called when [search].
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        static async Task OneshotSearch(Service service)
        {
            //// Login
            await service.LoginAsync("admin", "changeme");

            // Simple oneshot search
            using (SearchResults searchResults = await service.SearchOneshotAsync("search index=_internal | head 5"))
            {
                foreach (Result record in searchResults)
                {
                    Console.WriteLine(record);
                }
            }

            //Use JobArgs to define the search
            JobArgs args = new JobArgs("search index=_internal | head 5")
            {
                AutoCancel = 0,
            };

            using (SearchResults searchResults = await service.SearchOneshotAsync(args))
            {
                foreach (Result record in searchResults)
                {
                    Console.WriteLine(record);
                }
            }

            await service.LogoffAsync();
        }

        /// <summary>
        /// Do real time seach
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        static async Task SearchRealTime(Service service)
        {

            await service.LoginAsync("admin", "changeme");

            // Realtime window is 5 minutes
            var jobArgs = new JobArgs("search index=_internal")
            {
                SearchMode = SearchMode.Realtime,
                EarliestTime = "rt-5m",
                LatestTime = "rt",
            };

            Job job = await service.CreateJobAsync(jobArgs);


            //this sleep should be removed if DVPL-4503 is fixed.
            //Thread.Sleep(5000);
            for (var i = 0; i < 5; i++)
            {
                System.Console.WriteLine("============================= Snapshot {0}=================================", i);

                using (SearchResults searchResults = await job.GetSearchResultsPreviewAsync())
                {
                    System.Diagnostics.Debug.Assert(!searchResults.AreFinal);
                    System.Console.WriteLine("searchResults count:{0}", searchResults.Count());

                }

                Thread.Sleep(1000);
            }

            await job.CancelAsync();

            await service.LogoffAsync();
        }
    }
}
