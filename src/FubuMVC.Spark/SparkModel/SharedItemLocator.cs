﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace FubuMVC.Spark.SparkModel
{
    public interface ISharedItemLocator
    {
        SparkItem LocateSpark(string sparkName, SparkItem fromItem, IEnumerable<SparkItem> items);
    }

    public class SharedItemLocator : ISharedItemLocator
    {
        private readonly IEnumerable<string> _sharedFolderNames;
        public SharedItemLocator(IEnumerable<string> sharedFolderNames)
        {
            _sharedFolderNames = sharedFolderNames;
        }

        public SparkItem LocateSpark(string sparkName, SparkItem fromItem, IEnumerable<SparkItem> items)
        {
            var spark = locateSpark(sparkName, fromItem.FilePath, fromItem.RootPath, items);
            if (spark == null && fromItem.Origin != Constants.HostOrigin)
            {
                spark = locateInHostFromPackage(sparkName, items);
            }

            return spark;
        }

        private SparkItem locateSpark(string sparkName, string startPath, string stopPath, IEnumerable<SparkItem> items)
        {
            var reachables = reachableLocations(startPath, stopPath).ToList();
            return items.ByName(sparkName)
                .Where(x => reachables.Contains(x.DirectoryPath()))
                .FirstOrDefault();            
        }

        private SparkItem locateInHostFromPackage(string sparkName, IEnumerable<SparkItem> itemPool)
        {
            var hostRoot = itemPool.ByOrigin(Constants.HostOrigin).FirstValue(x => x.RootPath);
            if (hostRoot.IsEmpty()) return null;

            var sharedFolder = _sharedFolderNames.FirstValue(p => p);
            var startPath = Path.Combine(hostRoot, sharedFolder);

            return locateSpark(sparkName, startPath, hostRoot, itemPool);
        }

        private IEnumerable<string> reachableLocations(string path, string root)
        {
            do
            {
                path = Path.GetDirectoryName(path);
                if (path == null) break;

                foreach (var sharedFolder in _sharedFolderNames)
                {
                    yield return Path.Combine(path, sharedFolder);
                }

            } while (path.IsNotEmpty() && path.PathRelativeTo(root).IsNotEmpty());
        }
    }
}