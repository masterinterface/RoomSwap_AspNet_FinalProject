using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// This Controller is the 'central' coding piece of my 'WannaSwap' web app. 

// for access to DbContext class
using WannaSwapWebRole.Models;

// Entity Framework Libraries 
using System.Data;
using System.Data.Entity;

// Azure blob storage 
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.ServiceRuntime;

// for data stream to blob storage
using System.IO;

//using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.Diagnostics;
using System.Net;
using System.Configuration;

namespace WannaSwapWebRole.Controllers
{
    public class AdvertController : Controller
    {
        // calling the 'no-arg' constructor of DbContext. 
        // 'db' is now our link to the database
        private WannaSwapContext db = new WannaSwapContext();

        // We will be using methods from the CloudBlobContainer class to upload images to storage
        // More about thee class here: https://msdn.microsoft.com/en-us/library/microsoft.windowsazure.storage.blob.cloudblobcontainer.aspx
        private static CloudBlobContainer img;
        
        // GET: Advert
        // this is the default method for the Advert Controller
        // note the the Enums can be null, hence the '?' => refer to Advert Model for more on all my Enums
        public ActionResult Index(int? location, int? age, int? noise, int? tidy, int? babel, int? state)
        {
            // retreive a list of all Advert objects in the database
            // these will be displayed in the Index View usuing a for loop
            var advertList = db.Adverts.AsQueryable();

            // the following if statements check if any (one or zero) of the params are not null
            // if one if not null then the list of adverts is narrowed down to adverts that ONLY match that category
            // for eg if noise = 2 ==> only adverts for 'quiet' rooms will be included in the list
            if (location != null)
            {
                // (Location)location => this casts the 'location' integer back into a 'Location' Enum which can then be compared
                advertList = advertList.Where(a => a.Location == (Location)location);
            }

            else if (age != null)
            {
                advertList = advertList.Where(a => a.Age == (Age)age);
            }

            else if (noise != null)
            {
                advertList = advertList.Where(a => a.Noise == (Noise)noise);
            }

            else if (tidy != null)
            {
                advertList = advertList.Where(a => a.Tidy == (Tidy)tidy);
            }

            else if (babel != null)
            {
                advertList = advertList.Where(a => a.Babel == (Babel)babel);
            }

            else if (state != null)
            {
                advertList = advertList.Where(a => a.State == (State)state);
            }

            // Model binding allows me to pass the advertList to the Index View
            return View(advertList.ToList());
        }


        // this method will take in an 'id' integer, representing the Advert Id value, and passes an Ad object to the 'Details' View
        public ActionResult Details(int? id)
        {
            // if 'id' is null then I return a 404 bad request message to the client's browser 
            if(id == null)
            {
                // returns a http status code 400 to client : https://msdn.microsoft.com/en-us/library/system.net.httpstatuscode%28v=vs.110%29.aspx
                return new HttpStatusCodeResult(400);
            }

            // 'Find(int)' is a method from DbSet<> used to find a row with a certain id value
            Advert advert = db.Adverts.Find(id);

            // check that returned value is not null
            if(advert == null)
            {
                // equivalent to http status code 404 
                return HttpNotFound();
            }
            // here the method returns the Details view and passes an 'ad' object to it.
            return View(advert);
        }

        // GET: Url = Wannaswap.ie/advert/create
        public ActionResult Create()
        {
            // user authenitication for custuom outcome (using 'if' statement) 
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return View("Auth");
            }
            // returns the 'Create' View that I will scaffold through ASP.NET
            return View();
        }

