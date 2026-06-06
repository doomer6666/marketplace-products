using FluentValidation;
using Marketplace.Products.Application.DTOs;

namespace Marketplace.Products.Application.Validators;

public class ProductFilterDtoValidator : AbstractValidator<ProductFilterDto>
{
    public ProductFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.MinPrice >= 0 && x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice and greater then 0");
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        RuleFor(x => x.Category)
            .IsInEnum()
            .When(x => x.Category.HasValue);
    }
}