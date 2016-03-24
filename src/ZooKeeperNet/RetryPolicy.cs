/*
 *  Licensed to the Apache Software Foundation (ASF) under one or more
 *  contributor license agreements.  See the NOTICE file distributed with
 *  this work for additional information regarding copyright ownership.
 *  The ASF licenses this file to You under the Apache License, Version 2.0
 *  (the "License"); you may not use this file except in compliance with
 *  the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */
 using System;
 using System.Threading;

namespace ZooKeeperNet
{
    public interface IRetryPolicy
    {
        bool AllowRetry(int retryCount, TimeSpan elapsedTime, Action<TimeSpan> retrySleeper = null);
    }

    public abstract class SleepingRetry : IRetryPolicy
    {
        private readonly int _n;

        protected SleepingRetry(int n)
        {
            _n = n;
        }

        public int N { get { return _n; } }

        public bool AllowRetry(int retryCount, TimeSpan elapsedTime, Action<TimeSpan> retrySleeper = null)
        {
            var allow = false;
            var effectiveSleeper = retrySleeper ?? Thread.Sleep;

            if (retryCount < _n)
            {
                try
                {
                    effectiveSleeper(GetSleepTime(retryCount, elapsedTime));
                }
                catch (ThreadInterruptedException)
                {
                    // Handle gracefully and don't allow retries
                }
                allow = true;
            }
            return allow;
        }

        protected abstract TimeSpan GetSleepTime(int retryCount, TimeSpan elapsedTime);
    }

    public class RetryNTimes : SleepingRetry
    {
        private readonly TimeSpan _sleepTime;

        public RetryNTimes(int n, TimeSpan sleepTime)
            : base(n)
        {
            _sleepTime = sleepTime;
        }

        protected override TimeSpan GetSleepTime(int retryCount, TimeSpan elapsedTime)
        {
            return _sleepTime;
        }
    }
}