        // POST: Url = Wannaswap.ie/advert/create
        // this method carries the same name as the one above
        // however, here it receives the 'Advert' object posted back from the form
        // the routing engine sees the [HttpPost] attribute blow and knows to use this method
        [HttpPost]
        // user authentication => redirects to /Account/Login if not signed in
        [Authorize]
        public ActionResult Create(
            // here I define the input parameters that we receive back from the Client
            // these are: 1 Advert object (minus certain fields) and 1 image
            //as the server defines the 'postedOn and 'imgURL' fields, we can make sure these are not taken in.  
            [Bind(Exclude="ImgURL, PostedOn")] Advert advert,
            // more info on how to upload files in ASP.NET here : http://haacked.com/archive/2010/07/16/uploading-files-with-aspnetmvc.aspx/
            HttpPostedFileBase imgFile
            )
        {
            // creating new Blob instance for storing the image
            CloudBlockBlob blob = null;

            // back-end validation of Model - checks that inputs are all valid (before storing in database)
            // remember that the 'Create' View has front-end @ValidationMessageFor,
            // and the Advert model defines to what degree fields need to be validated
            // see 'Interesting discoveries' in my project log
            if(ModelState.IsValid)
            {
                // check that file has content
                // to avoid NullpointerExceptions I check that the imgFile is not null before I check its length
                // if it is null then program control doesn't check the 2nd comparison
                if (imgFile != null && imgFile.ContentLength > 0)
                {
                    // do not accept imagess larger than 10 mb : http://stackoverflow.com/questions/6388812/how-to-validate-uploaded-file-in-asp-net-mvc
                    if (imgFile.ContentLength > 10 * 1024 * 1024)
                    {
                        return Content("Image File Too Large. Max allowed size = 10 MB !");
                    }

                    // at this point we know there's an image to be stored, and it meets our max size criteria
                    // so lets start up a connection to Azure Blob storage account!
                    startStorage();

                    // call method to upload the image to Blob storage
                    // assign returned the CloudBlockBlob 
                    blob = storeInBlob(imgFile);

                    // so that we can now extract the URL and store to database
                    // from "Step 10 – Finding the BLOB URI": http://www.codeproject.com/Articles/490178/How-to-Use-Azure-Blob-Storage-with-Azure-Web-Sites
                    advert.ImgURL = blob.Uri.ToString();
                }
                // if the user has not supplied an image (allowed) then program control skips the above code and proceeds from here:

                // create a reference to when the new 'Advert' object was created 
                // as specified above, we excluded this field from input params, because in is created on the back-end (here!) 
                advert.PostedOn = DateTime.Now;

                // add & save the new 'Advert' object to the database (these are methods provided by DbContext.DbSet<>)
                // Note: I haven't assigned a value to the 'Id' property because this is assigned incrementally by the database 
                db.Adverts.Add(advert);
                db.SaveChanges();

                // return a message informing the Client that Advert and data has been saved
                return View("Success");
            }
            // here, this is the 'unsuccessfull' path - meaning that the Model 'ViewSate' was not validated correctly
            // we pass back the Client data (contained in the 'advert' object POSTed to us)
            // so the the user does not have to enter it all in again 
            return View(advert);  
        }

        // GET: Advert/Edit/integer
        // this method returns the advert object data to the Client 
        // so that it may be modified and sent back and received in the following POST 
        
        // User authentication => redirects to /Account/Login if not signed in
        [Authorize]
        public ActionResult Edit(int? id)
        {
            // we need an Id corresponding to the Advert to look up in the database
            if (id != null)
            {
                // instantiate a new Advert object and assign it to the returned value from database
                Advert advert = db.Adverts.Find(id);

                // pass the returned Advert object to the 'Edit' View ==> for the Client to modify data
                return View(advert);
            }
            // http stauts code 400 (Bad Request) is retured to Client
            return new HttpStatusCodeResult(400);
        }
        
        // POST: Advert/Edit
        [HttpPost]
        // User authentication => redirects to /Account/Login if not signed in
        [Authorize]
        public ActionResult Edit(
            // the back-end will update the PostedOn filed to reflect the Edit date.
            [Bind(Exclude = "PostedOn")] Advert advert, 
            HttpPostedFileBase imgFile
            )
        {
            // back-end validation again (see 'Create' method)
            if(ModelState.IsValid)
            {
                // check if the user has submitted a new image (not necessary)
                if(imgFile !=null && imgFile.ContentLength > 0)
                {
                    // open connection to Blob storage account
                    startStorage();
                    // creating new Blob instance for storing the new image
                    CloudBlockBlob blob = null;
                    // call the method to remove the old image blob
                    AdvertController.removeBlob(advert);
                    // upload the new image
                    blob = storeInBlob(imgFile);
                    // update the image URL reference in the database
                    advert.ImgURL = blob.Uri.ToString();
                    
                    // back-end updating of the 'postedOn' field (remember that this is 'excluded' above) 
                    advert.PostedOn = DateTime.Now;
                }
                
                // load the old database values
                var old = db.Adverts.Find(advert.Id);

                // exception handling 
                if(old != null)
                {
                    old.ImgURL = advert.ImgURL;
                    old.Location = advert.Location;
                    old.Noise = advert.Noise;
                    old.PhoneNumber = advert.PhoneNumber;
                    old.PostedOn = advert.PostedOn;
                    old.Rent = advert.Rent;
                    old.State = advert.State;
                    old.Text = advert.Text;
                    old.Tidy = advert.Tidy;

                    //save the changes to DbContext instance properties to the database
                    db.SaveChanges();
                    // confirm update to the user 
                    return View("Success");
                }
            }
            return View(advert);
        }


