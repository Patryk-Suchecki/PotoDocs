using FluentValidation;

namespace PotoDocs.Shared.Models;

public class DownloadRequest
{
    public List<Guid> Ids { get; set; } = [];
}

public class DownloadRequestValidator : AbstractValidator<DownloadRequest>
{
    public DownloadRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Lista nie może być pusta.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Lista zawiera nieprawidłowe identyfikatory.");
    }
}