namespace ruler.Features
{
    using System.Threading.Tasks;

    public interface ITokenService
    {
        ValueTask<bool> IsValidToken(string token);
    }
}