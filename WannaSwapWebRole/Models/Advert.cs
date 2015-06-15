using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// This will allow both front and back end validation (when user 'creates' a new Advert)
// From: https://msdn.microsoft.com/en-us/library/dd901590(VS.95).aspx
// The data annotation attributes fall into three categories: 
// validation attributes, display attributes, and data modeling attributes. 
using System.ComponentModel.DataAnnotations;

namespace WannaSwapWebRole.Models
{

    // Enumeration types: https://msdn.microsoft.com/en-us/library/vstudio/cc138362(v=vs.110).aspx
    // I am going to use an enum because:
    // In Visual Studio, IntelliSense lists the defined values
    // this will help when working on the Conroller
    public enum Location {Center, Suburb}
    public enum Age {Old, Young}
    public enum Noise {Social, Quiet}
    public enum Tidy {Clean, Relaxed}
    public enum Babel {Language, Normal}
    public enum State {Active, HomeLovers}
    
    // With Entity Framework (Code First) this class will become a table in the database
    public class Advert
    {
        //
        public int Id { get; set; }

        //Validation here:
        //this makes it an obligatory field
        [Required]
        //this makes sure that the string !> 80 chars
        [StringLength(80)]
        public string Caption { get; set; }

        // an integer vlue for the monthly rent
        public int Rent { get; set; }

        // will store the date that user created Advert
        // Date display/validation here: http://www.codeproject.com/Questions/847925/how-to-convert-date-format-like-dd-mm-yyyy-in-mvc
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PostedOn { get; set; }

        // the enums (these will allow to narrow down by category)
        public Location? Location { get; set; }
        public Age? Age { get; set; }
        public Noise? Noise { get; set; }
        public Tidy? Tidy { get; set; }
        public Babel? Babel { get; set; }
        public State? State { get; set; }

        // a string value to store phone number
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        // a string value to store the blob URL pointing to the image
        [Display(Name = "Image")]
        [StringLength(2000)]
        public string ImgURL { get; set; }

        // a string value to store the description of Room
        [Display(Name="Discribe your room!")]
        [StringLength(2000)]
        public string Text { get; set; }


    }
}