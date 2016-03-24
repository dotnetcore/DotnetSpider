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

namespace ZooKeeperNet.Recipes {
	public interface ILeaderWatcher {
		/// <summary>
		/// This is called when all of the below:
		/// <list type="bullet">
		/// <item>
		/// <description>After this process calls start</description>
		/// </item>
		/// <item>
		/// <description>The first time that we determine that we are leader</description>
		/// </item>
		/// </list>
		/// </summary>
		void TakeLeadership();
	}

	public class LeaderElection : ProtocolSupport {
		private readonly string path;
		private string id;
		private readonly byte[] data;
		private ZNodeName idName;
		private readonly ILeaderWatcher watcher;
		public volatile bool IsOwner = false;

		public LeaderElection(ZooKeeper zookeeper, string path, ILeaderWatcher watcher, byte[] data) : base(zookeeper) {
			this.path = path;
			this.watcher = watcher;
			this.data = data;
		}

		public bool RunForLeader() {
			long sessionId = Zookeeper.SessionId;
			string prefix = "election-" + sessionId + "-";
			var names = Zookeeper.GetChildren(path, false);
			// See whether we have already run for election in this process
			foreach (string name in names) {
				if (name.StartsWith(prefix)) {
					id = name;
					if (LOG.IsDebugEnabled) {
						LOG.DebugFormat("Found id created last time: {0}", id);
					}
				}
			}

			if (id == null) {
				id = Zookeeper.Create(path.Combine(prefix), data, Acl, CreateMode.EphemeralSequential);

				if (LOG.IsDebugEnabled) {
					LOG.DebugFormat("Created id: {0}", id);
				}
			}

			idName = new ZNodeName(id);

			names = Zookeeper.GetChildren(path, false);
			var sortedNames = new SortedSet<ZNodeName>();
			foreach (var name in names) {
				sortedNames.Add(new ZNodeName(name));
			}

			var priors = sortedNames.HeadSet(idName);

			if (priors.Count == 0) {
				throw new InvalidOperationException("Count of priors is 0, but should at least include this node.");
			}

			if (priors.Count == 1) {
				IsOwner = true;
				watcher.TakeLeadership();
				return true;
			}
			// only watch the node directly before us
			ZNodeName penultimate = null, last = null;
			foreach (var name in sortedNames) {
				penultimate = last;
				last = name;
			}
			if (penultimate == null) {
				throw new InvalidOperationException("Penultimate value in priors is null, but count shoudl have been at least 2.");
			}
			var watchPath = path.Combine(penultimate.Name);
			if (Zookeeper.Exists(watchPath, new LeaderWatcher(this, watchPath, watcher)) == null) {
				IsOwner = true;
				watcher.TakeLeadership();
				return true;
			}
			return false;
		}

		private class LeaderWatcher : IWatcher {
			private readonly LeaderElection election;
			private readonly string path;
			private readonly ILeaderWatcher watcher;

			public LeaderWatcher(LeaderElection election, string path, ILeaderWatcher watcher) {
				this.election = election;
				this.path = path;
				this.watcher = watcher;
			}

			public void Process(WatchedEvent @event) {
				if (@event.Type == EventType.NodeDeleted && @event.Path == path) {
					election.IsOwner = true;
					watcher.TakeLeadership();
				}
			}
		}

		public void Start() {
			EnsurePathExists(path);

			RetryOperation(RunForLeader);
		}

		public void Close() {
			IsOwner = false;

			Zookeeper.Delete(id, -1);
		}

		public override string ToString() {
			return string.Format("IdName: {0}, IsOwner: {1}", idName, IsOwner);
		}
	}
}