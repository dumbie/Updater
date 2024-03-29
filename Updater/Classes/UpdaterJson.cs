﻿namespace Updater.Classes
{
    public class UpdaterJson
    {
        public string UserName { get; set; }
        public string RepoName { get; set; }
        public string FileName { get; set; }
        public string ExtractName { get; set; }
        public string[] ProcessLaunch { get; set; }
        public string[] ProcessClose { get; set; }
        public string[] FilesIgnore { get; set; }
        public string[] FilesDelete { get; set; }
        public string[][] FilesMove { get; set; }
        public string[][] FilesCopy { get; set; }
        public string[] FoldersIgnore { get; set; }
        public string[] FoldersDelete { get; set; }
        public string[][] FoldersMove { get; set; }
        public string[][] FoldersCopy { get; set; }
    }
}