using Application.Features.ServiceTypes;
using Application.Features.ServiceTypes.Create;
using FluentValidation.TestHelper;

namespace ApplicationTests.Features.ServiceTypes.CreateServiceTypes;

public class CreateServiceTypeCommandValidatorTests
{
    private readonly CreateServiceTypeCommandValidator _validator;

    public CreateServiceTypeCommandValidatorTests()
    {
        _validator = new CreateServiceTypeCommandValidator();
    }
    
    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveError()
    {
        var command = new CreateServiceTypeCommand(string.Empty);
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.Errors.ShouldContain(e => e.ErrorCode == ServiceTypeErrors.NameRequired.Code);
    }

    [Fact]
    public void Validate_WhenNameLengthInRange_ShouldNotHaveError()
    {
        var longName = new string('B', 64);
        var command = new CreateServiceTypeCommand(longName);
        var result = _validator.TestValidate(command);
        
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WhenNameTooLong_ShouldHaveError()
    {
        var longName = new string('B', 65);
        var command = new CreateServiceTypeCommand(longName);
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.Errors.ShouldContain(e => e.ErrorCode == ServiceTypeErrors.NameTooLong(64).Code);
    }
}