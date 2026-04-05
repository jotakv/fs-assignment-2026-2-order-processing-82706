using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;

namespace SportsStore.Components {

    public class NavigationMenuViewComponent : ViewComponent {
        private IStoreRepository repository;
        private IConfiguration configuration;

        public NavigationMenuViewComponent(IStoreRepository repo, IConfiguration config) {
            repository = repo;
            configuration = config;
        }

        public IViewComponentResult Invoke() {
            ViewBag.SelectedCategory = RouteData?.Values["category"];
            ViewBag.AdminDashboardUrl = configuration["AdminDashboard:BaseUrl"] ?? "http://localhost:5174";

            return View(repository.Products
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x));
        }
    }
}
