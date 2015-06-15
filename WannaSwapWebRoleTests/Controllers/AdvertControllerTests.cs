using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WannaSwapWebRole.Controllers;
// reference the Model entity (Advert) form my Web Role
using WannaSwapWebRole.Models;

// reference the Unit Testing library 
using Microsoft.VisualStudio.TestTools.UnitTesting;

// reference the Controllers (we're testing these) from my Web Role
namespace WannaSwapWebRole.Controllers.Tests
{
    // attribute necessary for VS2013 to recognize the the class is for testing purposes
    [TestClass()]
    // name of main test class
    public class AdvertControllerTests
    {
        // attribute necessary for VS2013 to recognize the the method is for testing purposes
        [TestMethod()]
        // name of test method
        public void AuthTest()
        {
            // create an instance of the AdvertContoller so that we can call use its fields and methods
            var controller = new AdvertController();
            // call the 'Auth' method and cast its returned ActionResult
            var result = controller.Auth() as ViewResult;
            // Check if names are equal as expected
            Assert.AreEqual("Auth", result.ViewName);
        }



        [TestMethod()]
        public void DetailsTest()
        {
            var controller = new AdvertController();
            // call the 'Details' method with parameter '2' 
            var result = controller.Details(2) as ViewResult;
            // extract only the retured 'Advert' object from the above method's result
            var advert = (Advert)result.ViewData.Model;
            // check if Ids are equal as expected
            Assert.AreEqual(2, advert.Id);
        }

        [TestMethod()]
        public void IndexTest()
        {
            // test incorrect??
            var controller = new AdvertController();
            var result = controller.Index(1,null,null,null,null,null) as ViewResult;
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod()]
        public void EditTest()
        {
            var controller = new AdvertController();
            // call the 'Edit' method with a null parameter ('Edit' needs a non null param to work !) 
            var result = controller.Edit(null);
            // check that system handles the exception and returns status to user's browser
            Assert.IsInstanceOfType(result, typeof(HttpStatusCodeResult));
        }
    }
}