        // User authentication => redirects to /Account/Login if not signed in
        [Authorize]
        // this is the GET 'Delete' method that return the object to be deleted for confiration
        public ActionResult Delete(int? id)
        {
            if(id != null)
            {
                // get the advert object from the database
                Advert advert = db.Adverts.Find(id);

                // and pass it to the 'Delete' View
                return View(advert);
            }
            // return 400 bad request to browser if 'id' is null 
            return new HttpStatusCodeResult(400);
        }

        // User authentication => redirects to /Account/Login if not signed in
        [Authorize]
        // Here the user is confirming the object to be deleted by posting it back
        [HttpPost]
        public ActionResult Delete(int id)
        {
            //find the Advert by 'Id' in the database
            Advert advert = db.Adverts.Find(id);

            startStorage();

            // call the methode to remove to blob storage for the photo
            removeBlob(advert);

            // mark the Advert entity as Deleted so that it will be deleted from the database when SaveChanges is called.
            // more information on the DbSet.Remove(Object entity) method: https://msdn.microsoft.com/en-us/library/system.data.entity.dbset.remove(v=vs.113).aspx
            db.Adverts.Remove(advert);

            // save cahnges to the database
            db.SaveChanges();

            // return a confirmation to the user 
            return View("Success");
        }
        
        // method to return Category Search view
        public ActionResult CategorySearch()
        {
            return View();
        }

        // method to handle negative user authentication
        public ActionResult Auth()
        { 
            return View("Auth");
        }

        // method to store image file in Azure Blob storage
        private CloudBlockBlob storeInBlob(HttpPostedFileBase imgFile)
        {
            // the following code is taken from "Step 9 – Save the Image to a BLOB": http://www.codeproject.com/Articles/490178/How-to-Use-Azure-Blob-Storage-with-Azure-Web-Site
            
            // here a string formatter is used to append 0 (Guid) to 1 (the image file name)
            string uniqueBlobName = string.Format("{0}{1}",
            // Guid = positon 0 | Path = position 1
            Guid.NewGuid().ToString(), Path.GetExtension(imgFile.FileName));
            
            // get back a full reference to the Blob 
            CloudBlockBlob blob = img.GetBlockBlobReference(uniqueBlobName);
            
            // within the blob there is now an indicator showing that it is an image
            blob.Properties.ContentType = imgFile.ContentType;

            // this is the key stage - uploading the HttpPostedFileBase to the newly created CloudBlockBlob 
            blob.UploadFromStream(imgFile.InputStream);

            // return a pointer ==> for later finding where image is stored 
            return blob;
        } // end of storeInBlob() method

        // Note: the code in this method establishes a connection to the Blob storage.
        //       While similar looking, the code in the Global.asax check that there's a storage account
        //       BUT, most-importantly, it creates the storage account if none created before. 
        private void startStorage()
        {
            // this will look in ServiceDefinition.csfg file (in WannaSwapCloudService project)
            // for details on how to open the storage account  
            var storageAccount = CloudStorageAccount.Parse
                (RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            // Instructions for this section from: https://cmatskas.com/working-with-azure-blobs-through-the-net-sdk/

            // this creates the blob client
            var blobClient = storageAccount.CreateCloudBlobClient();

            // gets a reference to a blob container
            // If the name (here: "images") doesn’t match these rules => will get a 400 error (bad request)
            img = blobClient.GetContainerReference("images");
            
        } // end of startStorage() method 

        private static void removeBlob(Advert advert)
        {
            // nullPointerException handling : check that ImgUrl is not null 
            if(advert.ImgURL != null)
            {

                // How to fetch the blob using its URL. Guide found online : http://stackoverflow.com/questions/19723609/can-i-get-a-blob-by-using-its-url            
                
                // get the connectionString for the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

                // Here I use the Constructor 'CloudBlockBlob(Uri, StorageCredentials)' to initialise a new instance of the CloudBlockBlob class using an absolute URI to the blob.
                // convert the imgUrl string into a Uri and use the storage account connectionString credentials.
                CloudBlockBlob removeThisBlob = new CloudBlockBlob(new Uri(advert.ImgURL),storageAccount.Credentials);

                // remove the Blob
                removeThisBlob.Delete();
            }
        }
    }   
}




