#if IS_DOXYGEN
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

/// @defgroup HacknetGroup Hacknet
/// @brief The Vanilla Game
///
/// The namespace containing all Hacknet vanilla API classes and systems Pathfinder is thus mostly based upon.

/// @ingroup HacknetGroup
/// @brief The Vanilla Game's namespace
///
/// The namespace containing all Hacknet vanilla API classes and systems Pathfinder is thus mostly based upon.
/// All Game systems not found in this namespace can be located at
/// https://msdn.microsoft.com/en-us/library/bb200104.aspx
namespace Hacknet
{
    public class Game1 : Game
    {
        public static bool threadsExiting;
        /// <summary>
        /// Current CultureInfo of the game.
        /// </summary>
        public static CultureInfo culture;
        /// <summary>
        /// Original CultureInfo?
        /// </summary>
        public static CultureInfo OriginalCultureInfo;
        public static string AutoLoadExtensionPath;

        public GraphicsDeviceManager graphics;
        public GraphicsDeviceInformation graphicsInfo;
        public ScreenManager sman;

        public Game1 getSingleton();
        public void setWindowPosition(Vector2 pos);
    }

    public abstract class GameScreen
    {
        /// <summary>
        /// Retrieves the screen's controlling player's index, or <c>null</c> if it does not exist
        /// </summary>
        public PlayerIndex? ControllingPlayer { get; internal set; }
        /// <summary>
        /// Retrieves <c>true</c> if the screen is active, <c>false</c> otherwise.
        /// </summary>
        public bool IsActive { get; }
        /// <summary>
        /// Retrieves <c>true</c> if the screen is exiting, <c>false</c> otherwise.
        /// </summary>
        public bool IsExiting { get; protected set; }
        /// <summary>
        /// Retrieves <c>true</c> if the screen is a popup, <c>false</c> otherwise.
        /// </summary>
        public bool IsPopup { get; protected set; }
        public ScreenManager ScreenManager { get; internal set; }
        public ScreenState ScreenState { get; protected set; }
        public byte TransitionAlpha { get; }
        public TimeSpan TransitionOffTime { get; protected set; }
        public TimeSpan TransitionOnTime { get; protected set; }
        public float TransitionPosition { get; protected set; }

