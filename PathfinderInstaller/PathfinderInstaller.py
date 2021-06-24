import os
from tkinter import *
from tkinter import filedialog
import shutil


class App(Frame):
    def __init__(self, master: Tk):
        super().__init__(master)

        self.master = master

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

        self.make_message_box('Needs to be finished', title='aaaaaaaaaa')

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
