using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using PCLStorage;

namespace PCLStrg
{
	public partial class MainPage : ContentPage
	{
        // Define Object (Folder/File) class
        class Object : INotifyPropertyChanged
        {            
            public string Name { get; set; }    // Name of Object
            public string NameWithExtension     // If File, add "txt" extention to the name
            {
                get { return IsFolder ? Name : Name+".txt"; }
            }

            public bool IsFolder { get; set; }  // Folder -> true, File -> false

            // Notify the change of IsExpanded
            private bool isexpanded;
            public event PropertyChangedEventHandler PropertyChanged;
            public bool IsExpanded
            {
                get { return this.isexpanded; }
                set
                {
                    if (this.isexpanded != value)
                    {
                        this.isexpanded = value;
                        OnPropertyChanged("IsExpanded");
                        OnPropertyChanged("Icon");
                    }
                }
            }
            protected virtual void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            
            public Object Parent { get; set; }  // Parent Folder of this Object
            
            public int NoChildren { get; set; } // Number of Children in this Object            
            public string NoChildrenText        // Text of NoChildren
            {
                get { return IsFolder ? "(" + NoChildren.ToString() + ")" : ""; }
            }
            
            public string Path { get; set; }    // Directory Path
            
            public string Color                 // Text Color: Folder->Blue, File->Black
            {
                get { return IsFolder ? "Blue" : "Black"; }
            }
            
            public string Icon          // Icon: Expanded Folder -> expanded.png
                                        //       Collapsed Folder -> collapsed.png
                                        //       File -> file-hyphen.png
            {
                get { return IsFolder ? (IsExpanded ? "expanded.png" : "collapsed.png") : "file-hyphen.png"; }
            }
            
            public int Depth { get; set; }  // Depth in Folder Structure            
            public int Indent               // Indent Display Position according to the Depth
            {
                get { return Depth * 15; }
            }
            
            public string OpenAction    // ContextAction Menu: Folder->Open, File->Edit
            {
                get { return IsFolder ? "Open" : "Edit"; }
            }

            // PCL Storage: IFolder/IFile of this Object
            public IFolder iFolder { get; set; }
            public IFile iFile {get; set;}

        } // End of "class Object"

        ObservableCollection<Object> CurrentDisplayObjects
            = new ObservableCollection<Object>();
        ObservableCollection<Object> AllObjects
            = new ObservableCollection<Object>();

        Object CurrentFolder = new Object();

        static Object RootFolder = new Object
        {
            Name = "Root",
            IsFolder = true,
            IsExpanded = false,
            Parent = null,
            Path = "Root>",
            Depth = 0
        };
        

        public MainPage()
        {
            InitializeComponent();            

            CreateRootFolder();
            UpdateFinder(CurrentDisplayObjects);            
        }


        // Resize the ListView "Finder" by expanding the StackLayout "FinderBase" size
        // Two methods, 1 and 2, below are effective to do it, but 
        //    iOS: Both of 1 and 2
        //    UWP: Only 2             are effective respectively.
        /*
        // 1
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // Set "Footer" space at the bottom of ListView.
            // Tap the "Footer" space, Pop up menu of some commands
            Footer.HeightRequest = BackGround.Height - 100;

            // Fit the size of ListView "Finder" to the space            
            FinderBase.HeightRequest = BackGround.Height;
            FinderBase.WidthRequest = BackGround.Width;
        }
        */
        // 2
        void SetFinderSize(object sender, EventArgs e)
        {
            // Set "Footer" space at the bottom of ListView.
            // Tap the "Footer" space, Pop up menu of some commands
            Footer.HeightRequest = BackGround.Height - 100;

            // Fit the size of ListView "Finder" to the space
            FinderBase.HeightRequest = BackGround.Height;
            FinderBase.WidthRequest = BackGround.Width;
        }


        // Create the RootFolder in the root directory of LocalStorage
        void CreateRootFolder()
        {
            CurrentFolder = RootFolder;

            // PCL Storage: Create the IFolder of RootFolder (RootFolder.iFolder)
            ICreateFolder(RootFolder, IRootFolder);
        }


