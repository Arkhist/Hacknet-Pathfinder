using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Game.Folder
{
    public static class Extensions
    {
        public static Hacknet.Folder AddFolder(this Hacknet.Folder self, string name, List<Hacknet.Folder> folders = null, List<Hacknet.FileEntry> files = null)
        {
            self.Add(new Hacknet.Folder(name));
            if (folders != null) self.folders[self.folders.Count].folders = folders;
            if (files != null) self.folders[self.folders.Count].files = files;
            return self;
        }

        public static Hacknet.Folder AddFile(this Hacknet.Folder self, string name, string data = null)
        {
            self.Add(new Hacknet.FileEntry(name, data));
            return self;
        }

        public static Hacknet.Folder AddFile(this Hacknet.Folder self, string name, Executable.IInterface data) =>
                             self.AddFile(name, Executable.Handler.GetStandardFileDataBy(data));

        public static Hacknet.Folder AddFile(this Hacknet.Folder self, string name, uint? dataIndex = null) =>
                             self.AddFile(name,
                                          Hacknet.FileEntry.fileData[(int)Math.Min(dataIndex.Value,
                                                                                   Hacknet.FileEntry.filenames.Count - 1
                                                                                  )]);

        public static Hacknet.Folder AddExecutableFile(this Hacknet.Folder self, string name, string id) =>
                             self.AddFile(name,
                                          ExeInfoManager.GetExecutableInfo(id).Data ??
                                          Executable.Handler.GetStandardFileDataBy(id, true));

        public static Hacknet.Folder AddExecutableFile(this Hacknet.Folder self, string name, int id) =>
                             self.AddFile(name, ExeInfoManager.GetExecutableInfo(id).Data);

        public static Hacknet.Folder AddRandomFile(this Hacknet.Folder self, object name = null, uint? id = null) =>
                             self.AddFile(name as string ??
                                          Hacknet.FileEntry.filenames[
                                              (int)Math.Min(
                                                  (name as long?).GetValueOrDefault(),
                                                  Hacknet.FileEntry.filenames.Count - 1
                                                 )
                                             ],
                                         Hacknet.FileEntry.fileData[
                                              (int)Math.Min(
                                                  id.Value,
                                                  Hacknet.FileEntry.filenames.Count - 1)
                                             ]);

        public static Hacknet.Folder RemoveFolder(this Hacknet.Folder self, string name)
        {
            self.folders.RemoveAll(obj => obj.name == name);
            return self;
        }

        public static Hacknet.Folder RemoveFile(this Hacknet.Folder self, string name)
        {
            self.files.RemoveAll(obj => obj.name == name);
            return self;
        }

        public static Hacknet.Folder MoveFolder(this Hacknet.Folder self, string name, Hacknet.Folder to) =>
                             self.Move(self.folders.Find(obj => obj.name == name), to);
        
        public static Hacknet.Folder MoveFile(this Hacknet.Folder self, string name, Hacknet.Folder to) =>
                             self.Move(self.files.Find(obj => obj.name == name), to);

        public static Hacknet.Folder Add(this Hacknet.Folder self, Hacknet.Folder folder)
        {
            self.folders.Add(folder);
            return self;
        }

        public static Hacknet.Folder Add(this Hacknet.Folder self, Hacknet.FileEntry file)
        {
            self.files.Add(file);
            return self;
        }

        public static Hacknet.Folder Remove(this Hacknet.Folder self, Hacknet.Folder folder)
        {
            self.folders.Remove(folder);
            return self;
        }

        public static Hacknet.Folder Remove(this Hacknet.Folder self, Hacknet.FileEntry file)
        {
            self.files.Remove(file);
            return self;
        }

        public static Hacknet.Folder Move(this Hacknet.Folder self, Hacknet.Folder src, Hacknet.Folder to)
        {
            if (src == null || to == null) return self;
            self.folders.Remove(src);
            to.folders.Add(src);
            return self;
        }

        public static Hacknet.Folder Move(this Hacknet.Folder self, Hacknet.FileEntry src, Hacknet.Folder to)
        {
            if (src == null || to == null) return self;
            self.files.Remove(src);
            to.files.Add(src);
            return self;
        }
    }
}
