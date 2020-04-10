using System;
using System.Linq;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Game;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Internal
{
    public static class ReplacementsCommon
    {
        public static MemoryContents LoadMemoryContents(ElementInfo root)
        {
            MemoryContents result = new MemoryContents();

            /*
             * I've chosen not to spin up an entire ParsedTreeExecutor here,
             * as it is only four elements and if anyone truly needs this parser extended,
             * we can always add that later.
             */

            var commandsInfo = root.Children.FirstOrDefault(e => e.Name == "Commands");

            if(commandsInfo != null)
            {
                foreach(var commandInfo in commandsInfo.Children)
                {
                    string commandStr = commandInfo.Value;
                    if(commandStr.Contains("\n"))
                    {
                        string[] commands = commandStr.Split(Hacknet.Utils.robustNewlineDelim, StringSplitOptions.None);
                        foreach(string _command in commands)
                        {
                            string command = _command;
                            if(string.IsNullOrEmpty(command))
                                command = " ";
                            result.CommandsRun.Add(Folder.deFilter(command).HacknetFilter());
                        }
                    } else
                        result.CommandsRun.Add(Folder.deFilter(commandStr).HacknetFilter());
                }
            }

            var dataInfo = root.Children.FirstOrDefault(e => e.Name == "Data");

            if(dataInfo != null)
            {
                foreach(var blockInfo in dataInfo.Children)
                {
                    result.DataBlocks.Add(Folder.deFilter(blockInfo.Value).HacknetFilter());
                }
            }

            var fragmentsInfo = root.Children.FirstOrDefault(e => e.Name == "FileFragments");

            if(fragmentsInfo != null)
            {
                foreach(var fileInfo in fragmentsInfo.Children)
                {
                    result.FileFragments.Add(new KeyValuePair<string, string>(
                        Folder.deFilter(fileInfo.Attributes.GetValueOrDefault("name", "UNKNOWN")),
                        Folder.deFilter(fileInfo.Value)
                    ));
                }
            }

            var imagesInfo = root.Children.FirstOrDefault(e => e.Name == "Images");

            if(imagesInfo != null)
            {
                foreach(var imageInfo in imagesInfo.Children)
                {
                    result.Images.Add(Folder.deFilter(imageInfo.Value));
                }
            }

            return result;
        }
    }
}
