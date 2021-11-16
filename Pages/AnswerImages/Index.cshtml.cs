using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Lab5.Data;
using Lab5.Models;

namespace Lab5.Pages.AnswerImages
{
    public class IndexModel : PageModel
    {
        private readonly Lab5.Data.AnswerImageDataContext _context;

        public IndexModel(Lab5.Data.AnswerImageDataContext context)
        {
            _context = context;
        }

        public IList<AnswerImage> AnswerImage { get;set; }

        public async Task OnGetAsync()
        {
            AnswerImage = await _context.AnswerImages.ToListAsync();
        }
    }
}
