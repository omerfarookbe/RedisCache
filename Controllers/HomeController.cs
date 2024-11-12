using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RedisCache.Data;
using RedisCache.Models;
using System.Diagnostics;

namespace RedisCache.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _distributedCache;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IDistributedCache distributedCache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public IActionResult Index()
        {
            List<Category> categories= new();
            var cachedcategories = _distributedCache.GetString("categories");
            if (!string.IsNullOrEmpty(cachedcategories))
            {
                categories = JsonConvert.DeserializeObject<List<Category>>(cachedcategories);
                //cache
            }
            else
            {
                categories = _dbContext.Category.ToList();
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions();
                cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                _distributedCache.SetString("categories", JsonConvert.SerializeObject(categories), cacheOptions);
            }

            return View(categories);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
