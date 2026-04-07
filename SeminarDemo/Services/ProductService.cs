using SeminarDemo.Models;
using SeminarDemo.Repositories;

namespace SeminarDemo.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        Product? GetProductById(int id);
        void CreateProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(int id);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Product> GetAllProducts() => _repository.GetAll();

        public Product? GetProductById(int id) => _repository.GetById(id);

        public void CreateProduct(Product product) => _repository.Add(product);

        public void UpdateProduct(Product product) => _repository.Update(product);

        public void DeleteProduct(int id) => _repository.Delete(id);
    }
}
