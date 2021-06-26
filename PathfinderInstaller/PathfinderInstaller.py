import os
import platform
import stat
import subprocess
from tkinter import *
from tkinter import filedialog
from tkinter.ttk import *
import shutil
from threading import Thread
import requests
from zipfile import ZipFile
from io import BytesIO
import re
import pathlib

if platform.system() == 'Windows':
    from winreg import *


def install_pathfinder(gen_event_callback, hacknet_directory):
    for asset in requests.get('https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases').json()[0]['assets']:
        if 'Pathfinder.Release' in asset['name']:
            url = asset['browser_download_url']
            break

    with ZipFile(BytesIO(requests.get(url).content)) as pathfinder_zip:
        pathfinder_zip.extractall(path=hacknet_directory)

    patcher_exe = os.path.join(hacknet_directory, 'PathfinderPatcher.exe')

    if platform.system() == "Linux":
        os.chmod(patcher_exe, stat.S_IRWXU)

    completed = subprocess.run([patcher_exe], cwd=hacknet_directory)

    if completed.returncode != 0:
        gen_event_callback('<<InstallFailure>>')
        return

    try:
        os.remove(patcher_exe)
        os.remove(os.path.join(hacknet_directory, 'Mono.Cecil.dll'))
        hacknet_exe = os.path.join(hacknet_directory, 'Hacknet.exe')
        os.rename(hacknet_exe, os.path.join(hacknet_directory, 'HacknetOld.exe'))
        os.rename(os.path.join(hacknet_directory, 'HacknetPathfinder.exe'), hacknet_exe)
    except OSError:
        gen_event_callback('<<InstallFailure>>')
        return

    gen_event_callback('<<InstallComplete>>')


def try_find_hacknet_dir():
    def get_library_folders(vdf_path):
        with open(vdf_path) as vdf:
            match = re.search(r'^\s*"[0-9]+"\s*"(.+)"', vdf.read(), flags=re.MULTILINE)
            if match is None:
                return []
            return match.groups()
    hacknet_dir = ''

    folders = []

    if platform.system() == 'Windows':
        try:
            registry = ConnectRegistry(None, HKEY_LOCAL_MACHINE)
            key = OpenKey(registry, r'SOFTWARE\Wow6432Node\Valve\Steam')
            root_steamapps = os.path.join(QueryValueEx(key, 'InstallPath')[0], 'steamapps')

            folders.append(root_steamapps)
            libraries = get_library_folders(os.path.join(root_steamapps, 'libraryfolders.vdf'))
            folders.extend([os.path.join(library, 'steamapps') for library in libraries])
        except OSError:
            return hacknet_dir
    else:
        home = pathlib.Path.home()
        steam_root = None
        possible_roots = [
            os.path.join(home, '.local', 'share', 'Steam'),
            os.path.join(home, '.steam', 'steam'),
            os.path.join(home, '.steam', 'root'),
            os.path.join(home, '.steam'),
            os.path.join(home, '.var', 'app', 'com.valvesoftware.Steam', '.local', 'share', 'steam'),
            os.path.join(home, '.var', 'app', 'com.valvesoftware.Steam', '.steam', 'steam'),
            os.path.join(home, '.var', 'app', 'com.valvesoftware.Steam', '.steam', 'root'),
            os.path.join(home, '.var', 'com.valvesoftware.Steam', '.steam')
        ]
        for dir in possible_roots:
            if not os.path.exists(dir) or not os.path.exists(os.path.join(dir, 'steam.sh')):
                continue
            steam_root = dir
            break
        if steam_root is None:
            return hacknet_dir
        possible_steamapps = [
            os.path.join(steam_root, 'steamapps'),
            os.path.join(steam_root, 'steam', 'steamapps'),
            os.path.join(steam_root, 'root', 'steamapps')
        ]
        root_steamapps = None
        for possible_steamapp in possible_steamapps:
            if os.path.exists(possible_steamapp):
                root_steamapps = possible_steamapp
                break
        if root_steamapps is None:
            return hacknet_dir
        folders.append(root_steamapps)
        libraries = get_library_folders(os.path.join(root_steamapps, 'libraryfolders.vdf'))
        for library in libraries:
            for possible_steamapp in possible_steamapps:
                if os.path.exists(os.path.join(library, possible_steamapp)):
                    folders.append(possible_steamapp)

    for folder in folders:
        hacknet_acf = os.path.join(folder, 'appmanifest_365450.acf')
        if not os.path.exists(hacknet_acf):
            continue
        hacknet_dir_candidate = os.path.join(folder, 'common', 'Hacknet')
        hacknet_exe = os.path.join(hacknet_dir_candidate, 'Hacknet.exe')
        if not os.path.exists(hacknet_dir_candidate) or not os.path.exists(hacknet_exe):
            continue
        hacknet_dir = hacknet_dir_candidate

    return hacknet_dir


