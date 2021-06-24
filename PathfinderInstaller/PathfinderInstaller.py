import os
import platform
import subprocess
from tkinter import *
from tkinter import filedialog
from tkinter.ttk import *
import shutil
from threading import Thread
import requests
from zipfile import ZipFile
from io import BytesIO
from winreg import *


def install_pathfinder(gen_event_callback, hacknet_directory):
    url = requests.get('https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases').json()[0]['assets'][0]['browser_download_url']

    with ZipFile(BytesIO(requests.get(url).content)) as pathfinder_zip:
        pathfinder_zip.extractall(path=hacknet_directory)

    patcher_exe = os.path.join(hacknet_directory, 'PathfinderPatcher.exe')

    if platform.system() == 'Windows':
        completed = subprocess.run([patcher_exe], cwd=hacknet_directory)
    else:
        completed = subprocess.run(['mono', patcher_exe], shell=True)

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
