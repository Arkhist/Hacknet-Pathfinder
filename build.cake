var target = Argument("target", "BuildHacknet");
var configuration = Argument("configuration", "Release");
var HacknetDirectoryStr = Argument<String>("hacknet-dir", null);

if(HacknetDirectoryStr == null)
{
	if(IsRunningOnWindows()) {
		if(HasEnvironmentVariable("ProgramFiles(x86)"))
			HacknetDirectoryStr = EnvironmentVariable("ProgramFiles(x86)");
		else
			HacknetDirectoryStr = EnvironmentVariable("ProgramFiles");
		HacknetDirectoryStr += "\\Steam\\steamapps\\common\\Hacknet";

	} else {
		HacknetDirectoryStr = "/home";
		HacknetDirectoryStr += "/"+EnvironmentVariable("USER");
		HacknetDirectoryStr += "/.local/share/Steam/steamapps/common/Hacknet/";
	}
}

var HacknetDirectory = new DirectoryPath(HacknetDirectoryStr);

if(!DirectoryExists(HacknetDirectory) || !FileExists(HacknetDirectory.GetFilePath("Hacknet.exe")))
{
	Error("Could not find valid Hacknet executable in '"+HacknetDirectory+"'");
	throw new Exception("Valid Hacknet.exe not found");
}

public void CheckContainedOrCopy(DirectoryPath working, FilePath copyFrom)
{
	Information("Checking for file '"+copyFrom+"' in '"+working+"'");
	if(!FileExists(working.GetFilePath(copyFrom.GetFilename())))
		CopyFile(copyFrom, working.GetFilePath(copyFrom.GetFilename()));
}

public void CheckAndDeleteFile(FilePath path)
{
	Information("Deleting file '"+path+"'");
	if(FileExists(path))
		DeleteFile(path);
}

public void CheckAndDeleteDirectory(DirectoryPath path, DeleteDirectorySettings settings)
{
	Information("Deleting directory '"+path+"'");
	if(DirectoryExists(path))
		DeleteDirectory(path, settings);
}

public FilePath GetPathForFileExt(FilePath path, string[] exts)
{
	foreach(var e in exts)
	{
		FilePath newPath = path.AppendExtension(e);
		if(FileExists(newPath))
			return newPath;
	}
	throw new Exception("Could not find valid file extension for "+path);
}

public int StartMonoProcess(FilePath path, ProcessSettings settings)
{
	if(IsRunningOnWindows())
		return StartProcess(path, settings);
	else
	{
		settings.Arguments = path + " " + settings.Arguments?.Render();
		return StartProcess("mono", settings);
	}
}

Task("BuildPatcher")
	.Description("Builds PathfinderPatcher.exe\n")
	.Does(() => {
		MSBuild("./PathfinderPatcher/PathfinderPatcher.csproj",
			new MSBuildSettings {
				Configuration = configuration,
				WorkingDirectory = "./lib"
			});
		if(IsRunningOnUnix()) {
			Information("Correcting permissions on Unix");
			StartProcess("chmod", new ProcessSettings {
				Arguments = "+x PathfinderPatcher.exe",
				WorkingDirectory = "./lib"
			});
		}
	});

Task("PatcherSpit")
	.Description("Executing PathfinderPatcher.exe to produce a\n\t\t\t      hookless HacknetPathfinder.exe.\n")
	.IsDependentOn("BuildPatcher")
	.Does(() => {
		Information("Executing PathfinderPatcher for spit.");
		StartMonoProcess("PathfinderPatcher.exe",
			new ProcessSettings{
				Arguments = "-exeDir \"" + HacknetDirectory + "\" -spit",
				WorkingDirectory = "./lib"
			});
	});

Task("BuildPathfinder")
	.Description("Builds an unhooked Pathfinder.dll.\n")
	.IsDependentOn("PatcherSpit")
	.Does(() => {
		CheckContainedOrCopy("./lib", HacknetDirectory.GetFilePath("FNA.dll"));
		CheckContainedOrCopy("./lib", HacknetDirectory.GetFilePath("AlienFXManagedWrapper3.5.dll"));
		CheckContainedOrCopy("./lib", HacknetDirectory.GetFilePath("Steamworks.NET.dll"));
		MSBuild("./Pathfinder.csproj",
			new MSBuildSettings {
				Configuration = configuration,
				WorkingDirectory = "./lib"
			});
		if(IsRunningOnUnix()) {
			Information("Correcting permissions on Unix");
			StartProcess("chmod", new ProcessSettings {
				Arguments = "+x Pathfinder.dll PathfinderPatcher.exe",
				WorkingDirectory = "./lib"
			});
		}
	});