class App(Frame):
    def __init__(self, master: Tk):
        super().__init__(master)

        self.master = master
        self.master.bind('<<InstallComplete>>', self.install_complete)
        self.master.bind('<<InstallFailure>>', self.install_failure)

        self.content = Frame(self.master)

        self.file_frame = Frame(self.content)
        self.dir_label = Label(self.file_frame, text='Hacknet Folder')
        self.hacknet_directory = StringVar()
        self.hacknet_directory.set(try_find_hacknet_dir())
        self.dir = Entry(self.file_frame, textvariable=self.hacknet_directory)
        self.reopen_button = Button(self.file_frame, text='Open Directory Select', command=self.open_dir)

        self.button_frame = Frame(self.content)
        self.install_button = Button(self.button_frame, text='Install', command=self.install)
        self.uninstall_button = Button(self.button_frame, text='Uninstall', command=self.uninstall)

        self.setup_grid()

        self.progress = None

    def setup_grid(self):
        self.master.title('Pathfinder Installer')
        self.master.geometry("750x75")
        self.master.resizable(FALSE, FALSE)

        self.content.grid(column=0, row=0, sticky='NSEW')

        self.file_frame.grid(column=0, row=0, sticky='NSEW')
        self.dir_label.grid(column=0, row=0, padx=(5, 0))
        self.dir.grid(column=1, row=0, columnspan=2, padx=5, sticky='EW')
        self.reopen_button.grid(column=3, row=0, padx=(0, 5))

        self.button_frame.grid(column=0, row=1, pady=(0, 5))
        self.install_button.grid(column=0, row=0, padx=(0, 20))
        self.uninstall_button.grid(column=1, row=0, padx=(20, 0))

        self.master.columnconfigure(0, weight=1)
        self.master.rowconfigure(0, weight=1)
        self.content.columnconfigure(0, weight=1)
        self.content.rowconfigure(0, weight=1)
        self.content.rowconfigure(1, weight=1)
        self.file_frame.columnconfigure(1, weight=1)
        self.file_frame.rowconfigure(0, weight=1)

    def open_dir(self):
        self.hacknet_directory.set(filedialog.askdirectory())

    def install(self):
        hacknet_dir = self.hacknet_directory.get()
        if not self.valid_directory(hacknet_dir):
            return

        self.progress = Progressbar(self.button_frame, orient=HORIZONTAL, length=500, mode='indeterminate')
        self.progress.grid(column=0, row=0, columnspan=2)
        self.progress.start()
        Thread(target=install_pathfinder, args=(self.master.event_generate, hacknet_dir)).start()

    def install_complete(self, event):
        self.make_message_box('Installation Complete!', title='Success')
        self.progress.destroy()
        self.progress = None

    def install_failure(self, event):
        self.make_message_box('Installation failed, this may have left an unfinished installation in your Hacknet folder!', title='Failure')
        self.progress.destroy()
        self.progress = None
        return

    def uninstall(self):
        hacknet_dir = self.hacknet_directory.get()

        if not self.valid_directory(hacknet_dir):
            return
        hacknet_exe_path = os.path.join(hacknet_dir, 'Hacknet.exe')
        old_hacknet_path = os.path.join(hacknet_dir, 'HacknetOld.exe')
        if not os.path.exists(old_hacknet_path):
            self.make_message_box('Could not find OldHacknet.exe, are you sure Pathfinder is installed (and was installed by this installer)?', title='Error!')
            return
        try:
            os.remove(hacknet_exe_path)
            os.rename(old_hacknet_path, hacknet_exe_path)
            shutil.rmtree(os.path.join(hacknet_dir, 'BepInEx'), ignore_errors=True)
        except OSError:
            self.make_message_box('Failed to clean up all files, you may be left with an incomplete uninstall!', title='Error!')

        self.make_message_box('Pathfinder successfully uninstalled', title='Success')

    def valid_directory(self, directory):
        valid = True
        if not os.path.exists(directory):
            valid = False
            self.make_message_box(f'The directory {directory} does not exist!', title='Error!')
        elif not os.path.exists(os.path.join(directory, 'Hacknet.exe')):
            valid = False
            self.make_message_box(f'The directory {directory} does not contain a file called Hacknet.exe!', title='Error!')
        return valid

    def make_message_box(self, message, title='Message'):
        message_box = Toplevel(self.master)
        message_box.resizable(FALSE, FALSE)
        message_box.title(title)
        message_frame = Frame(message_box)
        message_frame.grid()
        Label(message_frame, text=message).grid(column=0, row=0, padx=5, pady=5)
        Button(message_frame, text='Ok', command=message_box.destroy).grid(column=0, row=1, pady=5)


root = Tk()
app = App(root)
root.mainloop()
