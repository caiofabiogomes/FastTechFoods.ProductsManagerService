﻿using FastTechFoods.ProductsManagerService.Application.InputModels;
using FastTechFoods.ProductsManagerService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastTechFoods.ProductsManagerService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _productService;
       

        public ProductController(IProductService productService)
        {
            this._productService = productService;
            
        }
       

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateOrEditProductInputModel product)
        {
            try
            {
                 var result = await _productService.CreateProductAsync(product);                
                 return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }               
        }     


        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(CreateOrEditProductInputModel product)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(product);                
                return Ok(result);
               
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
