using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lab5.Models;
using Azure.Storage.Blobs;
using Azure;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Lab5.Pages.AnswerImages
{
    public class CreateModel : PageModel
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string earthContainerName = "earthimages";
        private readonly string computerContainerName = "computerimages";

        private readonly Lab5.Data.AnswerImageDataContext _context;

        public CreateModel(Lab5.Data.AnswerImageDataContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public AnswerImage AnswerImage { get; set; }
      
        [BindProperty]
        public IFormFile file { set; get; }

        public async Task<IActionResult> OnPostAsync()
        {
            BlobContainerClient containerClient;
            // Create the container and return a container client object
            try
            {
                if (AnswerImage.Question == Question.Earth)
                {
                    containerClient = await _blobServiceClient.CreateBlobContainerAsync(earthContainerName);

                } else
                {
                    containerClient = await _blobServiceClient.CreateBlobContainerAsync(computerContainerName);
                }
                
                // Give access to public
                containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
            }
            catch (RequestFailedException)
            {
                if (AnswerImage.Question == Question.Earth)
                {
                    containerClient = _blobServiceClient.GetBlobContainerClient(earthContainerName);
                } else
                {
                    containerClient = _blobServiceClient.GetBlobContainerClient(computerContainerName);
                }
                   
            }

            Console.WriteLine("NAME: "+AnswerImage.FileName + "  URL: "+AnswerImage.Url+"   QUESTION: "+AnswerImage.Question);
            if (file == null)
            {
                Console.WriteLine("no file");
            }

            try
            {
                string randomFileName = Path.GetRandomFileName();
                
                var blockBlob = containerClient.GetBlobClient(randomFileName);
                if (await blockBlob.ExistsAsync())
                {
                    await blockBlob.DeleteAsync();
                }

                using (var memoryStream = new MemoryStream())
                {
                    // copy the file data into memory
                    await file.CopyToAsync(memoryStream);

                    // navigate back to the beginning of the memory stream
                    memoryStream.Position = 0;

                    // send the file to the cloud
                    await blockBlob.UploadAsync(memoryStream);
                    memoryStream.Close();
                }

                // add the photo to the database if it uploaded successfully
                var image = new AnswerImage
                {
                    Url = blockBlob.Uri.AbsoluteUri,
                    FileName = randomFileName,
                    Question = AnswerImage.Question
                };

                _context.AnswerImages.Add(image);
                _context.SaveChanges();
            }
            catch (RequestFailedException)
            { //redirect to error
                return RedirectToAction("./Error");
            }

            return RedirectToPage("./Index");
        }
    }
}
