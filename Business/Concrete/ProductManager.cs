using Business.Abstract;
using DataAccess.Abstract;
using Entities.DTOs;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.CCS;
using Core.Utilities.Results;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.AutoFac.Validation;
using Core.CrossCuttingConcerns.Validation;
using Core.Utilities.Business;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Business.Concrete
{
    //
    //[logAspect]-->AOP
    //[Validate]
    //[RemoveCache]
    //[Transaction]
    //[Performance]
    public class ProductManager : IProductService
    {
        IProductDal _productDal;
        ICategoryService _categoryService;
        public ProductManager(IProductDal productDal, ICategoryService categoryService)
        {
            _productDal = productDal;
            _categoryService = categoryService;
        }

        [ValidationAspect(typeof(ProductValidator))]
        public IResult Add(Product product)
        {
            //business codes
            //Validation

            /* fluentvalidation kullanılmadan önce
             if (product.productname.length < 2)
             {
                 //magic strings
                 //return new errorresult("ürün ismi en az 2 karakter olmalıdır.") ;
                 return new errorresult(messages.productnameınvalid);
             }*/
            //ValidationTool.Validate(new ProductValidator(), product);

            //business codes
            IResult result = BusinessRules.Run(CheckIfProductNameExists(product.ProductName),
                    CheckIfProductCountOfCategoryCorrect(product.CategoryID),
                    CheckIfCategoryLimitExceeded());

            if (result != null)
            {
                return result;
            }
            _productDal.Add(product);
            return new SuccessResult(Messages.ProductAdded);
        }



        [ValidationAspect(typeof(ProductValidator))]
        public IResult Update(Product product)
        {

            //business codes
            //Validation


            //business codes
            if (CheckIfProductCountOfCategoryCorrect(product.CategoryID).Success)
            {
                if (CheckIfProductNameExists(product.ProductName).Success)
                {
                    _productDal.Add(product);
                    return new SuccessResult(Messages.ProductUpdated);
                }
            }

            return new ErrorResult();

        }

        public IDataResult<List<Product>> GetAll()
        {
            //İş Kodları
            //Yetkisi var mı

            if (DateTime.Now.Hour == 21)
            {
                return new ErrorDataResult<List<Product>>(Messages.MaintenanceTime);
            }
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(), Messages.ProductsListed);
        }

        public IDataResult<List<Product>> GetAllByCategoryID(int id)
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(p => p.CategoryID == id));
        }

        public IDataResult<Product> GetById(int productId)
        {
            return new SuccessDataResult<Product>(_productDal.Get(p => p.ProductID == productId));
        }

        public IDataResult<List<Product>> GetByUnitPrice(decimal min, decimal max)
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(p => p.UnitPrice >= min && p.UnitPrice <= max));
        }

        public IDataResult<List<ProductDetailDto>> GetProductDetail()
        {
            if (DateTime.Now.Hour == 22)
            {
                return new ErrorDataResult<List<ProductDetailDto>>(Messages.MaintenanceTime);
            }
            return new SuccessDataResult<List<ProductDetailDto>>(_productDal.GetProductDetail());
        }

        private IResult CheckIfProductCountOfCategoryCorrect(int categoryId)
        {
            //bir kategoride en fazla 15 ürün olabilir
            //Select Count(*) from products where categoryId=1
            var result = _productDal.GetAll(p => p.CategoryID == categoryId).Count;
            if (result >= 15)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }

            return new SuccessResult();
        }
        private IResult CheckIfProductNameExists(string productName)
        {
            Product product;
            //Aynı isimde ürün eklenemez
            var result = _productDal.GetAll(x => x.ProductName == productName).Any();
            if (result)
            {
                return new ErrorResult(Messages.ProductNameAlreadyExists);
            }

            return new SuccessResult();
        }

        private IResult CheckIfCategoryLimitExceeded()
        {
            var result = _categoryService.GetAll();
            if (result.Data.Count > 15)
            {
                return new ErrorResult(Messages.CategoryLimitExceeded);
            }

            return new SuccessResult();
        }
    }
}
