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
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _controller;
        private List<Product> _products;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object);
            _products = new List<Product>
            {
             new Product{Id=1,Price=100,Stock=50,Name="Pencil",Color="Red"},
             new Product{Id=2,Price=150,Stock=50,Name="Book",Color="Blue"}
            };
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnView()
        {
            var result = await _controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnProductList()
        {
            _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(_products);

            var result = await _controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var prodcutList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal<int>(2, prodcutList.Count());
        }

        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Details(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void Details_IdInValid_ReturnNotFound()
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(0)).ReturnsAsync(product);
            var result = await _controller.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async void Details_ValidId_ReturnProduct(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Details(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecute_ReturnView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreatePOST_InValidModelState_ReturnView()
        {
            _controller.ModelState.AddModelError("Name", "Name is required.");
            var result = await _controller.Create(_products.First());

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_ReturnRedirectToAction()
        {
            var result = await _controller.Create(_products.First());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product newProduct = null;
            _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<Product>()))
                .Callback<Product>(x=> newProduct =x);

            var result = await _controller.Create(_products.First());

            _mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<Product>()), Times.Once);
            Assert.Equal(_products.First().Id, newProduct.Id);
        }

        [Fact]
        public async void CreatePOST_InValidModelState_NeverCreateMethodExecute()
        {
            _controller.ModelState.AddModelError("Name", "Error test");

            var result = await _controller.Create(_products.First());
            _mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Edit(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(0)]
        public async void Edit_IdInValid_ReturnNotFound(int prodcutId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(prodcutId)).ReturnsAsync(product);
            var result = await _controller.Edit(prodcutId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        public async void Edit_ActionExecute_ReturnProdcut(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Theory]
        [InlineData(1)]
        public async void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = await _controller.Edit(2, _products.Find(x => x.Id == productId));

            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async void EditPOST_InValidModelState_ReturnViewWithProduct(int productId)
        {
            _controller.ModelState.AddModelError("name", "test error");
            var result = await _controller.Edit(productId, _products.First(x=> x.Id == productId));
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public async void EditPOST_ValidModelState_ReturnRedirectToIndexAction(int productId)
        {
            var result = await _controller.Edit(productId, _products.First(x=> x.Id == productId));
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public async void EditPOST_ValidModelState_UpdateMethodExecute(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.UpdateAsync(product));
            await _controller.Edit(productId, product);
            _mockRepo.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }
        [Fact]
        public async void Delete_IdIsNull_ReturnNotFound()
        {
            var result = await _controller.Delete(null);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        public async void Delete_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecute_ReturnView(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecute_ReturnView(int prodcutId)
        {
            var result = await _controller.DeleteConfirmed(prodcutId);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecute(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockRepo.Setup(x => x.DeleteAsync(product));
            var result = await _controller.DeleteConfirmed(productId);
            _mockRepo.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _mockRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Product>()), Times.Once);
        }
    }
}
