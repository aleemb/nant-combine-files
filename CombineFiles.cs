using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using System.Collections.Generic;

// http://www.nabble.com/Re:-All-.js-files-into-1-p19593677.html

/*
Example:

<combine-files tofile="src\Web\all.js" separator=";" order-files="jquery-1.2.6.js,Common.js">
  <fileset basedir="src\Web\scripts">
    <include name="*.js" />
  </fileset>
</combine-files> 

*/


namespace Nant.SvnFunctions
{
    [TaskName("combine-files")]
    public class CombineFilesTask : Task
    {
        private string _toFile;

        [TaskAttribute("tofile")]
        public string ToFile
        {
            get { return Project.GetFullPath(_toFile); }
            set { _toFile = value; }
        }

        /// <summary>
        /// Order files comma separated eg. jQuery.js,other.js
        /// </summary>
        [TaskAttribute("order-files")]
        public string FileOrder { get; set; }

        [BuildElement("fileset")]
        public FileSet FileSet { get; set; }

        [TaskAttribute("separator")]
        public string FileSeparator { get; set; }

        [TaskAttribute("basedir")]
        public string BaseDir { get; set; }

        [TaskAttribute("filelist")]
        public string FileList { get; set; }

        protected override void ExecuteTask()
        {
            if (File.Exists(ToFile)) File.Delete(ToFile);
            if (BaseDir == null) BaseDir = ".";


            var finalFiles = new List<string>();

            if (FileList != null)
            {
                // get file list (ordered)
                var filesToAddTemp = FileList != null ? FileList.Split(',') : new string[] { };
                foreach (var orderedFile in filesToAddTemp)
                {
                    finalFiles.Add(BaseDir + "\\" + orderedFile);
                }
            }

            if (FileSet != null)
            {
                var filesToAdd = new Dictionary<string, string>();

                // get file set
                foreach (var filePath in FileSet.FileNames)
                {
                    filesToAdd.Add(Path.GetFileName(filePath), filePath);
                }

                // order file set
                var filesToOrder = FileOrder != null ? FileOrder.Split(',') : new string[] { };
                foreach (var orderedFile in filesToOrder)
                {
                    if (!filesToAdd.ContainsKey(orderedFile)) continue;

                    finalFiles.Add(filesToAdd[orderedFile]);
                    filesToAdd.Remove(orderedFile);
                }

                finalFiles.AddRange(filesToAdd.Values);
            }


            Log(Level.Info, "Creating " + ToFile);
            foreach (var filePath in finalFiles)
            {
                Log(Level.Info, "  Appending " + filePath);
                File.AppendAllText(ToFile, File.ReadAllText(filePath), Encoding.UTF8);
                if (!string.IsNullOrEmpty(FileSeparator))
                    File.AppendAllText(ToFile, FileSeparator, Encoding.UTF8);
            }
        }
    }
}