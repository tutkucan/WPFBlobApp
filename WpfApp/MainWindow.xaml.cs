using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.Azure;// Namespace for CloudConfigurationManager
using System.Text.RegularExpressions;
namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string fileFilter = "*.jpg";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string DirectoryPath=@"d:\";
            using (var myDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(DirectoryPath))
                {
                    myDialog.SelectedPath = DirectoryPath;
                }
                myDialog.ShowDialog();
                DirectoryPath = myDialog.SelectedPath;

            }

            var directories = Directory.GetDirectories(DirectoryPath);

            StringBuilder directoryBuilder = new StringBuilder();
            StringBuilder imageBuilder = new StringBuilder();
            StringBuilder resultBuilder = new StringBuilder();

            foreach (var directory in directories)
            {
                directoryBuilder.AppendLine(directory);
               var directoryImages = Directory.GetFiles(directory, fileFilter);
          
                foreach (var image in directoryImages)
                {
                    CreateBlobs(directory, image, resultBuilder);
                    imageBuilder.AppendLine(image);
                }
            }
            Directories.Text = directoryBuilder.ToString();
            Images.Text = imageBuilder.ToString();
            result.Text = resultBuilder.ToString();

        }
        private void CreateBlobs(string containerPath, string blobPath, StringBuilder result)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client. Enables you to retrieve containers and blobs stored in Blob storage
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(GetAppropriateNames(Path.GetFileName(containerPath)));

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            //container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(Path.GetFileName(blobPath));

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = File.OpenRead($@"{blobPath}"))
            {
                blockBlob.UploadFromStream(fileStream);
            }
            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in container.ListBlobs(null, true))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    result.AppendLine($"Block blob of length {blob.Properties.Length} : {blob.Uri}");
                }
            }
            

        }
        private string GetAppropriateNames(string fileName)
        {
            string message = "";
            string pattern = @"^(?!.*\-\-)[A-Za-z0-9][A-Za-z0-9\-]{2,63}$";
            Regex regex = new Regex(pattern);

            if (!regex.IsMatch(fileName))
            {
                message = "Container names must start with a letter or number, and can contain only letters, numbers, and the dash (-) character.";
                MessageBoxResult result = MessageBox.Show(message, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                Environment.Exit(0);
            }

            return fileName.ToLower().Trim();//All letters in a container name must be lowercase.
        }


    }
}
