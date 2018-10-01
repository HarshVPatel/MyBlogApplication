using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Helpers;
using Blog.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using PagedList.Mvc;


namespace Blog.Controllers
{
    [RequireHttps]

    public class BlogPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BlogPosts
        public ActionResult Index(int? page, string searchString)
        {

            int pageSize = 1; // display three blog posts at a time on this page
            int pageNumber = (page ?? 1);

            var postQuery = db.Posts.OrderBy(p => p.Created).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                postQuery = postQuery
                    .Where(p => p.Title.Contains(searchString) ||
                                p.Body.Contains(searchString) ||
                                p.Slug.Contains(searchString) ||
                                p.Comments.Any(t => t.Body.Contains(searchString))
                           ).AsQueryable();
            }

            var postList = postQuery.ToPagedList(pageNumber, pageSize);

            ViewBag.SearchString = searchString;

            return View(postList);
        }

       

        // GET: BlogPosts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Details/5
        public ActionResult DetailsSlug(string slug)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts
                .Include(p => p.Comments.Select(t => t.Author))
                .Where(p => p.Slug == slug)
                .FirstOrDefault();
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View("Details", blogPost);
        }

        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }
        // POST: BlogPosts/Details/5
        [HttpPost]
        public ActionResult DetailsSlug(string slug, string body)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blogPost = db.Posts
                .Where(p => p.Slug == slug)
                .FirstOrDefault();

            if (blogPost == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                ViewBag.ErrorMessage = "Comment is required";
                return View("Details", blogPost);
            }

            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.BlogPostId = blogPost.Id;
            comment.Created = DateTime.Now;
            comment.Body = body;

            db.Comments.Add(comment);
            db.SaveChanges();

            return RedirectToAction("DetailsSlug", new { slug = slug });
        }

      
        // POST: BlogPosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Id,Title,Body,MediaUrl,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                blogPost.Slug = StringUtilities.URLFriendly(blogPost.Title);

                if (db.Posts.Any(p => p.Slug == blogPost.Slug))
                {
                    ModelState.AddModelError(nameof(BlogPost.Title), "This slug already exist");
                    return View(blogPost);
                }

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogPost.MediaUrl = "/Uploads/" + fileName;
                }


                db.Posts.Add(blogPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Slug,Body,MediaUrl,Published")] BlogPost blogPost)
        {
            if (ModelState.IsValid)
            {
                var blog = db.Posts.Where(p => p.Id == blogPost.Id).FirstOrDefault();

                blog.Body = blogPost.Body;
                blog.MediaUrl = blogPost.MediaUrl;
                blog.Published = blogPost.Published;
                blog.Slug = blogPost.Slug;
                blog.Title = blogPost.Title;
                blog.Updated = DateTime.Now;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPost blogPost = db.Posts.Find(id);
            db.Posts.Remove(blogPost);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize]
        public ActionResult CreateComment(string slug, string body)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blogPost = db.Posts
                .Where(p => p.Slug == slug)
                .FirstOrDefault();

            if (blogPost == null)
            {
                return HttpNotFound();
            }
            if (string.IsNullOrWhiteSpace(body))
            {
                TempData["ErrorMessage"] = "Comment is required";
                return RedirectToAction("DetailsSlug", new { slug = slug });
            }


            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.BlogPostId = blogPost.Id;
            comment.Created = DateTime.Now;
            comment.Body = body;

            db.Comments.Add(comment);
            db.SaveChanges();

            return RedirectToAction("DetailsSlug", new { slug = slug });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
