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
﻿using System;
using System.Linq;
﻿using System.Collections.Generic;
﻿using System.Text;

namespace ZooKeeperNet
{
    public class ZKPaths
    {
        /**
         * Apply the namespace to the given path
         *
         * @param namespace namespace (can be null)
         * @param path path
         * @return adjusted path
         */
        public static String FixForNamespace(String theNamespace, String path)
        {
            if (theNamespace != null)
            {
                return MakePath(theNamespace, path);
            }
            return path;
        }

        /**
         * Given a full path, return the node name. i.e. "/one/two/three" will return "three"
         * 
         * @param path the path
         * @return the node
         */
        public static String GetNodeFromPath(String path)
        {
            PathUtils.ValidatePath(path);
            var i = path.LastIndexOf(PathUtils.PathSeparatorChar);
            if (i < 0)
            {
                return path;
            }

            if ((i + 1) >= path.Length)
            {
                return string.Empty;
            }
            return path.Substring(i + 1);
        }

        public class PathAndNode
        {
            public readonly string Path;
            public readonly string Node;

            public PathAndNode(string path, string node)
            {
                Path = path;
                Node = node;
            }
        }

        /**
         * Given a full path, return the node name and its path. i.e. "/one/two/three" will return {"/one/two", "three"}
         *
         * @param path the path
         * @return the node
         */
        public static PathAndNode GetPathAndNode(string path)
        {
            PathUtils.ValidatePath(path);
            var i = path.LastIndexOf(PathUtils.PathSeparatorChar);
            if (i < 0)
            {
                return new PathAndNode(path, string.Empty);
            }
            if ((i + 1) >= path.Length)
            {
                return new PathAndNode(PathUtils.PathSeparator, string.Empty);
            }
            var node = path.Substring(i + 1);
            var parentPath = (i > 0) ? path.Substring(0, i) : PathUtils.PathSeparator;
            return new PathAndNode(parentPath, node);
        }

        /**
         * Make sure all the nodes in the path are created. NOTE: Unlike File.mkdirs(), Zookeeper doesn't distinguish
         * between directories and files. So, every node in the path is created. The data for each node is an empty blob
         *
         * @param zookeeper the client
         * @param path      path to ensure
         * @throws InterruptedException thread interruption
         * @throws org.apache.zookeeper.KeeperException
         *                              Zookeeper errors
         */
        public static void Mkdirs(ZooKeeper zookeeper, String path)
        {
            Mkdirs(zookeeper, path, true);
        }

        /**
         * Make sure all the nodes in the path are created. NOTE: Unlike File.mkdirs(), Zookeeper doesn't distinguish
         * between directories and files. So, every node in the path is created. The data for each node is an empty blob
         *
         * @param zookeeper the client
         * @param path      path to ensure
         * @param makeLastNode if true, all nodes are created. If false, only the parent nodes are created
         * @throws InterruptedException thread interruption
         * @throws org.apache.zookeeper.KeeperException
         *                              Zookeeper errors
         */
        public static void Mkdirs(IZooKeeper zookeeper, String path, bool makeLastNode)
        {
            PathUtils.ValidatePath(path);

            var pos = 1; // skip first slash, root is guaranteed to exist
            do
            {
                pos = path.IndexOf(PathUtils.PathSeparatorChar, pos + 1);

                if (pos == -1)
                {
                    if (makeLastNode)
                    {
                        pos = path.Length;
                    }
                    else
                    {
                        break;
                    }
                }

                var subPath = path.Substring(0, pos);
                if (zookeeper.Exists(subPath, false) == null)
                {
                    try
                    {
                        zookeeper.Create(subPath, new byte[0], Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                    }
                    catch (KeeperException.NodeExistsException e)
                    {
                        // ignore... someone else has created it since we checked
                    }
                }

            }
            while (pos < path.Length);
        }

        /**
         * Return the children of the given path sorted by sequence number
         *
         * @param zookeeper the client
         * @param path      the path
         * @return sorted list of children
         * @throws InterruptedException thread interruption
         * @throws org.apache.zookeeper.KeeperException
         *                              zookeeper errors
         */
        public static List<String> GetSortedChildren(IZooKeeper zookeeper, String path)
        {
            var children = zookeeper.GetChildren(path, false);
            var sortedList = new List<string>(children.OrderBy(x => x));
            return sortedList;
        }

        /**
         * Given a parent path and a child node, create a combined full path
         *
         * @param parent the parent
         * @param child  the child
         * @return full path
         */
        public static String MakePath(String parent, String child)
        {
            var path = new StringBuilder();

            if (!parent.StartsWith("/"))
            {
                path.Append("/");
            }
            path.Append(parent);
            if (string.IsNullOrEmpty(child))
            {
                return path.ToString();
            }

            if (!parent.EndsWith(PathUtils.PathSeparator))
            {
                path.Append(PathUtils.PathSeparator);
            }

            if (child.StartsWith(PathUtils.PathSeparator))
            {
                path.Append(child.Substring(1));
            }
            else
            {
                path.Append(child);
            }

            return path.ToString();
        }

        private ZKPaths()
        {
        }
    }
}
