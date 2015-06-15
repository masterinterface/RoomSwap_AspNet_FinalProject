// Azure blob storage 
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WannaSwapWebRole

    // From http://aspalliance.com/1114_Understanding_the_Globalasax_file
    // The Global.asax file (also known as the ASP.NET application file) is an optional file that
    // This file exposes the application and session level events in ASP.NET 
    // It also provides a gateway to all the application and the session level events in ASP.NET. 
    // Can be used to implement the important application and session level events such as Application_Start. 

{
    public class MvcApplication : System.Web.HttpApplication
    {
        
        protected void Application_Start()
        {
            // Scaffolding generated code
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // call method to create, name, verify, and set permissions for the storage blob and its container.  
            // see method below
            startStorage();
        }

        // Instructions from: http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs
        private void startStorage()
        {

            // The following code will open storage account
            // taking the StorageConnectionString credentials from ServiceDefinition.cscfg file.
            var storageAccount = CloudStorageAccount.Parse
                (RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));


            // Instructions for this section from: https://cmatskas.com/working-with-azure-blobs-through-the-net-sdk/

            // this creates the blob client
            var blobClient = storageAccount.CreateCloudBlobClient();

            // gets a reference to a blob container
            // If the name (here: "images") doesn’t match these rules => will get a 400 error (bad request)
            var imagesBlobContainer = blobClient.GetContainerReference("images");

            // if the container doesn't exist - this code creates it 
            if (imagesBlobContainer.CreateIfNotExists())
            {
                // This allows public access ( the client's browser ) to the "images" container.
                imagesBlobContainer.SetPermissions(
                    
                    // The BlobContainerPermissions class being 'public' allows us to access the blob container via a URL. 
                    // if this were private - no access via URL 
                    new BlobContainerPermissions
                    {
                        PublicAccess =BlobContainerPublicAccessType.Blob
                    });
            }
        }
     }
}

