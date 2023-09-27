using Business.Concrete;
using DataAccess.Concrete.EntityFramework;
using DataAccess.Concrete.InMemory;
using System;

namespace ConsoleUI
{
    //SOLID
    //S
    //Open Closed Prince
    class Program
    {
        static void Main(string[] args)
        {
            //Data Taransformation Object
            //IoC
            ProductTest();
            //CategoryTest();
        }

        private static void CategoryTest()
        {
            CategoryManager categoryManager = new CategoryManager(new EfCategoryDal());
            foreach (var categorty in categoryManager.GetAll().Data)
            {
                Console.WriteLine(categorty.CategoryName);
            }
        }

        private static void ProductTest()
        {
            ProductManager productManager = new ProductManager(new EfProductDal(), new CategoryManager(new EfCategoryDal()));
            var result = productManager.GetProductDetail();
            if (result.Success == true)
                foreach (var product in result.Data)
                {
                    Console.WriteLine(product.ProductName + "/" + product.CategoryName);
                }
            else
                Console.WriteLine(result.Success + "\n" + result.Message);

        }
    }
}
