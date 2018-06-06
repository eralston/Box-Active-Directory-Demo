using Box.V2.Models;
using BoxDemo.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BoxDemo.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        // GET: App
        public async Task<ActionResult> Index()
        {
            string email = this.GetCurrentUserEmail();
            var boxUser = await BoxHelper.GetOrCreateBoxUser(email);

            var items = await BoxHelper.UserClient(boxUser.Id).FoldersManager.GetFolderItemsAsync("0", 100);
            ViewBag.Folders = items.Entries.Where(item => item.Type == "folder");
            ViewBag.Files = items.Entries.Where(item => item.Type == "file");

            ViewBag.AccessToken = BoxHelper.UserToken(boxUser.Id);

            return View();
        }

        public async Task<ActionResult> Doc(string id)
        {
            string email = this.GetCurrentUserEmail();
            var boxUser = await BoxHelper.GetOrCreateBoxUser(email);

            var file = await BoxHelper.UserClient(boxUser.Id).FilesManager.GetInformationAsync(id);
            ViewBag.BoxFile = file;

            return View();
        }

        public async Task<ActionResult> Download(string id)
        {
            string email = this.GetCurrentUserEmail();
            var boxUser = await BoxHelper.GetOrCreateBoxUser(email);

            var downloadUrl = await BoxHelper.UserClient(boxUser.Id).FilesManager.GetDownloadUriAsync(id);
            return Redirect(downloadUrl.ToString());
        }

        public async Task<ActionResult> Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = file.FileName;
                using (var fs = file.InputStream)
                {
                    // Create request object with name and parent folder the file should be uploaded to
                    BoxFileRequest request = new BoxFileRequest()
                    {
                        Name = fileName,
                        Parent = new BoxRequestEntity() { Id = "0" }
                    };
                    string email = this.GetCurrentUserEmail();
                    var boxUser = await BoxHelper.GetOrCreateBoxUser(email);
                    var boxFile = await BoxHelper.UserClient(boxUser.Id).FilesManager.UploadAsync(request, fs);
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<FileStreamResult> Thumbnail(string id)
        {
            string email = this.GetCurrentUserEmail();
            var boxUser = await BoxHelper.GetOrCreateBoxUser(email);
            var thumbBytes = await BoxHelper.UserClient(boxUser.Id).FilesManager.GetThumbnailAsync(id, minHeight: 256, minWidth: 256, maxHeight: 256, maxWidth: 256);
            return new FileStreamResult(thumbBytes, "image/png");
        }

        public async Task<ActionResult> Preview(string id)
        {
            string email = this.GetCurrentUserEmail();
            var boxUser = await BoxHelper.GetOrCreateBoxUser(email);
            var previewUrl = await BoxHelper.UserClient(boxUser.Id).FilesManager.GetPreviewLinkAsync(id);
            return Redirect(previewUrl.ToString());
        }
    }
}