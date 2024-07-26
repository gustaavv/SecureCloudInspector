using SCICore.entity;
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

    /// <summary>
    /// Encryption API, can be used for both creating and updating the encrypted folder.
    /// </summary>
    /// <param name="db"></param>
    public static async Task EncryptData(Database db)
    {
        var oldNode = db.Node;
        // get latest folder structure
        var newNode = await CalculateHashesWhenWalk(db.SourceFolder);
        // find archives can be reused
        ReuseArchives(newNode, oldNode);
        db.Node = newNode;
        // delete old archives cannot be reused
        DeleteOldArchives(db.Node, new DirectoryInfo(db.EncryptedFolder).Parent!.ToString());
        // make new archives
        await MakeNewArchive(db, db.Node,
            new DirectoryInfo(db.EncryptedFolder).Parent!.ToString(),
            new DirectoryInfo(db.SourceFolder).Parent!.ToString()
        );
    }

    /// <summary>
    /// To update encrypted folder instead of create a new encrypted folder, this
    /// method identifies those files aren't changed, whose corresponding archives
    /// can be reused.
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="oldNode"></param>
    /// <returns></returns>
    private static void ReuseArchives(Node newNode, Node oldNode)
    {
        if (newNode.Type != oldNode.Type)
            return;

        if (newNode.Type == ItemType.Dir)
        {
            var newMap = newNode.GetChildFileNameMap();
            var oldMap = oldNode.GetChildFileNameMap();
            foreach (var (newFileName, child) in newMap)
            {
                if (!oldMap.ContainsKey(newFileName))
                    continue;

                ReuseArchives(child, oldMap[newFileName]);
            }

            newNode.ArchiveName = oldNode.ArchiveName;
            return;
        }

        if (newNode.FileHashResult != oldNode.FileHashResult) return;

        newNode.ArchiveName = oldNode.ArchiveName;
        newNode.ArchiveSize = oldNode.ArchiveSize;
        newNode.ArchiveHashResult = oldNode.ArchiveHashResult;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node">a node representing current directory</param>
    /// <param name="parentPath"> path to the parent of the encrypted folder </param>
    private static void DeleteOldArchives(Node node, string parentPath)
    {
        if (node.Type != ItemType.Dir)
            throw new Exception("node is not Dir type");

        if (node.ArchiveName == null!)
            return;

        var curPath = Path.Join(parentPath, node.ArchiveName);
        var map = node.GetChildArchiveNameMap();

        foreach (var f in Directory.GetFiles(curPath))
        {
            if (map.ContainsKey(f))
                continue;
            File.Delete(Path.Join(curPath, f));
        }

        foreach (var d in Directory.GetDirectories(curPath))
        {
            if (map.ContainsKey(d))
            {
                DeleteOldArchives(map[d], curPath);
            }
            else
            {
                Directory.Delete(Path.Join(curPath, d), true);
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"> Database </param>
    /// <param name="node">a node representing current directory</param>
    /// <param name="parentPath"> path to the parent of the encrypted folder </param>
    /// <param name="sourceParentPath"> path to the parent of the source folder </param>
    private static async Task MakeNewArchive(Database db, Node node, string parentPath, string sourceParentPath)
    {
        // TODO: code review
        // TODO: 确定 top-level node 的 archiveFileName 应该是什么
        if (node.Type != ItemType.Dir)
            throw new Exception("node is not Dir type");
        var curPath = Path.Join(parentPath, node.ArchiveName);

        // old archives and folders
        var set = new HashSet<string>();
        Directory.GetFiles(curPath).ToList().ForEach(e => set.Add(e));
        Directory.GetDirectories(curPath).ToList().ForEach(e => set.Add(e));

        var makeRarTasks = new List<Task>();
        var callSubDirTasks = new List<Task>();

        foreach (var child in node.Children)
        {
            if (child.Type == ItemType.File)
            {
                if (set.Contains(child.ArchiveName))
                    continue;
                var results = await Task.WhenAll(
                    EncryptApi.MakeArchiveName(db.EncryptScheme.FileNamePattern, child.FileName),
                    EncryptApi.MakeArchivePwd(db, child.FileName)
                );
                var archiveName = results[0] + ".rar";
                var pwd = results[1];
                var sourceFilePath = Path.Join(sourceParentPath, node.FileName, child.FileName);
                var targetArchivePath = Path.Join(curPath, archiveName);
                makeRarTasks.Add(ArchiveUtils.CompressRar(new List<string> { sourceFilePath }, targetArchivePath, pwd));
                child.ArchiveName = archiveName;
            }
            else if (child.Type == ItemType.Dir)
            {
                if (child.ArchiveName == null!)
                {
                    string archiveName =
                        await EncryptApi.MakeArchiveName(db.EncryptScheme.FileNamePattern, child.FileName);
                    Directory.CreateDirectory(Path.Join(curPath, archiveName));
                    child.ArchiveName = archiveName;
                }

                callSubDirTasks.Add(MakeNewArchive(db, child, curPath, Path.Join(sourceParentPath, node.FileName)));
            }
            else
            {
                throw new Exception($"unknown type: {child.Type}");
            }
        }

        await Task.WhenAll(makeRarTasks);


        // now calculate hashes of new archives
        var newArchiveHashTasks = new List<Task>();
        foreach (var child in node.Children)
        {
            if (child.Type == ItemType.File && child.ArchiveHashResult == null!)
            {
                newArchiveHashTasks.Add(Task.Run(() =>
                {
                    var archivePath = Path.Join(curPath, child.ArchiveName);
                    var hashResult = HashUtils.ComputeFileHash(archivePath).Result;
                    child.ArchiveHashResult = hashResult;
                    child.ArchiveSize = new FileInfo(archivePath).Length;
                }));
            }
        }

        var finalTasks = new List<Task>();
        finalTasks.AddRange(newArchiveHashTasks);
        finalTasks.AddRange(callSubDirTasks);
        await Task.WhenAll(finalTasks);
    }
}