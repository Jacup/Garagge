using Application.Features.ServiceTypes;
using Application.Features.ServiceTypes.Create;
using Application.Features.ServiceTypes.Update;
using FluentValidation.TestHelper;

namespace ApplicationTests.Features.ServiceTypes.UpdateServiceTypes;

public class UpdateServiceTypeCommandValidatorTests
{
    private readonly UpdateServiceTypeCommandValidator _validator;

    public UpdateServiceTypeCommandValidatorTests()
    {
        _validator = new UpdateServiceTypeCommandValidator();
    }
    
    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveError()
    {
        var command = new UpdateServiceTypeCommand(Guid.Empty, "ABC");
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(c => c.Id);
        result.Errors.ShouldContain(e => e.ErrorCode == ServiceTypeErrors.IdRequired.Code);
    }
    
    [Fact]
    public void Validate_WhenIdIsNotEmpty_ShouldNotHaveError()
    {
        var command = new UpdateServiceTypeCommand(Guid.NewGuid(), "ABC");
        var result = _validator.TestValidate(command);
        
        result.ShouldNotHaveValidationErrorFor(c => c.Id);
    }
    
    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveError()
    {
        var command = new UpdateServiceTypeCommand(Guid.NewGuid(), string.Empty);
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.Errors.ShouldContain(e => e.ErrorCode == ServiceTypeErrors.NameRequired.Code);
    }

    [Fact]
    public void Validate_WhenNameLengthInRange_ShouldNotHaveError()
    {
        var longName = new string('B', 64);
        var command = new UpdateServiceTypeCommand(Guid.NewGuid(), longName);
        var result = _validator.TestValidate(command);
        
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WhenNameTooLong_ShouldHaveError()
    {
        var longName = new string('B', 65);
        var command = new UpdateServiceTypeCommand(Guid.NewGuid(), longName);
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.Errors.ShouldContain(e => e.ErrorCode == ServiceTypeErrors.NameTooLong(64).Code);
    }
}