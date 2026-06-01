using System.Collections;

namespace RentalPro.Shared;

public class Errors(IEnumerable<Error> errors) : IEnumerable<Error>
{
    private readonly List<Error> _errors = [..errors];

    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator Errors(Error[] errors)
        => new(errors);

    public static implicit operator Errors(Error error)
        => new([error]);
}