        // Display all updated Objects on the ListView "Finder"
        void UpdateFinder(ObservableCollection<Object> objects)
        {
            // Bind the sorted Objects to ListView "Finder"
            CurrentDisplayObjects = SortObject(objects);
            Finder.ItemsSource = CurrentDisplayObjects;

            // Display the Current Path
            CurrentFolderEntry.Text = CurrentFolder.Path;

            // In RootFolder, disable BackToParent Button
            if (CurrentFolder == RootFolder)
            {
                BackToParentButton.TextColor = Color.Yellow;
                BackToParentButton.IsEnabled = false;
            }
            else
            {
                BackToParentButton.TextColor = Color.Black;
                BackToParentButton.IsEnabled = true;
            }              
        }

        
        // Sort all Objects (Folders&Files) in a Folder
        ObservableCollection<Object> SortObject(ObservableCollection<Object> objects)
        {
            ObservableCollection<Object> Folders = new ObservableCollection<Object>();
            ObservableCollection<Object> Files = new ObservableCollection<Object>();
            ObservableCollection<Object> subObjects = new ObservableCollection<Object>();
            ObservableCollection<Object> sortedObjects = new ObservableCollection<Object>();

            // Get minimum Depth of the Objects
            int minDepth = int.MaxValue;
            foreach(Object o in objects)
            {
                if (minDepth > o.Depth) minDepth = o.Depth;
            }
            
            // Separate Folders&Files with the Depth of minDepth
            foreach(Object o in objects)
            {
                if(o.Depth==minDepth)
                {
                    if (o.IsFolder) Folders.Add(o);
                    else            Files.Add(o);
                }
            }

            // Sort Folders&Files by Name respectively
            sortedObjects = new ObservableCollection<Object>(Folders.OrderBy(n => n.Name));
            Files = new ObservableCollection<Object>(Files.OrderBy(n => n.Name));            

            // Recursively sort all subObject located within all Expanded Folders
            foreach(Object o in Folders)            
            {
                if(o.IsExpanded)
                {
                    // Collect all objects whose parent is "o"
                    foreach(Object oChild in AllObjects)
                    {
                        if (oChild.Parent == o) subObjects.Add(oChild);
                    }

                    // Recursive call: 
                    //      Sort the subObjects located within this object "o"
                    subObjects = SortObject(subObjects);
                    
                    // Insert the sorted subObjects below its parent object "o"
                    int i = sortedObjects.IndexOf(o)+1;
                    foreach(Object sub_o in subObjects)
                    {
                        sortedObjects.Insert(i, sub_o);
                        i++;
                    }
                }
            }

            // Finally, add Files to the end of sortedObjects
            foreach(Object o in Files)
            {
                sortedObjects.Add(o);
            }

            return sortedObjects;
        }


        // Expand/Collapse Folder
        void FolderExpandCollapse(object sender, EventArgs e)
        {
            // Get index of Folder whose IsExpanded was switched
            int index = CurrentDisplayObjects.IndexOf(
                ( (Object)( (Button)sender ).CommandParameter )    );

            // Toggle switch IsExpanded
            CurrentDisplayObjects[index].IsExpanded = !CurrentDisplayObjects[index].IsExpanded;

            UpdateFinder(CurrentDisplayObjects);            
        }


        // Pop up Menu to create a new Object
        void PopUpCreateObjectMenu(object sender, EventArgs args)
        {
            // default: create Folder
            isFolder = true;
            FolderButton.TextColor = Color.Black;
            FileButton.TextColor = Color.LightGray;

            PopUpMenu.IsVisible = true;     // Pop up Create-Folder-Menu

            // Set Entry "ObjectName" Keyboard
            //      Plain: No Characterize and Spell Check
            ObjectName.Keyboard = Keyboard.Plain;
            
            ObjectName.Focus();     // Set cursor on Entry "ObjectName"
        }


        // Select Folder/File to be created
        bool isFolder = new bool();        
        void CreateFolder(object sender, EventArgs e)   // Folder Create Mode
        {
            isFolder = true;
            FolderButton.TextColor = Color.Black;
            FileButton.TextColor = Color.LightGray;
        }        
        void CreateFile(object sender, EventArgs e)     // File Create Mode
        {
            isFolder = false;
            FolderButton.TextColor = Color.LightGray;
            FileButton.TextColor = Color.Black;
        }

        // OK Button pressed, and create a new Object
        void OKCreateObject(object sender, EventArgs e)
        {
            Object newObject = new Object
            {
                Name = ObjectName.Text,
                IsFolder = isFolder, 
                IsExpanded = false,
                Parent = CurrentFolder,
                Depth = CurrentFolder.Depth + 1
            };
            
            // Create a new Object with the random name, if no Object's name input
            if (newObject.Name == "")                
                newObject.Name = Guid.NewGuid().ToString("N").Substring(0, 3);

            // Check the existence of newObject
            foreach(Object o in CurrentDisplayObjects)
            {
                if(o.Parent == CurrentFolder) // Has the same Parent?
                {
                    if(o.IsFolder==isFolder)  // Folder or File?
                    {
                        if (o.Name == ObjectName.Text){
                            DisplayAlert("", o.NameWithExtension + " already exists!", "OK");
                            return;
                        }
                    }
                }
            }


            // Update Path name
            newObject.Path = newObject.Parent.Path + newObject.Name + ">";
            if (newObject.Path.Length > 50) // If Path name is too long, shorten it.
            {
                string p = newObject.Path.Substring(newObject.Path.Length - 45);
                newObject.Path = "..." + p;
            }
            
            newObject.Parent.NoChildren++;  // Increment the number of children

            // Add newObject to the both of the displayed Objecs and all Objects
            CurrentDisplayObjects.Add(newObject);
            AllObjects.Add(newObject);

            UpdateFinder(CurrentDisplayObjects);

            // Clear the text in ObjectName Entry and focus it again
            ObjectName.Text = "";
            ObjectName.Focus();


            // PCL Storage: Create a new IFolder/IFile corresponding to the newObject
            if (isFolder) ICreateFolder(newObject, CurrentFolder.iFolder);
            else ICreateFile(newObject, CurrentFolder.iFolder);

        }

