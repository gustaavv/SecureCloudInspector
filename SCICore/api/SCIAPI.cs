﻿using SCICore.entity;
using SCICore.util;

namespace SCICore.api;

/// <summary>
/// SCI API
/// </summary>
public static class SciApi
{
    /// <summary>
    /// Walk through a dir, build an item tree and calculate hashes for each file.
    /// This method can be used for both source folder and encrypted folder.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static async Task<Node> CalculateHashesWhenWalk(string dir)
    {
        var item = FsApi.BuildItemTree(dir);

        dir = Path.GetFullPath(dir);
        var node = await CalculateHashesWhenWalk(item, new DirectoryInfo(dir).Parent!.ToString());
        return node;
    }

    private static async Task<Node> CalculateHashesWhenWalk(Item item, string parentPath)
    {
        var curPath = Path.Join(parentPath, item.Name);
        if (item.Type == ItemType.File)
        {
            var hashResult = await HashUtils.ComputeFileHash(curPath);
            var node = new Node(item.Name, item.Type, item.Size, hashResult);
            return node;
        }
        else
        {
            // postorder 
            var tasks = item.Children.Select(child =>
                CalculateHashesWhenWalk(child, curPath)).ToList();
            var results = await Task.WhenAll(tasks);

            var node = new Node(item.Name, item.Type);
            node.Children.AddRange(results);
            return node;
        }
    }
}