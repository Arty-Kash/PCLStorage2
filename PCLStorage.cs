using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;

using PCLStorage;

namespace PCLStrg
{
    public partial class MainPage : ContentPage
    {
        //Get Root Folder Path
        IFolder IRootFolder = FileSystem.Current.LocalStorage;
        // Root Name: IRootFolder.Name
        // Root Path: IRootFolder.Path


        //Create a new IFolder "folder.iFolder" 
        //  corresponding to the Object "folder" in the IFolder "parent".
        //  (If the IFolder already exists, move there.)
        async void ICreateFolder(Object folder, IFolder iparent)
        {
            folder.iFolder =
                      await iparent.CreateFolderAsync(folder.Name, CreationCollisionOption.ReplaceExisting);
        }


        // Delete IFolder "ifolder" in IFolder "iparent"
        async void IDeleteFolder(IFolder ifolder, IFolder iparent)
        {
            // Check the existence of the IFolder corresponding to the Object "folder"
            bool IsExist = await ICheckFolderExist(ifolder, iparent);
            if( ! IsExist)
            {
                await DisplayAlert("", ifolder.Name.ToString() + " doesn't exist", "OK");
                return;
            }            

            await ifolder.DeleteAsync();
        }


        // Check existence of IFolder "ifolder" in IFolder "iparent"
        async Task<bool> ICheckFolderExist(IFolder ifolder, IFolder iparent)
        {
            ExistenceCheckResult IsExist = await iparent.CheckExistsAsync(ifolder.Name);
            if (IsExist == ExistenceCheckResult.FolderExists) return true;
            return false;
        }


        // Get All IFolders in the IFolder "iparent"
        async void IGetFolders(IFolder iparent)
        {
            IList<IFolder> folders = await iparent.GetFoldersAsync();
        }


        // Delete all of IFolders and IFiles in IFolder "iparent"
        async void IDleteAllObjects(IFolder iparent)
        {
            // Get ALl IFolders and IFiles in the IFolder "parent"
            IList<IFolder> ifolders = await iparent.GetFoldersAsync();
            IList<IFile> ifiles = await iparent.GetFilesAsync();

            // Delete All IFolders
            foreach(IFolder ifolder in ifolders)
            {
                await ifolder.DeleteAsync();
            }

            // Delete All IFiles
            foreach (IFile ifile in ifiles)
            {
                await ifile.DeleteAsync();
            }

            // Clear All Objects (Folders&Files)
            CurrentDisplayObjects.Clear();
            AllObjects.Clear();
            UpdateFinder(CurrentDisplayObjects);
        }



        //Create a new IFile "file.iFile" 
        //   corresponding to the Object "file" in the IFolder "iparent".
        //  (If the IFile already exists, open it.)
        async void ICreateFile(Object file, IFolder iparent)
        {
            file.iFile =
                    await iparent.CreateFileAsync(file.NameWithExtension, CreationCollisionOption.ReplaceExisting);
        }

        // Delete IFile "ifile" in IFolder "iparent"
        async void IDeleteFile(IFile ifile, IFolder iparent)
        {
            // Check the existence of "file"
            bool IsExist = await ICheckFileExist(ifile, iparent);
            if (!IsExist)
            {
                await DisplayAlert("", ifile.Name.ToString() + " doesn't exist", "OK");
                return;
            }

            await ifile.DeleteAsync();     // Delete the IFile "file"
        }

        // Check existence of IFile "ifile" in IFolder "iparent"
        async Task<bool> ICheckFileExist(IFile ifile, IFolder iparent)
        {
            ExistenceCheckResult IsExist = await iparent.CheckExistsAsync(ifile.Name);
            if (IsExist == ExistenceCheckResult.FileExists) return true;
            return false;
        }

        // Get All IFiles in in the IFolder "iparent"
        async void IGetFiles(IFolder iparent)
        {
            IList<IFile> ifiles = await iparent.GetFilesAsync();
        }

        // Write "content" to IFile "ifile"
        async void IWriteFile(IFile ifile, string content)
        {
            await ifile.WriteAllTextAsync(content);
        }

        // Read all text of IFile "ifile"
        async Task<string> IReadFile(IFile ifile)
        {
            string content = await ifile.ReadAllTextAsync();
            return content;
        }


        // Get All IFolders and IFiles in IFolder "parent"
        async void IGetAllObjects(IFolder parent)
        {
            await DisplayAlert("", "Get All IFolders and IFiles in parent", "OK");
        }

        // Copy, Cut, Paste IFolder
        async void ICopyFolder()
        {
            await DisplayAlert("", "Copy IFolder", "OK");
        }
        async void ICutFolder()
        {
            await DisplayAlert("", "Cut IFolder", "OK");
        }
        async void IPasteFolder()
        {
            await DisplayAlert("", "Paste IFolder", "OK");
        }

        // Copy, Cut, Paste IFile
        async void ICopyFile()
        {
            await DisplayAlert("", "Copy IFile", "OK");
        }
        async void ICutFile()
        {
            await DisplayAlert("", "Cut IFile", "OK");
        }
        async void IPasteFile()
        {
            await DisplayAlert("", "Paste IFile", "OK");
        }

    }
}
