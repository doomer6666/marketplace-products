using FluentValidation;
using Marketplace.Products.Application.DTOs;

namespace Marketplace.Products.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Weight).GreaterThan(0);
    }
}