Task("Package")
	.Description("Packages Pathfinder files in to a releases zip.\n")
	.IsDependentOn("BuildPathfinder")
	.Does(() => {
		Information("Copying README.md and LICENSE to lib");
		CopyFile("./README.md", "./lib/README.md");
		CopyFile("./LICENSE", "./lib/LICENSE");
		Information("Zipping releases/Pathfinder.Release.V_.zip");
		Zip("./lib", "./releases/Pathfinder.Release.V_.zip", new []{
			"./lib/PathfinderPatcher.exe",
			//"./lib/PathfinderPatcher.arch64",
			//"./lib/PathfinderPatcher.osx",
			//"./lib/PathfinderPatcher.ubuntu86",
			//"./lib/PathfinderPatcher.ubuntu64",
			"./lib/Pathfinder.dll",
			"./lib/Mono.Cecil.dll",
			"./lib/Mono.Cecil.Inject.dll",
			"./lib/Cecil_LICENSE.txt",
			"./lib/Cecil_Inject_LICENSE.txt",
			"./lib/README.md",
			"./lib/LICENSE"
		});
		Information("Deleting lib/README.md and lib/LICENSE");
		DeleteFile("./lib/README.md");
		DeleteFile("./lib/LICENSE");
	});

Task("BuildHacknet")
	.Description("Finalizes Pathfinder's Patching.\n")
	.IsDependentOn("BuildPathfinder")
	.Does(() => {
		Information("Executing PathfinderPatcher for final Hacknet build.");
		StartMonoProcess("PathfinderPatcher.exe",
			new ProcessSettings{
				Arguments = "-exeDir \"" + HacknetDirectory + "\"",
				WorkingDirectory = "./lib"
			});
	});

Task("RunHacknet")
	.Description("Prepares and runs HacknetPathfinder.exe in the\n\t\t\t      Hacknet Directory.\n\t\t\t      Reverts the Hacknet Directory when complete.\n")
	.IsDependentOn("BuildPathfinder")
	.Does(() => {
		Information("Copying Pathfinder to Hacknet directory, backing up Pathfinder.dll to OldPathfinder.dll.");
		if(FileExists(HacknetDirectory.GetFilePath("Pathfinder.dll"))) {
			if(FileExists(HacknetDirectory.GetFilePath("OldPathfinder.dll")))
				DeleteFile(HacknetDirectory.GetFilePath("Pathfinder.dll"));
			else
				MoveFile(HacknetDirectory.GetFilePath("Pathfinder.dll"), HacknetDirectory.GetFilePath("OldPathfinder.dll"));
		}
		CopyFile("lib/Pathfinder.dll", HacknetDirectory.GetFilePath("Pathfinder.dll"));
		Information("Executing PathfinderPatcher for Hacknet execution.");
		StartMonoProcess(MakeAbsolute(File("lib/PathfinderPatcher.exe")),
			new ProcessSettings{ WorkingDirectory = HacknetDirectory });
		FilePath path = null;
		if(IsRunningOnUnix()) {
			path = GetPathForFileExt(HacknetDirectory.GetFilePath("HacknetPathfinder.bin"), new[] { "x86", "x86_64", "osx" });
			//if(path.GetExtension().Contains("x86"))
			//	StartProcess("declare", new ProcessSettings{ Arguments = "TERM=xterm", WorkingDirectory=HacknetDirectory });
		}
		Information("Starting HacknetPathfinder.");
		StartProcess(path == null
			? HacknetDirectory.GetFilePath("HacknetPathfinder.exe")
			: path);
		Information("Replacing built Pathfinder.dll with OldPathfinder.dll");
		DeleteFile(HacknetDirectory.GetFilePath("Pathfinder.dll"));
		if(FileExists(HacknetDirectory.GetFilePath("OldPathfinder.dll")))
			MoveFile(HacknetDirectory.GetFilePath("OldPathfinder.dll"), HacknetDirectory.GetFilePath("Pathfinder.dll"));
		StartMonoProcess(HacknetDirectory.GetFilePath("PathfinderPatcher.exe"),
			new ProcessSettings{ WorkingDirectory = HacknetDirectory });
	});

Task("BuildDocs")
	.Description("Builds Pathfinder's Documentation into docs.\n")
	.Does(() => {
		Information("Building documentation.");
		StartProcess("doxygen");
	});

Task("Clean")
	.Description("Cleans the built Pathfinder files.")
	.Does(() => {
		CheckAndDeleteFile("./lib/HacknetPathfinder.exe");
		CheckAndDeleteFile("./lib/PathfinderPatcher.exe");
		CheckAndDeleteFile("./lib/Pathfinder.dll");
		CheckAndDeleteFile("./lib/PathfinderPatcher.ubuntu86");
		CheckAndDeleteFile("./lib/PathfinderPatcher.ubuntu64");
		CheckAndDeleteFile("./lib/PathfinderPatcher.arch64");
		CheckAndDeleteDirectory("./bin", new DeleteDirectorySettings { Recursive = true });
		CheckAndDeleteDirectory("./obj", new DeleteDirectorySettings { Recursive = true });
	});

RunTarget(target);