        // Press Cancel Button, hide the PopUp Menu
        void CancelCreateObject(object sender, EventArgs e)
        {
            PopUpMenu.IsVisible = false;
        }

        
        // Folder/File is selected
        void ObjectSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Object selectedObject = (Object)e.SelectedItem;

            /* Switch Displaying Buttons for processing folders/files ? */            
        }       


        // Folder/File is double clicked
        async void ObjectDoubleTapped(object sender, EventArgs e)
        {
            Label tappedLabel = (Label)sender;
            

            // Identify the tapped object
            foreach ( Object o in CurrentDisplayObjects )
            {
                if(o.NameWithExtension == tappedLabel.Text)
                {
                    if(o.IsFolder)  // If Folder, move to there
                    {
                        CurrentFolder = o;
                        break;
                    }
                    else            // If File, open it
                    {
                        openedFile = o;
                        FileEditorTitle.Text = "File Name: " + o.NameWithExtension;
                        FileEditor.Keyboard = Keyboard.Plain;
                        PopUpEditor.IsVisible = true;
                        FileEditor.Text = await IReadFile(o.iFile);
                        //IOpenFile(o, (Editor)FileEditor);
                        return;
                    }
                }                
            }
            CurrentDisplayObjects = GetCurrentDisplayObjects(CurrentFolder);
            UpdateFinder(CurrentDisplayObjects);
        }


        // Get all display Objects in Folder 
        ObservableCollection<Object> GetCurrentDisplayObjects(Object folder)
        {
            ObservableCollection<Object> NewDisplayObjects 
                            = new ObservableCollection<Object>();

            // Get all objects whose parent is tappedObject
            foreach (Object o in AllObjects)
            {
                if (o.Parent == folder)
                {                    
                        NewDisplayObjects.Add(o);
                }
            }            
            return NewDisplayObjects;
        }


        // Back to the Parent Folder
        void BackToParent(object sender, EventArgs e)
        {
            // If in RootFolder, don' do anything
            if (CurrentFolder == RootFolder) return;

            // Change the CurrentFolder
            CurrentFolder = CurrentFolder.Parent;
            CurrentDisplayObjects = GetCurrentDisplayObjects(CurrentFolder);
            UpdateFinder(CurrentDisplayObjects);
        }

        
        // ContextActions: Open/Edit, Delete, Others
        Object openedFile = new Object();
        async void OpenObject(object sender, EventArgs e)
        {
            Object o = (Object)((MenuItem)sender).CommandParameter;
            openedFile = o;
            
            if (o.IsFolder) // If Folder, move to there
            {
                CurrentFolder = o;
                CurrentDisplayObjects = GetCurrentDisplayObjects(CurrentFolder);
                UpdateFinder(CurrentDisplayObjects);
            }
            else            // If File, open it
            {
                FileEditorTitle.Text = "File Name: " + o.NameWithExtension; 
                FileEditor.Keyboard = Keyboard.Plain;
                PopUpEditor.IsVisible = true;
                FileEditor.Text = await IReadFile(o.iFile);
            }
        }

        void DelObject(object sender, EventArgs e)
        {
            Object o = (Object)((MenuItem)sender).CommandParameter;
            CurrentDisplayObjects.Remove(o);
            UpdateFinder(CurrentDisplayObjects);

            // PCL Storage: Delete IFolder/IFile of "o"
            if (isFolder) IDeleteFolder(o.iFolder, o.Parent.iFolder);
            else IDeleteFile(o.iFile, o.Parent.iFolder);
        }

        // Pop Up Others Processing Menu including Copy, Cut, Paste, Rename
        void OtherObject(object sender, EventArgs e)
        {

        }

        // Buttons of PopUpEditor Menu: save File, cancel Editor
        void SaveFile(object sender, EventArgs e)
        {
            // PCL Storage: Write content in IFile
            IWriteFile(openedFile.iFile, FileEditor.Text);
        }
        void CancelEditor(object sender, EventArgs e)
        {
            PopUpEditor.IsVisible = false;
        }

        // 
        void FooterClicked(object sender, EventArgs e)
        {
            
        }


    } // End of ™public partial class MainPage : ContentPage™
} // End of "namespace PCLStrg"
