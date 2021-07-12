using System;
using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    public static class ReplacementsCommon
    {
        public static MemoryContents LoadMemoryContents(ElementInfo info)
        {
            var memory = new MemoryContents();

            if (info.Children.TryGetElement("Commands", out var commands))
            {
                foreach (var command in commands.Children.Select(x => x.Content))
                {
                    if (command.Contains("\n"))
                    {
                        foreach (var trueCommand in command.Split(Utils.robustNewlineDelim, StringSplitOptions.RemoveEmptyEntries))
                        {
                            memory.CommandsRun.Add(ComputerLoader.filter(Folder.deFilter(string.IsNullOrEmpty(trueCommand) ? " " : trueCommand)));
                        }
                    }
                    else
                        memory.CommandsRun.Add(ComputerLoader.filter(Folder.deFilter(command)));
                }
            }

            if (info.Children.TryGetElement("Data", out var data))
            {
                foreach (var block in data.Children)
                {
                    memory.DataBlocks.Add(ComputerLoader.filter(Folder.deFilter(block.Content)));
                }
            }

            if (info.Children.TryGetElement("FileFragments", out var files))
            {
                foreach (var file in files.Children)
                {
                    memory.FileFragments.Add(new KeyValuePair<string, string>(
                        Folder.deFilter(file.Attributes.GetString("name", "UNKNOWN")),
                        Folder.deFilter(file.Content)
                    ));
                }
            }

            if (info.Children.TryGetElement("Images", out var images))
            {
                foreach (var image in images.Children)
                {
                    memory.Images.Add(Folder.deFilter(image.Content));
                }
            }
            
            return memory;
        }
    }
}
