using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestExample.Web.Controllers;
using UnitTestExample.Web.Models;
using UnitTestExample.Web.Repository;
using Xunit;

namespace UnitTestExample.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _controller;
        private List<Product> _products;

        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);
            _products = new List<Product>
            {
             new Product{Id=1,Price=100,Stock=50,Name="Pencil",Color="Red"},
             new Product{Id=2,Price=150,Stock=50,Name="Book",Color="Blue"}
            };
        }
        [Fact]
        public async void GetProducts_ActionExecutes_ReturnOkResultWithProduct()
        {
            _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(_products);
            var result = await _controller.GetProducts();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnProduct = Assert.IsAssignableFrom<List<Product>>(okResult.Value);
            Assert.Equal(2, returnProduct.Count);
        }
        [Theory]
        [InlineData(0)]
        public async void GetProduct_IdInValid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.GetProduct(productId);
            var notFound = Assert.IsType<NotFoundResult>(result);
        }
        [Theory]
        [InlineData(2)]
        public async void GetProduct_IdValid_ReturnOkResult(int productId)
        {
            var product = _products.Find(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.GetProduct(productId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultProduct = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(productId, resultProduct.Id);
        }
        [Theory]
        [InlineData(1)]
        public async void PutProduct_IdIsNotEqualValid_ReturnBadRequest(int productId)
        {
            var product = _products.Find(x => x.Id == productId);
            var result = await _controller.PutProduct(2, product);
            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void PutProduct_ActionExecute_ReturnNoContent(int productId)
        {
            var product = _products.Find(x => x.Id == productId);
            _mockRepo.Setup(x => x.UpdateAsync(product));
            var result = await _controller.PutProduct(productId, product);
            _mockRepo.Verify(x => x.UpdateAsync(product), Times.Once);
            var noContentResult = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecute_ReturnCreatedAction()
        {
            var product = _products.First();
            _mockRepo.Setup(x => x.CreateAsync(product)).Returns(Task.CompletedTask);
            var result = await _controller.PostProduct(product);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            _mockRepo.Verify(x => x.CreateAsync(product), Times.Once);
            Assert.Equal("GetProduct", createdAtActionResult.ActionName);
        }

        [Theory]
        [InlineData(0)]
        public async void DeleteProduct_IdInValid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.DeleteProduct(productId);
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_ActionExecute_ReturnNoContent(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockRepo.Setup(x => x.DeleteAsync(product)).Returns(Task.CompletedTask);
            var result = await _controller.DeleteProduct(productId);
            _mockRepo.Verify(x => x.GetByIdAsync(productId), Times.Once);
            _mockRepo.Verify(x => x.DeleteAsync(product), Times.Once);
            var noContentResult = Assert.IsType<NoContentResult>(result);
        }
    }
}