        /// <summary>
        /// Draws the screen given the delta of gameTime.
        /// </summary>
        /// <param name="gameTime">The delta of last draw</param>
        public virtual void Draw(GameTime gameTime);
        /// <summary>
        /// Exits the screen.
        /// </summary>
        public void ExitScreen();
        /// <summary>
        /// Handles general input.
        /// </summary>
        public virtual void HandleInput(InputState input);
        /// <summary>
        /// Marks input method to change.
        /// </summary>
        /// <param name="usingGamePad">If set to <c>true</c> then input changes to game pad.</param>
        public virtual void inputMethodChanged(bool usingGamePad);
        /// <summary>
        /// Unloads content if need be.
        /// </summary>
        public virtual void UnloadContent();
        /// <summary>
        /// Loads content.
        /// </summary>
        public virtual void LoadContent();
        /// <summary>
        /// Updates the screen given the delta of gameTime gameTime.
        /// </summary>
        /// <param name="gameTime">The delta of last update.</param>
        /// <param name="otherScreenHasFocus">If other screen has focus then is <c>true</c>, otherwise <c>false</c>.</param>
        /// <param name="coveredByOtherScreen">If covered by another screen then is <c>true</c>, otherwise <c>false</c>.</param>
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen);
    }

    public class OS : GameScreen
    {
        public static bool TestingPassOnly = false;
        public static bool DEBUG_COMMANDS = Settings.debugCommandsEnabled;
        public static float EXE_MODULE_HEIGHT = 250f;
        public static float TCP_STAYALIVE_TIMER = 10f;
        public static float WARNING_FLASH_TIME = 2f;
        public static int TOP_BAR_HEIGHT = 21;
        /// <summary>
        /// The current main client OS instance.
        /// </summary>
        public static OS currentInstance;
        public static bool WillLoadSave = false;
        public static object displayObjectCache = null;
        public static float operationProgress = 0f;
        public static double currentElapsedTime = 0.0;

        public BootCrashAssistanceModule BootAssitanceModule;
        public RunnableConditionalActions ConditionalActions;
        public bool IsDLCSave;
        public string PreDLCVisibleNodesCache;
        public bool IsDLCConventionDemo;
        public ActiveEffectsUpdater EffectsUpdater;
        /// <summary>
        /// The designated override action for a trace completion.
        /// </summary>
        public Action traceCompleteOverrideAction;
        public List<OS.TrackerDetail> TrackersInProgress;
        public Action postFXDrawActions;
        public Action<float> UpdateSubscriptions;
        public string PreDLCFaction;
        public Color defaultHighlightColor;
        public bool ShowDLCAlertsIcon;
        public HubServerAlertsIcon hubServerAlertsIcon;
        public float timer;
        public bool HasLoadedDLCContent;
        public string connectedIPLastFrame;
        public string homeNodeID;
        public string homeAssetServerID;
        public bool DisableTopBarButtons;
        public bool DisableEmailIcon;
        public bool IsInDLCMode;
        public bool HasExitedAndEnded;
        public ProgressionFlags Flags;
        public List<KeyValuePair<string, string>> ActiveHackers;
        public Color defaultTopBarColor;
        public SoundEffect beepSound;
        public bool terminalOnlyMode;
        public Color warningColor;
        public Color indentBackgroundColor;
        public Color subtleTextColor;
        public Color connectedNodeHighlight;
        public Color exeModuleTopBar;
        public Color exeModuleTitleText;
        public Color netmapToolTipColor;
        public Color netmapToolTipBackground;
        public Color displayModuleExtraLayerBackingColor;
        public Color topBarIconsColor;
        public Color BackgroundImageFillColor;
        public bool UseAspectPreserveBackgroundScaling;
        public Color AFX_KeyboardMiddle;
        public Color AFX_KeyboardOuter;
        public Color AFX_WordLogo;
        public Color AFX_Other;
        public Color thisComputerNode;
        public Color scanlinesColor;
        public Color superLightWhite;
        public Color highlightColor;
        public Color topBarTextColor;
        public Color semiTransText;
        public Color darkBackgroundColor;
        public ActionDelayer delayer;
        public Color outlineColor;
        public Color lockedColor;
        public Color brightLockedColor;
        public Color brightUnlockedColor;
        public Color unlockedColor;
        public Color lightGray;
        public Color shellColor;
        public Color shellButtonColor;
        public Color moduleColorSolid;
        public Color moduleColorSolidDefault;
        public Color moduleColorStrong;
        public Color moduleColorBacking;
        public Color topBarColor;
        public Color terminalTextColor;
        public bool validCommand;
        public bool commandInvalid;
        public TraceDangerSequence TraceDangerSequence;
        public IntroTextModule introTextModule;
        public GameTime lastGameTime;
        public UserDetail defaultUser;
        /// <summary>
        /// The Terminal module of the OS.
        /// </summary>
        public Terminal terminal;
        /// <summary>
        /// The NetworkMap module of the OS.
        /// </summary>
        public NetworkMap netMap;
        /// <summary>
        /// The CrashModule of the OS.
        /// </summary>
        public CrashModule crashModule;
        /// <summary>
        /// The DisplayModule of the OS.
        /// </summary>
        public DisplayModule display;
        public IncomingConnectionOverlay IncConnectionOverlay;
        public AircraftInfoOverlay AircraftInfoOverlay;
        public ActiveMission currentMission;
        public List<ActiveMission> branchMissions = new List<ActiveMission>();
        public TraceTracker traceTracker;
        /// <summary>
        /// The RamModule of the OS.
        /// </summary>
        public RamModule ram;
        public Rectangle fullscreen;
        public List<ShellExe> shells;
        public bool FirstTimeStartup = Settings.slowOSStartup;
        public bool initShowsTutorial = Settings.initShowsTutorial;
        public bool inputEnabled = false;
        public bool isLoaded = false;
        public float PorthackCompleteFlashTime = 0f;
        public float MissionCompleteFlashTime = 0f;
        public string username = "";
        public int totalRam = 800 - (TOP_BAR_HEIGHT + 2) - RamModule.contentStartOffset;
        public int ramAvaliable = 800 - (TOP_BAR_HEIGHT + 2);
        public int currentPID = 0;
        /// <summary>
        /// The ExeModule list, or the executables executing on the OS.
        /// </summary>
        public List<ExeModule> exes;
        public List<string> shellIPs;
        public MailIcon mailicon;
        public bool isServer = false;
        public bool canRunContent;
        public float stayAliveTimer;
        public string opponentLocation;
        public string displayCache;
        public string getStringCache;
        public bool multiplayer = false;
        public AllFactions allFactions;
        public string connectedIP = "";
        /// <summary>
        /// The OS' owned computer.
        /// </summary>
        public Computer thisComputer = null;
        /// <summary>
        /// The OS' network connected computer.
        /// </summary>
        public Computer connectedComp = null;
        public Computer opponentComputer = null;
        public float warningFlashTimer = 0f;
        public Faction currentFaction;
        public EndingSequenceModule endingSequence;
        public ContentManager content;
        public float gameSavedTextAlpha = -1f;
        public string SaveGameUserName = "";
        public string SaveUserAccountName = null;
        public string SaveUserPassword = "password";
        public List<int> navigationPath = new List<int>();

        /// <summary>
        /// Adds an ExeModule to exes.
        /// </summary>
        /// <param name="exe">The ExeModule to add.</param>
        public void addExe(ExeModule exe);
        public void clientDisconnected();
        public void connectedComputerCrashed(Computer c);
        public override void Draw(GameTime gameTime);
        public void drawBackground();
        public void drawModules(GameTime gameTime);
        public void drawScanlines();
        public void endMultiplayerMatch(bool won);
        /// <summary>
        /// Executes the specified text as though it was typed in the Terminal.
        /// </summary>
        /// <param name="text">The text to execute.</param>
        public void execute(string text);
        /// <summary>
        /// Triggers the boot to fail to Terminal.
        /// </summary>
        public void failBoot();
        /// <summary>
        /// Gets the bounds of which all standard executable displays are within.
        /// </summary>
        public Rectangle getExeBounds();
        /// <see cref="failBoot()"/>
        public void graphicsFailBoot();
        public override void HandleInput(InputState input);
        public bool hasConnectionPermission(bool admin);
        public override void inputMethodChanged(bool usingGamePad);
        public void launchExecutable(string exeName,
                                     string exeFileData,
                                     int targetPort,
                                     string[] allParams = null,
                                     string originalName = null);
        public void loadBranchMissionsSaveData(XmlReader reader);
        public override void LoadContent();
        public void LoadExtraTitleSaveData(XmlReader rdr);
        public void loadMissionNodes();
        public void loadMultiplayerMission();
        public void loadOtherSaveData(XmlReader reader);
        public void loadSaveFile();
        public void loadTitleSaveData(XmlReader reader);
        public void quitGame(object sender, PlayerIndexEventArgs e);
        /// <summary>
        /// Reboots the thisComputer.
        /// </summary>
        public void rebootThisComputer();
        /// <summary>
        /// Refreshes the theme.
        /// </summary>
        public void RefreshTheme();
        public void RequestRemovalOfAllPopups();
        public void runCommand(string text);
        /// <summary>
        /// Saves the game.
        /// </summary>
        public void saveGame();
        /// <summary>
        /// Writes the message to the network stream.
        /// </summary>
        public void sendMessage(string message);
        /// <summary>
        /// Sets the mouse visiblity.
        /// </summary>
        /// <param name="mouseIsVisible">If set to <c>true</c> mouse is visible.</param>
        public void setMouseVisiblity(bool mouseIsVisible);
        public void sucsesfulBoot();
        /// <summary>
        /// Gives admin privilege of the connectedComputer to the ip of thisComputer
        /// </summary>
        public void takeAdmin();
        /// <summary>
        /// Gives admin privilege of the ip to the ip of thisComputer
        /// </summary>
        /// <param name="ip">The ip of the computer to give admin privilege from.</param>
        public void takeAdmin(string ip);
        /// <summary>
        /// Crashes the thisComputer.
        /// </summary>
        public void thisComputerCrashed();
        /// <summary>
        /// Resets the thisComputer's ip address.
        /// </summary>
        public void thisComputerIPReset();
        public void threadedSaveExecute(bool preventSaveText = false);
        public void timerExpired();
        public override void UnloadContent();
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen);
        public void warningFlash();
        /// <summary>
        /// Write the specified text to Terminal.
        /// </summary>
        /// <param name="text">The text to write to the terminal.</param>
        public void write(string text);
        /// <summary>
        /// Writes the specified text to Terminal without any special serialization of the text.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void writeSingle(string text);

        public struct TrackerDetail
        {
            public Computer comp;
            public float timeLeft;
        }
    }

    [Serializable]
    public class Computer
    {
        public const byte CORPORATE = 1;
        public static float BASE_PROXY_TICKS = 30f;
        public static float BASE_REBOOT_TIME = 10.5f;
        public static float BASE_BOOT_TIME = Settings.isConventionDemo ? 15f : 25.5f;
        public static float BASE_TRACE_TIME = 15f;
        public const byte EMPTY = 4;
        public const byte SERVER = 3;
        public const byte HOME = 2;
        public const byte EOS = 5;

        public float proxyOverloadTicks = 0f;
        public float highlightFlashTime = 0f;
        public byte type;
        public string adminPass;
        /// <summary>
        /// The active Daemons on Computer
        /// </summary>
        public List<Daemon> daemons;
        public bool AllowsDefaultBootModule = true;
        public bool hasProxy = false;
        public float startingOverloadTicks = -1f;
        /// <summary>
        /// The firewall for Computer.
        /// </summary>
        public Firewall firewall = null;
        /// <summary>
        /// The reporting shell for Computer.
        /// </summary>
        public ShellExe reportingShell = null;
        public ExternalCounterpart externalCounterpart = null;
        public string attatchedDeviceIDs = null;
        public bool firewallAnalysisInProgress = false;
        public bool HasTracker = false;
        public Administrator admin = null;
        public bool proxyActive = false;
        public string icon = null;
        public UserDetail currentUser;
        public string name;
        public string idName;
        public string ip;
        /// <summary>
        /// The NetworkMap location of the Computer.
        /// </summary>
        public Vector2 location;
        /// <summary>
        /// The FileSystem for Computer.
        /// </summary>
        public FileSystem files;
        public int securityLevel;
        public float traceTime;
        public int portsNeededForCrack = 0;
        public string adminIP;
        public List<UserDetail> users;
        public List<int> links;
        /// <summary>
        /// The ports on Computer.
        /// </summary>
        public List<int> ports;
        /// <summary>
        /// The location on ports of the opened ports.
        /// </summary>
        public List<byte> portsOpen;
        public bool silent = false;
        public bool disabled = false;
        public MemoryContents Memory;
        public bool userLoggedIn = false;
        public Dictionary<int, int> PortRemapping = null;

        public Computer(string compName, string compIP, Vector2 compLocation, int seclevel, byte compType);
        public Computer(string compName, string compIP, Vector2 compLocation, int seclevel);
        public Computer(string compName, string compIP, Vector2 compLocation, int seclevel, byte compType, OS opSystem);

        public static string generateBinaryString(int length);
        public static string generateBinaryString(int length, MSRandom rng);
        public static Folder getFolderAtDepth(Computer c, int depth, List<int> path);
        public static Computer load(XmlReader reader, OS os);
        public static Computer loadFromFile(string filename);

        public void addFirewall(int level);
        public void addFirewall(int level, string solution);
        public void addFirewall(int level, string solution, float additionalTime);
        public void addMultiplayerTargetFile();
        public void addNewUser(string ipFrom, string name, string pass, byte type);
        public void addNewUser(string ipFrom, UserDetail usr);
        public void addProxy(float time);
        public void bootupTick(float t);
        public bool canCopyFile(string ipFrom, string name);
        public bool canReadFile(string ipFrom, FileEntry f, int index);
        public void closeCDTray(string ipFrom);
        public void closePort(int portNum, string ipFrom);
        public bool connect(string ipFrom);
        public void crash(string ipFrom);
        public bool deleteFile(string ipFrom, string name, List<int> folderPath);
        public void disconnecting(string ipFrom, bool externalDisconnectToo = true);
        public void forkBombClients(string ipFrom);
        public string generateFileData(int seed);
        public string generateFileName(int seed);
        public string generateFolderName(int seed);
        public FileSystem generateRandomFileSystem();
        public int GetCodePortNumberFromDisplayPort(int displayPort);
        public Daemon getDaemon(Type t);
        public int GetDisplayPortNumberFromCodePort(int codePort);
        public Folder getFolderFromPath(string path, bool createFoldersThatDontExist = false);
        public List<int> getFolderPath(string path, bool createFoldersThatDontExist = false);
        public string getSaveString();
        public Vector2 getScreenSpacePosition();
        public string getTooltipString();
        public void giveAdmin(string ipFrom);
        public void hostileActionTaken();
        public void initDaemons();
        public bool isPortOpen(int portNum);
        public void log(string message);
        public virtual int login(string username, string password, byte type = 1);
        public bool makeFile(string ipFrom, string name, string data, List<int> folderPath, bool isUpload = false);
        public bool makeFolder(string ipFrom, string name, List<int> folderPath);
        public bool moveFile(string ipFrom, string name, string newName, List<int> folderPath, List<int> destFolderPath);
        public void openCDTray(string ipFrom);
        public void openPort(int portNum, string ipFrom);
        public void openPortsForSecurityLevel(int security);
        public bool PlayerHasAdminPermissions();
        public void reboot(string ipFrom);
        public void sendNetworkMessage(string s);
        public void setAdminPassword(string newPass);
        public override string ToString();
    }

    public class Daemon
    {
        public string name;
        public bool isListed;
        public Computer comp;
        public OS os;

        public Daemon(Computer computer, string serviceName, OS opSystem);

        public static bool validUser(byte type);

        public virtual void draw(Rectangle bounds, SpriteBatch sb);
        public virtual string getSaveString();
        public virtual void initFiles();
        public virtual void loadInit();
        public virtual void navigatedTo();
        public void registerAsDefaultBootDaemon();
        public virtual void userAdded(string name, string pass, byte type);
    }

    public class Module
    {
        public static int PANEL_HEIGHT = 15;

        public Rectangle bounds;
        public SpriteBatch spriteBatch;
        public OS os;
        public string name = "Unknown";
        public bool visible = true;

        public Rectangle Bounds { get; set }

        public Module(Rectangle location, OS operatingSystem);

        public virtual void Draw(float t);
        public void drawFrame();
        public virtual void LoadContent();
        public virtual void PostDrawStep();
        public virtual void PreDrawStep();
        public virtual void Update(float t);
    }

    public class CrashModule : Module
    {
        public static float BLUESCREEN_TIME = 8f;
        public static float BOOT_TIME = Settings.isConventionDemo ? 5f : (Settings.FastBootText ? 1.2f : 14.5f);
        public static float BLACK_TIME = 2f;
        public static float POST_BLACK_TIME = 1f;

        public float elapsedTime = 0f;
        public int state = 0;
        public Color bluescreenBlue = new Color(0, 0, 170);
        public string BootLoadErrors = "";
        public Color textColor = new Color(0, 0, 255);
        public Color bluescreenGrey = new Color(167, 167, 167);

        public CrashModule(Rectangle location, OS operatingSystem)

        public string checkOSBootFiles(string bootString);
        public void completeReboot();
        public override void Draw(float t);
        public override void LoadContent();
        public void reset();
        public override void Update(float t);
    }

    public class CoreModule : Module
    {
        public bool inputLocked = false;
        public CoreModule(Rectangle location, OS operatingSystem) : base(location, operatingSystem);

        public override void LoadContent();
        public override void PostDrawStep();
        public override void PreDrawStep();
    }

    public class ExeModule : Module
    {
        public static float FADEOUT_RATE = 0.5f;
        public static float MOVE_UP_RATE = 350f;
        public static int DEFAULT_RAM_COST = 246;

        public string IdentifierName = "UNKNOWN";
        public bool needsProxyAccess = false;
        public int ramCost = DEFAULT_RAM_COST;
        public string targetIP = "";
        public bool needsRemoval = false;
        public bool isExiting = false;
        public float fade = 1f;
        public int PID = 0;
        public float moveUpBy = 0f;

        public ExeModule(Rectangle location, OS operatingSystem);

        public virtual void Completed();
        public override void Draw(float t);
        public virtual void drawOutline();
        public virtual void drawTarget(string typeName = "app:");
        public Rectangle GetContentAreaDest();
        public virtual void Killed();
        public override void LoadContent();
        public override void Update(float t);
    }

    public class NotesExe : ExeModule
    {
        public const string NotesReopenOnLoadFile = "Notes_Reopener.bat";

        public List<string> notes = new List<string>();
        public float MemoryWarningFlashTime = 0f;

        public NotesExe(Rectangle location, OS operatingSystem);

        public static void AddNoteToOS(string note, OS os, bool isRecursiveSelfAdd = false);
        public static bool NoteExists(string note, OS os);

        public void AddNote(string note);
        public void DisplayOutOfMemoryWarning();
        public override void Draw(float t);
        public bool HasNote(string note);
        public override void Killed();
        public override void LoadContent();
        public override void Update(float t);
    }

    public class ShellExe : ExeModule
    {
        public static int INFOBAR_HEIGHT = 16;
        public static int BASE_RAM_COST = 40;
        public static float RAM_CHANGE_PS = 200f;
        public static int TRAP_RAM_USE = 100;

        public string destinationIP = "";

        public ShellExe(Rectangle location, OS operatingSystem);

        public void cancelTarget();
        public override void Completed();
        public void completedAction(int action);
        public void doControlButtons();
        public void doGui();
        public override void Draw(float t);
        public override void LoadContent();
        public void reportedTo(string data);
        public void StartOverload();
        public override void Update(float t);
    }

    public class Terminal : CoreModule
    {
        public static float PROMPT_OFFSET = 0f;

        public string currentLine;
        public string lastRunCommand;
        public string prompt;
        public bool usingTabExecution = false;
        public bool preventingExecution = false;
        public bool executionPreventionIsInteruptable = false;

        public Terminal(Rectangle location, OS operatingSystem);

        public void clearCurrentLine();
        public int commandsRun();
        public void doGui();
        public void doTabComplete();
        public override void Draw(float t);
        public void executeLine();
        public string getLastRunCommand();
        public List<string> GetRecentTerminalHistoryList();
        public string GetRecentTerminalHistoryString();
        public override void LoadContent();
        public void NonThreadedInstantExecuteLine();
        public void reset();
        public override void Update(float t);
        public void write(string text);
        public void writeLine(string text);
    }

    public class RamModule : CoreModule
    {
        public static int contentStartOffset = 16;
        public static int MODULE_WIDTH = 252;
        public static Color USED_RAM_COLOR = new Color(60, 60, 67);
        public static float FLASH_TIME = 3f;

        public RamModule(Rectangle location, OS operatingSystem);

        public override void Draw(float t);
        public virtual void drawOutline();
        public void FlashMemoryWarning();
        public override void LoadContent();
        public override void Update(float t);
    }

    public class DisplayModule : CoreModule
    {
        public Texture2D lockSprite;
        public int y;
        public int x;
        public string LastDisplayedFileSourceIP = null;
        public Folder LastDisplayedFileFolder = null;
        public string[] commandArgs;
        public Texture2D openLockSprite;
        public string command = "";

        public DisplayModule(Rectangle location, OS operatingSystem);

        public static string cleanSplitForWidth(string s, int width);
        public static string splitForWidth(string s, int width);
        public static string splitForWidth(string s, int width, bool correct);

        public override void Draw(float t);
        public void forceLogin(string username, string pass);
        public Texture2D GetComputerImage(Computer comp);
        public override void LoadContent();
        public void typeChanged();
        public override void Update(float t);
    }

    public class NetworkMap : CoreModule
    {
        public static int NODE_SIZE = 26;
        public static float ADMIN_CIRCLE_SCALE = 0.62f;
        public static float PULSE_DECAY = 0.5f;
        public static float PULSE_FREQUENCY = 0.8f;

        public Computer academicDatabase;
        public Computer mailServer;
        public NetmapSortingAlgorithm SortingAlgorithm = NetmapSortingAlgorithm.Scatter;
        public bool DimNonConnectedNodes = false;
        public ConnectedNodeEffect adminNodeEffect;
        public ConnectedNodeEffect nodeEffect;
        public Computer lastAddedNode;
        public List<Corporation> corporations;
        public List<int> visibleNodes;
        public List<Computer> nodes;

        public NetworkMap(Rectangle location, OS operatingSystem);

        public static string generateRandomIP();

        public void CleanVisibleListofDuplicates();
        public bool collides(Vector2 location, float minSeperation = -1f);
        public void discoverNode(Computer c);
        public void discoverNode(string cName);
        public void doGui(float t);
        public override void Draw(float t);
        public void drawLine(Vector2 origin, Vector2 dest, Vector2 offset);
        public List<Corporation> generateCorporations();
        public List<Computer> generateDemoNodes();
        public List<Computer> generateGameNodes();
        public List<Computer> generateNetwork(OS os);
        public List<Computer> generateSPNetwork(OS os);
        public Vector2 GetNodeDrawPos(Computer node, int nodeIndex);
        public Vector2 GetNodeDrawPos(Computer node);
        public Vector2 GetNodeDrawPosDebug(Vector2 nodeLocation);
        public Vector2 getRandomPosition();
        public string getSaveString();
        public string getVisibleNodesString();
        public void load(XmlReader reader);
        public override void LoadContent();
        public void randomizeNetwork();
        public override void Update(float t);
    }

    public enum NetmapSortingAlgorithm
    {
        Scatter,
        Grid,
        Chaos,
        LockGrid
    }

    public class Helpfile
    {
        public static List<string> help;
        public static string prefix =
            "---------------------------------\n" + LocaleTerms.Loc("Command List - Page [PAGENUM] of [TOTALPAGES]") + ":\n";

        public static int getNumberOfPages();
        public static void init();
        public static void writeHelp(OS os, int page = 0);
    }

    public static class GuiData
    {
        public static SpriteFont UITinyfont;
        public static int enganged = -1;
        public static SpriteBatch spriteBatch;
        public static SpriteFont font;
        public static SpriteFont titlefont;
        public static SpriteFont smallfont;
        public static SpriteFont tinyfont;
        public static SpriteFont UISmallfont;
        public static SpriteFont detailfont;
        public static int hot = -1;
        public static bool blockingInput = false;
        public static bool willBlockTextInput = false;
        public static Vector2 scrollOffset = Vector2.Zero;
        public static float lastTimeStep = 0.016f;
        public static List<GuiData.FontCongifOption> FontConfigs = new List<GuiData.FontCongifOption>();
        public static Dictionary<string, List<GuiData.FontCongifOption>> LocaleFontConfigs = new Dictionary<string, List<GuiData.FontCongifOption>>();
        public static GuiData.FontCongifOption ActiveFontConfig = default(GuiData.FontCongifOption);
        public static bool blockingTextInput = false;
        public static int active = -1;
        public static Color Default_Trans_Grey_Solid = new Color(100, 100, 100, 255);
        public static Vector2 temp = default(Vector2);
        public static Color tmpColor = default(Color);
        public static Rectangle tmpRect = default(Rectangle);
        public static MouseState lastMouse;
        public static MouseState mouse;
        public static InputState lastInput;
        public static Color Default_Selected_Color = new Color(0, 166, 235);
        public static int lastMouseWheelPos = -1;
        public static Color Default_Backing_Color = new Color(30, 30, 50, 100);
        public static Color Default_Unselected_Color = new Color(255, 128, 0);
        public static Color Default_Lit_Backing_Color = new Color(255, 199, 41, 100);
        public static Color Default_Dark_Neutral_Color = new Color(10, 10, 15, 200);
        public static Color Default_Dark_Background_Color = new Color(40, 40, 45, 180);
        public static Color Default_Trans_Grey = new Color(30, 30, 30, 100);
        public static Color Default_Trans_Grey_Bright = new Color(60, 60, 60, 100);
        public static Color Default_Trans_Grey_Dark = new Color(20, 20, 20, 200);
        public static Color Default_Trans_Grey_Strong = new Color(80, 80, 80, 100);
        public static Color Default_Light_Backing_Color = new Color(80, 80, 100, 255);

        public static void ActivateFontConfig(GuiData.FontCongifOption config);
        public static void ActivateFontConfig(string configName);
        public static void doInput(InputState input);
        public static void doInput();
        public static void endDraw();
        public static char[] getFilteredKeys();
        public static KeyboardState getKeyboadState();
        public static KeyboardState getLastKeyboadState();
        public static Point getMousePoint();
        public static Vector2 getMousePos();
        public static float getMouseWheelScroll();
        public static void init(GameWindow window);
        public static void InitFontOptions(ContentManager content);
        public static bool isMouseLeftDown();
        public static bool mouseLeftUp();
        public static bool mouseWasPressed();
        public static void setTimeStep(float t);
        public static void startDraw();

        public struct FontCongifOption
        {
            public SpriteFont smallFont;
            public SpriteFont tinyFont;
            public SpriteFont bigFont;
            public SpriteFont detailFont;
            public string name;
            public float tinyFontCharHeight;
        }
    }

    public static class ProgramList
    {
        public static List<string> programs;

        public static List<string> getExeList(OS os);
        public static void init();
    }

    public static class ProgramRunner
    {
        public static bool ExecuteProgram(object os_object, string[] arguments);
        public static bool ExeProgramExists(string name, object binariesFolder);
    }

    public static class Programs
    {
        public static void addNote(string[] args, OS os);
        public static void analyze(string[] args, OS os);
        public static void cat(string[] args, OS os);
        public static void cd(string[] args, OS os);
        public static void cdDrive(bool open);
        public static void clear(string[] args, OS os);
        public static bool computerExists(OS os, string ip);
        public static void connect(string[] args, OS os);
        public static void disconnect(string[] args, OS os);
        public static void doDots(int num, int msDelay, OS os);
        public static void execute(string[] args, OS os);
        public static void fastHack(string[] args, OS os);
        public static void firstTimeInit(string[] args, OS os, bool callWasRecursed = false);
        public static Computer getComputer(OS os, string ip_Or_ID_or_Name);
        public static Folder getCurrentFolder(OS os);
        public static Folder getFolderAtDepth(OS os, int depth);
        public static Folder getFolderAtPath(string path, OS os, Folder rootFolder = null, bool returnsNullOnNoFind = false);
        public static Folder getFolderAtPathAsFarAsPossible(string path, OS os, Folder rootFolder, out string likelyFilename);
        public static Folder getFolderAtPathAsFarAsPossible(string path, OS os, Folder rootFolder);
        public static Folder getFolderFromNavigationPath(List<int> path, Folder startFolder, OS os);
        public static List<int> getNavigationPathAtPath(string path, OS os, Folder currentFolder = null);
        public static void getString(string[] args, OS os);
        public static void kill(string[] args, OS os);
        public static void login(string[] args, OS os);
        public static void ls(string[] args, OS os);
        public static void mv(string[] args, OS os);
        public static void opCDTray(string[] args, OS os, bool isOpen);
        public static bool parseStringFromGetStringCommand(OS os, out string data);
        public static void probe(string[] args, OS os);
        public static void ps(string[] args, OS os);
        public static void reboot(string[] args, OS os);
        public static void replace(string[] args, OS os);
        public static void replace2(string[] args, OS os);
        public static void revealAll(string[] args, OS os);
        public static void rm(string[] args, OS os);
        public static void rm2(string[] args, OS os);
        public static void scan(string[] args, OS os);
        public static void scp(string[] args, OS os);
        public static void solve(string[] args, OS os);
        public static void sudo(OS os, Action action);
        public static void typeOut(string s, OS os, int delay = 50);
        public static void upload(string[] args, OS os);
    }

    public static class TextureBank
    {
        public static List<LoadedTexture> textures = new List<LoadedTexture>();

        public static Texture2D getIfLoaded(string filename);
        public static Texture2D load(string filename, ContentManager content);
        public static void unload(Texture2D tex);
        public static void unloadWithoutRemoval(Texture2D tex);
    }

    public static class Settings
    {
        public static bool MenuStartup = true;
        public static bool IsInExtensionMode = false;
        public static bool DrawHexBackground = true;
        public static bool StartOnAltMonitor = false;
        public static bool isDemoMode = false;
        public static bool isPressBuildDemo = false;
        public static bool isConventionDemo = false;
        public static bool isLockedDemoMode = false;
        public static bool isSpecialTestBuild = false;
        public static bool lighterColorHexBackground = false;
        public static string ConventionLoginName = "Agent";
        public static bool MultiLingualDemo = false;
        public static bool DLCEnabledDemo = true;
        public static bool ShuffleThemeOnDemoStart = true;
        public static bool HasLabyrinthsDemoStartMainMenuButton = false;
        public static bool ForceEnglish = false;
        public static bool IsExpireLocked = false;
        public static DateTime ExpireTime = Utils.SafeParseDateTime("10/06/2017 23:59:01");
        public static bool windowed = false;
        public static bool isServerMode = false;
        public static bool initShowsTutorial = osStartsWithTutorial;
        public static bool isPirateBuild = false;
        public static bool slowOSStartup = true;
        public static bool osStartsWithTutorial = slowOSStartup;
        public static bool isAlphaDemoMode = false;
        public static bool soundDisabled = false;
        public static bool debugCommandsEnabled = false;
        public static bool testingMenuItemsEnabled = false;
        public static bool debugDrawEnabled = false;
        public static bool forceCompleteEnabled = false;
        public static bool emergencyForceCompleteEnabled = true;
        public static bool emergencyDebugCommandsEnabled = true;
        public static bool AllTraceTimeSlowed = false;
        public static bool FastBootText = false;
        public static bool AllowExtensionMode = true;
        public static bool AllowExtensionPublish = false;
        public static bool EducationSafeBuild = false;
        public static string ActiveLocale = "en-us";
        public static bool EnableDLC = true;
        public static bool sendsDLC1PromoEmailAtEnd = true;
        public static bool recoverFromErrorsSilently = true;
    }

    public static class MusicManager
    {
        public static bool dataLoadedFromOutsideFile = false;
        public static string currentSongName;
        public static bool isPlaying;
        public static Song curentSong;
        public static float FADE_TIME;
        public static bool isMuted;

        public static float getVolume();
        public static void init(ContentManager content);
        public static void loadAsCurrentSong(string songname);
        public static void loadAsCurrentSongUnsafe(string songname);
        public static void playSong();
        public static void playSongImmediatley(string songname);
        public static void setIsMuted(bool muted);
        public static void setVolume(float volume);
        public static void stop();
        public static void toggleMute();
        public static void transitionToSong(string songName);
        public static void Update(float t);
    }

    public static class LocaleTerms
    {
        public static Dictionary<string, string> ActiveTerms = new Dictionary<string, string>();

        public static void ClearForEnUS();
        public static string Loc(string input);
        public static void ReadInTerms(string termsFilepath, bool clearPreviouslyLoadedTerms = true);
    }

    public static class FileEncrypter
    {
        public static string[] DecryptHeaders(string data, string pass = "");
        public static string[] DecryptString(string data, string pass = "");
        public static string EncryptString(string data, string header, string ipLink, string pass = "", string fileExtension = null);
        public static int FileIsEncrypted(string data, string pass = "");
        public static string MakeReplacementsForDisplay(string input);
        internal static string[] TestingDecryptString(string data, ushort pass);
    }

    public class DelayableActionSystem
    {
        public DelayableActionSystem(Folder sourceFolder, object osObj);
        internal DelayableActionSystem();

        internal static DelayableActionSystem FindDelayableActionSystemOnComputer(Computer c);

        public virtual void AddAction(SerializableAction action, float delay);
        public virtual void InstantlyResolveAllActions(object osObj);
    }

    public class FastDelayableActionSystem : DelayableActionSystem
    {
        public FastDelayableActionSystem(Folder sourceFolder, object osObj);

        public override void AddAction(SerializableAction action, float delay);
        public void DeserializeActions(List<FileEntry> files);
        public List<FileEntry> GetAllFilesForActions();
        public override void InstantlyResolveAllActions(object osObj);
    }

    public class HackerScriptExecuter
    {
        public const string splitDelimiter = " $#%#$\r\n";

        public static void runScript(string scriptName, object os, string sourceCompReplacer = null, string targetCompReplacer = null);
    }

    public interface FileType
    {
        string getName();
    }

    public class FileEntry : FileType
    {
        public static List<string> filenames;
        public static List<string> fileData;

        public string name;
        public string data;
        public int size;
        public int secondCreatedAt;

        public FileEntry();
        public FileEntry(string dataEntry, string nameEntry);

        public static void init(ContentManager content);

        public string getName();
        public string head();
    }

    public class Folder : FileType
    {
        public List<FileEntry> files = new List<FileEntry>();
        public List<Folder> folders = new List<Folder>();
        public string name;

        public Folder(string foldername);

        public static string deFilter(string s);
        public static string Filter(string s);
        public static Folder load(XmlReader reader);

        public bool containsFile(string name, string data);
        public bool containsFile(string name);
        public bool containsFileWithData(string data);
        public string getName();
        public string getSaveString();
        public void load(string data);
        public FileEntry searchForFile(string fileName);
        public Folder searchForFolder(string folderName);
        public string TestEqualsFolder(Folder f);
    }

    public class FileSystem
    {
        public Folder root;

        public FileSystem(bool empty);
        public FileSystem();

        public static FileSystem load(XmlReader reader);

        public void generateSystemFiles();
        public override int GetHashCode();
        public string getSaveString();
        public string TestEquals(object obj);
    }

    public class Firewall
    {
        public bool solved = false;

        public Firewall(int complexity, string solution, float additionalTime);
        public Firewall(int complexity, string solution);
        public Firewall();
        public Firewall(int complexity);

        public static Firewall load(XmlReader reader);

        public bool attemptSolve(string attempt, object os);
        public override bool Equals(object obj);
        public override int GetHashCode();
        public string getSaveString();
        public void resetSolutionProgress();
        public override string ToString();
        public void writeAnalyzePass(object os_object, object target_object);
    }

    public class ScreenManager : DrawableGameComponent
    {
        public WaveBank waveBank;
        public AudioEngine audioEngine;
        public bool usingGamePad = false;
        public Color screenFillColor = Color.Black;
        public PlayerIndex controllingPlayer;
        public SpriteFont hugeFont;
        public SoundBank soundBank;

        public SpriteFont Font { get; }
        public SpriteBatch SpriteBatch { get; }
        public bool TraceEnabled { get; set; }

        public ScreenManager(Game game);

        public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer);
        public void AddScreen(GameScreen screen);
        public string clipStringForMessageBox(string s);
        public override void Draw(GameTime gameTime);
        public void FadeBackBufferToBlack(int alpha);
        public GameScreen[] GetScreens();
        public void handleCriticalError();
        public override void Initialize();
        protected override void LoadContent();
        public void playAlertSound();
        public void RemoveScreen(GameScreen screen);
        public void ShowPopup(string message);
        protected override void UnloadContent();
        public override void Update(GameTime gameTime);
    }
}
#endif